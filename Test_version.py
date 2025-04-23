import os
import re
from datetime import datetime, timedelta
from dotenv import load_dotenv
from fastapi import FastAPI
from pydantic import BaseModel
from openai import AzureOpenAI

# Load environment variables
load_dotenv()
azure_oai_endpoint = os.getenv("AZURE_OAI_ENDPOINT")
azure_oai_key = os.getenv("AZURE_OAI_KEY")
azure_oai_deployment = os.getenv("AZURE_OAI_DEPLOYMENT")
azure_search_endpoint = os.getenv("AZURE_SEARCH_ENDPOINT")
azure_search_key = os.getenv("AZURE_SEARCH_KEY")
azure_search_index = os.getenv("AZURE_SEARCH_INDEX")

# OpenAI clients
rag_client = AzureOpenAI(
    base_url=f"{azure_oai_endpoint}/openai/deployments/{azure_oai_deployment}/extensions",
    api_key=azure_oai_key,
    api_version="2023-09-01-preview"
)

chat_client = AzureOpenAI(
    base_url=f"{azure_oai_endpoint}/openai/deployments/{azure_oai_deployment}",
    api_key=azure_oai_key,
    api_version="2023-09-01-preview"
)

# FastAPI app + in-memory session state
app = FastAPI()
user_memory = {}

class QueryRequest(BaseModel):
    user_id: str
    question: str

@app.get("/")
async def root():
    return {"message": "VR Kitchen AI is running!"}

@app.post("/ask")
async def ask_question(request: QueryRequest):
    user_id = request.user_id
    question = request.question.strip()
    question_lower = question.lower()

    # Clean up expired sessions
    now = datetime.utcnow()
    timeout_seconds = 600  # 10 minutes

    expired_users = [
        uid for uid, mem in user_memory.items()
        if "last_active" in mem and (now - mem["last_active"]).total_seconds() > timeout_seconds
    ]
    for uid in expired_users:
        print(f"[INFO] Memory for user {uid} expired due to inactivity.")
        del user_memory[uid]

    # Initialize memory if new session
    if user_id not in user_memory:
        user_memory[user_id] = {
            "active_recipe": None,
            "active_risk": None,
            "context": None,
            "chat_history": [],
            "last_active": now
        }

    memory = user_memory[user_id]
    memory["last_active"] = now  # update activity timestamp

    # STEP VALIDATION MODE ("what now?")
    if question_lower.endswith("what now?"):
        active_context = memory["context"]
        if active_context not in ["recipe", "risk"]:
            return {"response": "❓ You're not following a recipe or scenario right now."}

        flow = memory[f"active_{active_context}"]
        steps = flow["steps"]
        index = flow["current_step_index"]

        if index >= len(steps):
            memory[f"active_{active_context}"] = None
            if active_context == "risk" and memory["active_recipe"]:
                memory["context"] = "recipe"
                return {"response": "✅ Risk scenario complete. Resuming your recipe...\n" +
                                    f"Next step: {memory['active_recipe']['steps'][memory['active_recipe']['current_step_index']]}"}            
            return {"response": "✅ All steps completed. Great job!"}

        expected_step = steps[index]
        cleaned_expected = re.sub(r"\[.*?\]", "", expected_step).strip()
        next_step = steps[index + 1] if index + 1 < len(steps) else None

        prompt = (
            "You are a step-following assistant in a VR kitchen simulation.\n"
            f"✅ Expected step:\n{cleaned_expected}\n"
            f"{'👉 Next step:\n' + next_step if next_step else 'This is the final step.'}\n"
            f"🗣️ Player said:\n{question}\n\n"
            "Your job is to check if the player did the correct step.\n"
            "- If yes, confirm and provide the next step.\n"
            "- If not, explain what they still need to do.\n"
            "DO NOT invent or reword steps."
        )

        gpt_response = chat_client.chat.completions.create(
            model=azure_oai_deployment,
            temperature=0,
            max_tokens=300,
            messages=[
                {"role": "system", "content": "You are a VR cooking assistant. Only use memory. Never invent steps."},
                {"role": "user", "content": prompt}
            ]
        )

        reply = gpt_response.choices[0].message.content.strip()

        # Fuzzy match for backend validation
        q_words = set(question_lower.split())
        e_words = set(cleaned_expected.lower().split())
        match_ratio = len(q_words & e_words) / max(len(e_words), 1)

        if match_ratio >= 0.5:
            flow["completed_steps"].append(expected_step)
            flow["current_step_index"] += 1

        print(f"[DEBUG] Memory for {user_id}:\n{user_memory}")
        return {"response": reply}

    # RAG MODE for knowledge or starting new recipe/risk
    extension_config = {
        "dataSources": [{
            "type": "AzureCognitiveSearch",
            "parameters": {
                "endpoint": azure_search_endpoint,
                "key": azure_search_key,
                "indexName": azure_search_index,
            }
        }]
    }

    # Maintain last 10 turns of chat
    history = memory.setdefault("chat_history", [])[-10:]
    history.append({"role": "user", "content": question})

    messages = [{"role": "system", "content": "You are a helpful kitchen assistant. Use documents to answer questions or give step-by-step instructions."}]
    messages.extend(history)

    rag_response = rag_client.chat.completions.create(
        model=azure_oai_deployment,
        temperature=0.5,
        max_tokens=1000,
        messages=messages,
        extra_body=extension_config
    )

    content = rag_response.choices[0].message.content.strip()
    history.append({"role": "assistant", "content": content})
    memory["chat_history"] = history

    print("[DEBUG] RAG response:\n", content)

    # Try to extract steps if it's a recipe or scenario
    steps = re.findall(r"\d+\.\s+(.*)", content)
    if not steps:
        steps = [line.strip("-• ").strip() for line in content.split("\n") if line.strip()]

    if len(steps) >= 2:
        tag = "risk" if any(word in question_lower for word in ["fire", "spill", "emergency", "contamination"]) else "recipe"
        memory[f"active_{tag}"] = {
            "title": question,
            "steps": steps,
            "current_step_index": 0,
            "completed_steps": [],
        }
        memory["context"] = tag
        print(f"[DEBUG] Initialized {tag} memory for {user_id}:\n", user_memory)
        return {"response": f"✅ Got it! Let's begin.\nFirst step: {steps[0]}"}

    return {"response": content}
