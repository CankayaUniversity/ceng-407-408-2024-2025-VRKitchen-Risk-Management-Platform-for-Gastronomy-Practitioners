import time
import uuid
from typing import List, Optional
from pydantic import BaseModel, Field
from botocore.exceptions import ClientError
import boto3
import os
from rag_app.session_model import SessionModel  # Assuming this file exists

# Ensure environment variable TABLE_NAME is set.
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
        """Get DynamoDB Table resource."""
        dynamodb = boto3.resource("dynamodb")
        return dynamodb.Table(TABLE_NAME)

    def put_item(self):
        """Put the QueryModel instance to DynamoDB."""
        item = self.as_ddb_item()
        try:
            response = QueryModel.get_table().put_item(Item=item)
            print(f"Item saved to DynamoDB: {response}")
        except ClientError as e:
            print(f"ClientError: {e.response['Error']['Message']}")
            raise e

    def as_ddb_item(self):
        """Converts QueryModel instance to DynamoDB item format."""
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

