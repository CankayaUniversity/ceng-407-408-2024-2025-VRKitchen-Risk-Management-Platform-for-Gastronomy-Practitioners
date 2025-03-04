import os
import json
from query_model import QueryModel
from rag_app.query_rag import query_rag_with_session
from rag_app.session_model import SessionModel

# Initialize the session store (in-memory for local testing)
sessions = {}

def handler(event, context):
    """Lambda handler function."""
    print("Received event:", event)
    
    # Assuming `event` contains necessary fields for the query model
    query_item = QueryModel(**event)

    # Retrieve or create session using the session_id from the query
    session = get_or_create_session(query_item.session_id)

    # Process the query with the RAG model
    response = invoke_rag(query_item, session)

    # Return the response to the caller (API Gateway, etc.)
    return {
        "statusCode": 200,
        "body": json.dumps({
            "query_text": response.query_text,
            "response_text": response.response_text,
            "sources": response.sources
        })
    }

def get_or_create_session(session_id: str) -> SessionModel:
    """Retrieve or create a session for the given session_id."""
    if session_id not in sessions:
        sessions[session_id] = SessionModel(session_id)
    return sessions[session_id]

def invoke_rag(query_item: QueryModel, session: SessionModel):
    """Invoke RAG and update the query item with the response."""
    rag_response = query_rag_with_session(query_item.query_text, session)

    query_item.answer_text = rag_response.response_text
    query_item.sources = rag_response.sources
    query_item.is_complete = True

    # Save the updated query to DynamoDB
    query_item.put_item()

    print(f"✅ Item is updated: {query_item}")
    return query_item

def main():
    """Local test for RAG invocation."""
    print("Running example RAG call.")

    # Simulate an initial query and session for local testing
    query_item_1 = QueryModel(
        query_text="How to make rice and chicken in the game?",
        session_id="default_session"  # Ensure to use an existing session_id or create one
    )
    
    # Get or create session for local testing
    session_1 = get_or_create_session(query_item_1.session_id)
    
    # Invoke RAG with the first query and session
    response_1 = invoke_rag(query_item_1, session_1)
    print(f"Received: {response_1}")

    # Simulate the second query (continuing the conversation)
    query_item_2 = QueryModel(
        query_text="Okay, I boild the water, what's next?",
        session_id="default_session"  # Same session_id for continuation
    )
    
    # Invoke RAG with the second query and session
    response_2 = invoke_rag(query_item_2, session_1)
    print(f"Received: {response_2}")

    # Simulate a topic switch (new question, but same session)
    query_item_3 = QueryModel(
        query_text="Also, can I make waffles with the same ingredients?",
        session_id="default_session"  # Same session_id for continuity
    )
    
    # Invoke RAG with the third query and session
    response_3 = invoke_rag(query_item_3, session_1)
    print(f"Received: {response_3}")

    # Returning to pancakes
    query_item_4 = QueryModel(
        query_text="Cool! What's after draining the chicken?",
        session_id="default_session"  # Same session_id for continuity
    )
    
    # Invoke RAG with the fourth query and session
    response_4 = invoke_rag(query_item_4, session_1)
    print(f"Received: {response_4}")

if __name__ == "__main__":
    # For local testing
    main()
