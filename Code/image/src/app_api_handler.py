import os
import uvicorn
from fastapi import FastAPI
from pydantic import BaseModel
from typing import Optional
from mangum import Mangum
from rag_app.query_rag import query_rag_with_session
from rag_app.session_model import SessionModel

app = FastAPI()
handler = Mangum(app)  # Entry point for AWS Lambda.

# In-memory session store
sessions = {}

class SessionQueryRequest(BaseModel):
    query_text: str
    session_id: Optional[str] = None

@app.get("/")
def index():
    return {"Hello": "World"}

@app.post("/query_with_memory")
def query_with_memory(request: SessionQueryRequest):
    session_id = request.session_id or "default_session"

    # Retrieve or create session
    if session_id not in sessions:
        sessions[session_id] = SessionModel(session_id)

    session = sessions[session_id]

    # Process query with memory
    response = query_rag_with_session(request.query_text, session)

    return {
        "query_text": response.query_text,
        "response_text": response.response_text,
        "sources": response.sources,
        "session_id": session_id,
    }

if __name__ == "__main__":
    port = 8000
    print(f"Running FastAPI server on port {port}.")
    uvicorn.run("app_api_handler:app", host="0.0.0.0", port=port)