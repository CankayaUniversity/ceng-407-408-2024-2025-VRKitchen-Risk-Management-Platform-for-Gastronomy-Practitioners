from dataclasses import dataclass
from typing import List
from langchain.prompts import ChatPromptTemplate
from langchain_aws import ChatBedrock
from .get_chroma_db import get_chroma_db
from .session_model import SessionModel
from query_model import QueryModel
import re

PROMPT_TEMPLATE = """
You are an AI assistant with memory. Use the following conversation history and context to answer:

Conversation History:
{history}

Relevant Context:
{context}

---

User: {question}
AI:
"""

BEDROCK_MODEL_ID = "anthropic.claude-3-haiku-20240307-v1:0"

PDF_SOURCES = {
    "Cross contamination in the game.pdf",
    "General Fire In The Game.pdf",
    "Steak and Fries in the Game.pdf",
    "Rice and Chicken in the Game.pdf",
    "Pasta with Tomato Sauce In the Game.pdf"
    "Water Spill In the Game.pdf"
}

RECIPE_KEYWORDS = {
    "steak": "Steak and Fries in the Game.pdf",
    "fries": "Steak and Fries in the Game.pdf",
    "pasta": "Pasta with Tomato Sauce In the Game.pdf",
    "chicken": "Rice and Chicken in the Game.pdf",
    "rice": "Rice and Chicken in the Game.pdf",
    "contamination": "Cross contamination in the game.pdf",
    "fire": "General Fire In The Game.pdf",
    "spill" : "Water Spill In the Game.pdf"
}

@dataclass
class QueryResponse:
    query_text: str
    response_text: str
    sources: List[str]

def extract_steps(text: str):
    lines = [line.strip() for line in text.split("\n") if line.strip()]
    return [line for line in lines if re.match(r"^\d+[\.\)\-]?\s+", line)]

def is_step_progress_query(question: str) -> bool:
    triggers = [
        "what now", "what’s the next step", "what is the next step",
        "what should i do next", "then what", "next step", "continue"
    ]
    return any(p in question.lower() for p in triggers)

