import os
import time
import boto3
from botocore.exceptions import ClientError
from typing import List, Dict
from decimal import Decimal
import uuid

TABLE_NAME = os.environ.get("TABLE_NAME")
if not TABLE_NAME:
    raise ValueError("TABLE_NAME environment variable is not set")

class SessionModel:
    def __init__(self, session_id: str, query_id: str = None):
        self.session_id = session_id
        self.query_id = query_id or str(uuid.uuid4())
        self.history: List[Dict[str, str]] = []
        self.last_activity = time.time()

        self.active_context = None  # "recipe", "risk", or None
        self.active_recipe_flow = self._empty_flow()
        self.active_risk_flow = self._empty_flow()

    def _empty_flow(self):
        return {
            "title": None,
            "steps": [],
            "current_step_index": 0,
            "completed_steps": [],
        }

    @classmethod
    def get_table(cls):
        return boto3.resource("dynamodb").Table(TABLE_NAME)

    def add_to_history(self, user_query: str, bot_response: str):
        self.history.append({"user": user_query, "bot": bot_response})
        self.last_activity = time.time()
        self.save()

    def get_conversation_context(self) -> str:
        return "\n".join(
            [f"User: {entry['user']}\nBot: {entry['bot']}" for entry in self.history]
        )

    def get_history(self) -> List[Dict[str, str]]:
        return self.history

    def is_expired(self, timeout: int = 3600) -> bool:
        return (time.time() - self.last_activity) > timeout

    def save(self):
        item = {
            "session_id": self.session_id,
            "query_id": self.query_id,
            "history": self.history,
            "last_activity": Decimal(str(self.last_activity)),
            "active_context": self.active_context,
            "active_recipe_flow": self.active_recipe_flow,
            "active_risk_flow": self.active_risk_flow,
        }
        try:
            self.get_table().put_item(Item=item)
        except ClientError as e:
            print(f"Error saving session: {e.response['Error']['Message']}")
            raise e

    @classmethod
    def get(cls, session_id: str):
        try:
            response = cls.get_table().get_item(Key={"session_id": session_id})
            if "Item" in response:
                item = response["Item"]
                session = SessionModel(session_id=item["session_id"])
                session.history = item.get("history", [])
                session.last_activity = float(item.get("last_activity", time.time()))
                session.active_context = item.get("active_context")

                session.active_recipe_flow = item.get("active_recipe_flow", session._empty_flow())
                session.active_risk_flow = item.get("active_risk_flow", session._empty_flow())
                return session
            return None
        except ClientError as e:
            print(f"Error retrieving session: {e.response['Error']['Message']}")
            return None
