import os
import re
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

user_memory = {}

class QueryRequest(BaseModel):
    user_id: str
    question: str

class SelectRecipeRequest(BaseModel):
    user_id: str
    recipe_name: str
    steps: list[str]

class UpdateActionRequest(BaseModel):
    user_id: str
    action: str

@app.get("/")
async def root():
    return {"message": "VR Kitchen AI is running!"}

@app.post("/select_recipe")
async def select_recipe(request: SelectRecipeRequest):
    user_memory[request.user_id] = {
        "selected_recipe": request.recipe_name,
        "steps": request.steps,
        "current_step_index": 0,
        "completed_steps": []
    }
    return {"message": f"Recipe '{request.recipe_name}' selected."}

@app.post("/update_action")
async def update_action(request: UpdateActionRequest):
    memory = user_memory.get(request.user_id)
    if not memory:
        raise HTTPException(status_code=400, detail="No recipe selected for this user.")

    current_index = memory["current_step_index"]
    if current_index < len(memory["steps"]):
        expected_step = memory["steps"][current_index]
        if request.action.lower() in expected_step.lower():
            memory["completed_steps"].append(expected_step)
            memory["current_step_index"] += 1
            return {"message": f"Step '{expected_step}' marked as completed."}
        else:
            return {"message": f"Received action: '{request.action}', but expected: '{expected_step}'"}
    else:
        return {"message": "All steps already completed."}

@app.get("/next_step")
async def get_next_step(user_id: str):
    memory = user_memory.get(user_id)
    if not memory:
        raise HTTPException(status_code=400, detail="No recipe selected for this user.")
    index = memory["current_step_index"]
    if index < len(memory["steps"]):
        return {"next_step": memory["steps"][index]}
    return {"message": "All steps completed."}

@app.post("/ask")
async def ask_question(request: QueryRequest):
    try:
        memory = user_memory.get(request.user_id)
        question_lower = request.question.lower()

        if memory:
            if "what now" in question_lower or "next step" in question_lower:
                step_index = memory["current_step_index"]
                if step_index < len(memory["steps"]):
                    return {"response": f"Next step: {memory['steps'][step_index]}"}
                else:
                    return {"response": "You've completed all the steps!"}

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

        response = client.chat.completions.create(
            model=azure_oai_deployment,
            temperature=0.5,
            max_tokens=1000,
            messages=[
                {
                    "role": "system",
                    "content": "You are a helpful kitchen agent. When asked about a recipe, respond with numbered step-by-step instructions."
                },
                {"role": "user", "content": request.question}
            ],
            extra_body=extension_config
        )

        content = response.choices[0].message.content
        print("Response:\n", content)

        numbered_steps = re.findall(r"\d+\.\s+(.*)", content)
        if numbered_steps:
            steps = [step.strip() for step in numbered_steps]
        else:
            steps = [line.strip("-. ").strip() for line in content.split("\n") if line.strip()]

        if len(steps) >= 2:
            user_memory[request.user_id] = {
                "selected_recipe": request.question,
                "steps": steps,
                "current_step_index": 1,
                "completed_steps": [steps[0]]
            }
            return {
                "response": f"Got it! Let's start.\nFirst step: {steps[0]}"
            }

        return {"response": content}

    except Exception as ex:
        raise HTTPException(status_code=500, detail=str(ex))
