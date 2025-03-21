import os
import json
from dotenv import load_dotenv
from fastapi import FastAPI, HTTPException
from pydantic import BaseModel
from openai import AzureOpenAI

load_dotenv()

azure_oai_endpoint = os.getenv("AZURE_OAI_ENDPOINT")
azure_oai_key = os.getenv("AZURE_OAI_KEY")
azure_oai_deployment = os.getenv("AZURE_OAI_DEPLOYMENT")
azure_search_endpoint = os.getenv("AZURE_SEARCH_ENDPOINT")
azure_search_key = os.getenv("AZURE_SEARCH_KEY")
azure_search_index = os.getenv("AZURE_SEARCH_INDEX")

required_env_vars = {
    "AZURE_OAI_ENDPOINT": azure_oai_endpoint,
    "AZURE_OAI_KEY": azure_oai_key,
    "AZURE_OAI_DEPLOYMENT": azure_oai_deployment,
    "AZURE_SEARCH_ENDPOINT": azure_search_endpoint,
    "AZURE_SEARCH_KEY": azure_search_key,
    "AZURE_SEARCH_INDEX": azure_search_index,
}

for var, value in required_env_vars.items():
    if not value:
        print(f"Warning: {var} is not set!")

app = FastAPI()

client = AzureOpenAI(
    base_url=f"{azure_oai_endpoint}/openai/deployments/{azure_oai_deployment}/extensions",
    api_key=azure_oai_key,
    api_version="2023-09-01-preview"
)

class QueryRequest(BaseModel):
    question: str

@app.get("/")
async def root():
    return {"message": "VR Kitchen AI is running!"}

@app.post("/ask")
async def ask_question(request: QueryRequest):
    try:
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

        print("...Sending the following request to Azure OpenAI endpoint...")
        print("Request: " + request.question + "\n")

        response = client.chat.completions.create(
            model=azure_oai_deployment,
            temperature=0.5,
            max_tokens=1000,
            messages=[
                {"role": "system", "content": "You are a helpful kitchen agent"},
                {"role": "user", "content": request.question}
            ],
            extra_body=extension_config
        )

        return {
            "response": response.choices[0].message.content
        }
    
    except Exception as ex:
        raise HTTPException(status_code=500, detail=str(ex))