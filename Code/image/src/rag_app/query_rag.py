from dataclasses import dataclass
from typing import List
from langchain.prompts import ChatPromptTemplate
from langchain_aws import ChatBedrock
from .get_chroma_db import get_chroma_db
from .session_model import SessionModel

# The template for the AI assistant's prompt
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

# Data structure to hold the query and the response
@dataclass
class QueryResponse:
    query_text: str
    response_text: str
    sources: List[str]

def query_rag_with_session(query_text: str, session: SessionModel) -> QueryResponse:
    # Get the Chroma database for retrieving relevant documents
    db = get_chroma_db()

    # Retrieve relevant documents using similarity search
    results = db.similarity_search_with_score(query_text, k=3)
    context_text = "\n\n---\n\n".join([doc.page_content for doc, _ in results])

    # Get the conversation history from the session model
    history = session.get_conversation_context()

    # Create a prompt with the conversation history and the relevant context
    prompt_template = ChatPromptTemplate.from_template(PROMPT_TEMPLATE)
    prompt = prompt_template.format(history=history, context=context_text, question=query_text)

    # Use the Bedrock model to generate a response based on the prompt
    model = ChatBedrock(model_id=BEDROCK_MODEL_ID)
    response = model.invoke(prompt)
    response_text = response.content

    # Collect sources from the documents used in the context
    sources = [doc.metadata.get("id", None) for doc, _ in results]

    # Update the session with the new query and response
    session.add_to_history(query_text, response_text)

    # Return the query response
    return QueryResponse(
        query_text=query_text,
        response_text=response_text,
        sources=sources,
    )