import os
import time
import boto3
from botocore.exceptions import ClientError
from typing import List, Dict, Optional
from decimal import Decimal
from query_model import QueryModel
import uuid

TABLE_NAME = os.environ.get("TABLE_NAME")
if not TABLE_NAME:
    raise ValueError("TABLE_NAME environment variable is not set")

class SessionModel:
    def __init__(self, session_id: str, query_id: str = None):
        self.session_id = session_id
        self.history = []  # Stores conversation history as a list of dictionaries
        self.last_activity = time.time()  # Track last activity for session expiration
        self.query_id = query_id or str(uuid.uuid4())  # Generate a unique query_id if not provided


    @classmethod
    def get_table(cls):
        """Get DynamoDB Table resource."""
        dynamodb = boto3.resource("dynamodb")
        return dynamodb.Table(TABLE_NAME)

    def add_to_history(self, user_query: str, bot_response: str):
        """Add a user query and bot response to the session history."""
        self.history.append({"user": user_query, "bot": bot_response})
        self.last_activity = time.time()  # Update last activity time
        self.save()  # Save session data to DynamoDB after adding to history

    def get_conversation_context(self) -> str:
        """Retrieve the conversation history as a formatted string for context."""
        return "\n".join([f"User: {entry['user']}\nBot: {entry['bot']}" for entry in self.history])

    def get_history(self) -> List[Dict[str, str]]:
        """Return the full conversation history."""
        return self.history

    def is_expired(self, timeout: int = 3600) -> bool:
        """Check if the session has expired due to inactivity."""
        return (time.time() - self.last_activity) > timeout

    def save(self):
        """Save session data to DynamoDB."""
        item = {
            "session_id": self.session_id,
            "query_id": self.query_id,  # Ensure query_id is included
            "history": self.history,
            "last_activity": Decimal(str(self.last_activity)),  # Convert float to Decimal
        }
        try:
            response = self.get_table().put_item(Item=item)
            print(f"Session saved to DynamoDB: {response}")
        except ClientError as e:
            print(f"Error saving session: {e.response['Error']['Message']}")
            raise e

    @classmethod
    def get(cls, session_id: str):
        """Retrieve a session from DynamoDB."""
        try:
            response = cls.get_table().get_item(Key={"session_id": session_id})
            if "Item" in response:
                item = response["Item"]
                session = SessionModel(session_id=item["session_id"])
                session.history = item["history"]
                session.last_activity = item["last_activity"]
                return session
            else:
                print(f"Session not found with ID: {session_id}")
                return None
        except ClientError as e:
            print(f"Error retrieving session: {e.response['Error']['Message']}")
            return None
