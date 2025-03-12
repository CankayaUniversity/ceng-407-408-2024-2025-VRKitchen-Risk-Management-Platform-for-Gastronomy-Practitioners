import boto3
import time
import uuid
import os
from typing import List, Optional  # Import Optional and List from typing
from pydantic import BaseModel, Field
from botocore.exceptions import ClientError

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
    def get_table(cls):
        """Get DynamoDB Table resource."""
        dynamodb = boto3.resource("dynamodb")
        return dynamodb.Table(TABLE_NAME)

    def put_item(self):
        """Put the QueryModel instance to DynamoDB."""
        item = self.as_ddb_item()
        try:
            response = QueryModel.get_table().put_item(Item=item)
            print(f"Query saved to DynamoDB: {response}")
        except ClientError as e:
            print(f"Error saving query: {e.response['Error']['Message']}")
            raise e

    def as_ddb_item(self):
        """Converts QueryModel instance to DynamoDB item format."""
        return {k: v for k, v in self.dict().items() if v is not None}

    @classmethod
    def get_item(cls, query_id: str) -> "QueryModel":
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
            print(f"Error retrieving query: {e.response['Error']['Message']}")
            return None
