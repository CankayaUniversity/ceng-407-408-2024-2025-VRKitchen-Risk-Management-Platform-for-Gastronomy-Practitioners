from typing import List, Dict
import time

class SessionModel:
    def __init__(self, session_id: str):
        self.session_id = session_id
        self.history = []  # Stores conversation history as a list of dictionaries
        self.last_activity = time.time()  # Track last activity for session expiration

    def add_to_history(self, user_query: str, bot_response: str):
        """Add a user query and bot response to the session history."""
        self.history.append({"user": user_query, "bot": bot_response})
        self.last_activity = time.time()  # Update last activity time

    def get_conversation_context(self) -> str:
        """Retrieve the conversation history as a formatted string for context."""
        return "\n".join([f"User: {entry['user']}\nBot: {entry['bot']}" for entry in self.history])

    def get_history(self) -> List[Dict[str, str]]:
        """Return the full conversation history."""
        return self.history

    def is_expired(self, timeout: int = 3600) -> bool:
        """Check if the session has expired due to inactivity."""
        return (time.time() - self.last_activity) > timeout