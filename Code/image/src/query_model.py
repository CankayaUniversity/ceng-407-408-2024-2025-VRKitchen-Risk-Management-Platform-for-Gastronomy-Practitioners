# query_model.py (Updated)
import time
import uuid
from typing import List, Optional
from pydantic import BaseModel, Field
from botocore.exceptions import ClientError
import boto3
import os
from rag_app.session_model import SessionModel  # Assuming this file exists

TABLE_NAME = os.environ.get("TABLE_NAME")
if not TABLE_NAME:
    raise ValueError("TABLE_NAME environment variable is not set")

class QueryModel(BaseModel):
    query_id: str = Field(default_factory=lambda: uuid.uuid4().hex)
    create_time: int = Field(default_factory=lambda: int(time.time()))
    query_text: str
    answer_text: Optional[str] = None
    sources: List[str] = Field(default_factory=list)
    is_complete: bool = False
    session_id: str  # This is used to link the query to a session

    @classmethod
    def get_table(cls: "QueryModel") -> boto3.resource:
        dynamodb = boto3.resource("dynamodb")
        return dynamodb.Table(TABLE_NAME)

    def put_item(self):
        item = self.as_ddb_item()
        try:
            response = QueryModel.get_table().put_item(Item=item)
            print(f"Item saved to DynamoDB: {response}")
        except ClientError as e:
            print(f"ClientError: {e.response['Error']['Message']}")
            raise e

    def as_ddb_item(self):
        """Converts QueryModel instance to a DynamoDB item format."""
        return {k: v for k, v in self.dict().items() if v is not None}

    @classmethod
    def get_item(cls: "QueryModel", query_id: str) -> "QueryModel":
        """Retrieve a QueryModel from DynamoDB based on query_id."""
        try:
            response = cls.get_table().get_item(Key={"query_id": query_id})
            if "Item" in response:
                item = response["Item"]
                return cls(**item)
            else:
                print(f"No item found with query_id: {query_id}")
                return None
        except ClientError as e:
            print(f"ClientError: {e.response['Error']['Message']}")
            return None
    
    @classmethod
    def get_session(cls, session_id: str) -> SessionModel:
        """Retrieve or create a session."""
        session = sessions.get(session_id)
        if not session:
            session = SessionModel(session_id)
            sessions[session_id] = session
        return session

    def update_answer(self, answer_text: str, sources: List[str]):
        """Update the answer text and sources, and mark as complete."""
        self.answer_text = answer_text
        self.sources = sources
        self.is_complete = True
        self.put_item()

# In-memory storage for sessions (to be replaced by DynamoDB or another persistent store)
sessions = {}  # Simulating an in-memory session store

# Example of how to use sessions and queries:
def example_usage():
    # Create or get an existing session
    session_id = "session_123"  # This would be dynamic in practice
    session = QueryModel.get_session(session_id)

    # Create a new query model
    query = QueryModel(query_text="What is the capital of France?", session_id=session_id)
    print(f"Created QueryModel: {query}")

    # Save the query to DynamoDB
    query.put_item()

    # Simulate getting an answer and updating the query
    query.update_answer(answer_text="The capital of France is Paris.", sources=["Wikipedia"])
    print(f"Updated QueryModel: {query}")

    # Retrieve the model from DynamoDB
    retrieved_query = QueryModel.get_item(query.query_id)
    if retrieved_query:
        print(f"Retrieved QueryModel: {retrieved_query}")

    # Add the query and response to the session's history
    session.add_to_history(query.query_text, query.answer_text)
    print(f"Session History: {session.get_history()}")

if __name__ == "__main__":
    example_usage()
