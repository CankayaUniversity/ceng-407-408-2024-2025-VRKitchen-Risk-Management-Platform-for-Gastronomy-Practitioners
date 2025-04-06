import os
import re
import time
from dotenv import load_dotenv
from fastapi import FastAPI, HTTPException
from pydantic import BaseModel
from openai import AzureOpenAI

# Load env vars
load_dotenv()
azure_oai_endpoint = os.getenv("AZURE_OAI_ENDPOINT")
azure_oai_key = os.getenv("AZURE_OAI_KEY")
azure_oai_deployment = os.getenv("AZURE_OAI_DEPLOYMENT")
azure_search_endpoint = os.getenv("AZURE_SEARCH_ENDPOINT")
azure_search_key = os.getenv("AZURE_SEARCH_KEY")
azure_search_index = os.getenv("AZURE_SEARCH_INDEX")

# OpenAI clients
rag_client = AzureOpenAI(
    base_url=f"{azure_oai_endpoint}/openai/deployments/{azure_oai_deployment}/extensions",
    api_key=azure_oai_key,
    api_version="2023-09-01-preview"
)

chat_client = AzureOpenAI(
    base_url=f"{azure_oai_endpoint}/openai/deployments/{azure_oai_deployment}",
    api_key=azure_oai_key,
    api_version="2023-09-01-preview"
)

# FastAPI app + in-memory tracking
app = FastAPI()
user_memory = {}

class QueryRequest(BaseModel):
    user_id: str
    question: str

@app.get("/")
async def root():
    return {"message": "VR Kitchen AI is running!"}

@app.post("/ask")
async def ask_question(request: QueryRequest):
    user_id = request.user_id
    question = request.question.strip()
    question_lower = question.lower()
    memory = user_memory.get(user_id)

    # --- STEP VALIDATION FLOW ---
    if memory and question_lower.endswith("what now?"):
        current_index = memory["current_step_index"]
        steps = memory["steps"]

        if current_index >= len(steps):
            return {"response": "✅ You’ve completed all the steps. Great job!"}

        expected_step_raw = steps[current_index]
        expected_step_clean = re.sub(r"\[.*?\]", "", expected_step_raw).strip()
        next_step = steps[current_index + 1] if current_index + 1 < len(steps) else None
        completed = memory["completed_steps"]

        # GPT prompt for validation with improved logic
        prompt = (
            "You are a step-following assistant in a VR cooking game.\n"
            "The player will report actions they have just completed.\n"
            "Your job is to compare what they said with the expected step.\n\n"
            "🧠 IMPORTANT:\n"
            "- Treat the player's input as a completed action.\n"
            "- Check whether it semantically matches the expected step.\n"
            "- It's okay if the wording is different as long as the meaning is the same.\n"
            "- NEVER invent or reword steps yourself.\n"
            "- Only use the exact next step provided from memory if the player did the right thing.\n\n"
            "🟢 If the player's action matches the expected step:\n"
            "- Confirm it kindly (e.g. '✅ Great job!')\n"
            "- Then say: 'Next step: <next step from memory>'\n\n"
            "🔴 If the action does NOT match:\n"
            "- Say something like: '❌ Not quite. You still need to: <expected step>'\n\n"
            f"✅ Expected step:\n{expected_step_clean}\n"
            f"{'👉 Next step:\n' + next_step if next_step else 'You’re finished after this!'}\n"
            f"🗣️ Player said:\n{question}"
        )

        gpt_response = chat_client.chat.completions.create(
            model=azure_oai_deployment,
            temperature=0,
            max_tokens=300,
            messages=[
                {"role": "system", "content": "You are a VR cooking assistant. Only use memory. Never invent steps."},
                {"role": "user", "content": prompt}
            ]
        )

        message = gpt_response.choices[0].message.content.strip()

        # Fuzzy match logic for backend tracking
        player_words = set(question_lower.split())
        step_words = set(expected_step_clean.lower().split())
        match_ratio = len(player_words & step_words) / max(len(step_words), 1)

        if match_ratio >= 0.5:
            memory["completed_steps"].append(expected_step_raw)
            memory["current_step_index"] += 1

        print(f"[DEBUG] Updated memory for {user_id}:\n", user_memory)
        return {"response": message}

    # --- RAG: Ask for a recipe or general help ---
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

    rag_response = rag_client.chat.completions.create(
        model=azure_oai_deployment,
        temperature=0.5,
        max_tokens=1000,
        messages=[
            {
                "role": "system",
                "content": "You are a helpful kitchen assistant. When asked about a recipe, respond with numbered step-by-step instructions."
            },
            {"role": "user", "content": question}
        ],
        extra_body=extension_config
    )

    content = rag_response.choices[0].message.content
    print("RAG Response:\n", content)

    # Extract steps from RAG output
    steps = re.findall(r"\d+\.\s+(.*)", content)
    if not steps:
        steps = [line.strip("-• ").strip() for line in content.split("\n") if line.strip()]

    if len(steps) >= 2:
        user_memory[user_id] = {
            "selected_recipe": question,
            "steps": steps,
            "current_step_index": 0,
            "completed_steps": [],
            "last_active": time.time()
        }
        print(f"[DEBUG] Initialized memory for {user_id}:\n", user_memory)
        return {"response": f"✅ Got it! Let's begin.\nFirst step: {steps[0]}"}

    return {"response": content}