def query_rag_with_session(query_text: str, session: SessionModel) -> QueryResponse:
    db = get_chroma_db()
    results = db.similarity_search_with_score(query_text, k=5)
    context_text = "\n\n---\n\n".join([doc.page_content for doc, _ in results])
    history = session.get_conversation_context()
    lower_q = query_text.lower()

    if is_step_progress_query(query_text):
        if session.active_context == "risk":
            flow = session.active_risk_flow
        elif session.active_context == "recipe":
            flow = session.active_recipe_flow
        else:
            return QueryResponse(
                query_text=query_text,
                response_text="You are not following any recipe or risk scenario right now.",
                sources=[]
            )

        if not flow or not flow["steps"]:
            return QueryResponse(
                query_text=query_text,
                response_text="There are no steps in your current flow.",
                sources=[]
            )

        index = flow["current_step_index"]
        steps = flow["steps"]

        # === AUTO-RESUME RECIPE IF RISK COMPLETES ===
        if index >= len(steps):
            if flow == session.active_risk_flow and session.active_recipe_flow:
                if session.active_recipe_flow["current_step_index"] < len(session.active_recipe_flow["steps"]):
                    session.active_context = "recipe"
                    session.save()
                    next_step = session.active_recipe_flow["steps"][session.active_recipe_flow["current_step_index"]]
                    session.add_to_history(query_text, f"Step complete. Next: {next_step}")
                    return QueryResponse(
                        query_text=query_text,
                        response_text=f"Step complete. Next: {next_step}",
                        sources=[]
                    )
            session.active_context = None
            session.save()
            return QueryResponse(
                query_text=query_text,
                response_text="All steps completed. Great job!",
                sources=[]
            )

        expected_step = steps[index]
        cleaned_expected = re.sub(r"\[.*?\]", "", expected_step).strip()
        next_step = steps[index + 1] if index + 1 < len(steps) else "All steps complete."

        validation_prompt = (
            "You are an AI assistant in a VR kitchen simulation. Your task is to check if the player followed the step exactly.\n\n"
            f"Expected step:\n{cleaned_expected}\n\n"
            f"Player said:\n{query_text}\n\n"
            "Only respond in one of the following formats:\n"
            "Step complete.\n Next: [next step]\n"
            "Incorrect.\n Current step: [expected step]\n\n"
            "Do NOT rephrase or summarize. Do NOT include any extra text. Respond with a single line."
        )

        model = ChatBedrock(model_id=BEDROCK_MODEL_ID)
        validation_response = model.invoke(validation_prompt)
        reply = validation_response.content.strip()

        q_words = set(lower_q.split())
        e_words = set(cleaned_expected.lower().split())
        match_ratio = len(q_words & e_words) / max(len(e_words), 1)

        if "Step complete" in reply or match_ratio >= 0.5:
            flow["completed_steps"].append(expected_step)
            flow["current_step_index"] += 1

            if "restart the recipe" in next_step.lower():
                session.active_context = None
                session.active_recipe_flow = session._empty_flow()

            session.add_to_history(query_text, f"Step complete. Next: {next_step}")
            session.save()
            return QueryResponse(query_text=query_text, response_text=f"Step complete. Next: {next_step}", sources=[])

        session.add_to_history(query_text, reply)
        session.save()
        return QueryResponse(query_text=query_text, response_text=reply, sources=[])

    # === GENERAL QUERY HANDLING ===
    prompt = ChatPromptTemplate.from_template(PROMPT_TEMPLATE).format(
        history=history, context=context_text, question=query_text
    )
    model = ChatBedrock(model_id=BEDROCK_MODEL_ID)
    response = model.invoke(prompt)
    response_text = response.content

    # === STRICT STEP FLOW FROM PDFS ONLY ===
    verified_docs = [
        doc for doc, _ in results
        if doc.metadata.get("source", "").split("/")[-1] in PDF_SOURCES
    ]
    verified_context = "\n\n---\n\n".join(doc.page_content for doc in verified_docs)
    verified_steps = extract_steps(verified_context)

    # === FALLBACK: MATCH BY KEYWORD IF CHROMA FAILS ===
    fallback_file = None
    for keyword, filename in RECIPE_KEYWORDS.items():
        if keyword in lower_q:
            fallback_file = filename
            break

    if not verified_steps and fallback_file:
        all_docs = db._collection.get(include=["metadatas", "documents"])
        matched_chunks = [
            (doc, meta) for doc, meta in zip(all_docs["documents"], all_docs["metadatas"])
            if meta.get("source", "").endswith(fallback_file)
        ]
        if matched_chunks:
            verified_context = "\n".join(doc for doc, _ in matched_chunks)
            verified_steps = extract_steps(verified_context)
            results = [(doc, 0.0) for doc, _ in matched_chunks]

    if len(verified_steps) >= 1:
        is_risk = any(word in lower_q for word in ["fire", "contamination", "spill", "emergency", "hazard"])
        flow_data = {
            "title": query_text,
            "steps": verified_steps,
            "current_step_index": 0,
            "completed_steps": [],
        }
        if is_risk:
            session.active_context = "risk"
            session.active_risk_flow = flow_data
        else:
            session.active_context = "recipe"
            session.active_recipe_flow = flow_data

        session.save()
        return QueryResponse(
            query_text=query_text,
            response_text="\n".join(verified_steps),
            sources=[getattr(doc, "metadata", {}).get("id", None) for doc, _ in results]
        )

    # === NON-FLOW RESPONSE ===
    if "in the game" in lower_q:
        response_text = "\n".join(extract_steps(response_text))

    session.add_to_history(query_text, response_text)
    session.save()
    sources = [getattr(doc, "metadata", {}).get("id", None) for doc, _ in results]

    QueryModel(
        query_text=query_text,
        answer_text=response_text,
        sources=sources,
        session_id=session.session_id,
        is_complete=True
    ).put_item()

    return QueryResponse(query_text=query_text, response_text=response_text, sources=sources)

