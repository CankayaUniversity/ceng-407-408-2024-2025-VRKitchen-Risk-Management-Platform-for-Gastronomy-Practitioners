import requests

BASE_URL = "https://lviubjhdkcolf6ihebsg3aohf40ocbxs.lambda-url.eu-central-1.on.aws"
session_id = "user_123"

# 1️⃣ First query
query1 = {
    "query_text": "How to make rice and chicken in the game?",
    "session_id": session_id
}
res1 = requests.post(f"{BASE_URL}/query_with_memory", json=query1).json()
print("\n--- First Query ---")
print("Q:", query1["query_text"])
print("A:", res1["response_text"])

# 2️⃣ Follow-up query (should use memory)
query2 = {
    "query_text": "I drained the chicken, what's next in the game?",
    "session_id": session_id
}
res2 = requests.post(f"{BASE_URL}/query_with_memory", json=query2).json()
print("\n--- Second Query (With Memory) ---")
print("Q:", query2["query_text"])
print("A:", res2["response_text"])

# 3️⃣ Clear memory
clear_res = requests.get(f"{BASE_URL}/clear_memory").json()
print("\n--- Clear Memory ---")
print(clear_res)

# 4️⃣ Repeat follow-up (should now act like a first-time query again)
res3 = requests.post(f"{BASE_URL}/query_with_memory", json=query2).json()
print("\n--- Repeated Second Query (After Clear) ---")
print("Q:", query2["query_text"])
print("A:", res3["response_text"])
