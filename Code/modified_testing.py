import requests
import time

API_URL = "http://localhost:8000/query_with_memory"
SESSION_ID = "test_user"
DELAY = 5  # seconds between each request

def send_query(text):
    try:
        print(f"\n🧾 Query: {text}")
        response = requests.post(API_URL, json={
            "query_text": text,
            "session_id": SESSION_ID
        })
        response.raise_for_status()
        print("🧠 Response:", response.json().get("response_text", "⚠️ No 'response_text' returned"))
    except Exception as e:
        print("❌ Error:", str(e))
    time.sleep(DELAY)

def main():
    print("🔥 Starting Dual Flow Memory Test with Delays")

    # === Start recipe flow ===
    send_query("How do I cook steak and fries in the game?")  # initialize recipe steps
    send_query("I flipped the steak in the game. What now?")              # should block (skipping ahead)
    send_query("I placed the raw steak on the cutting board in the game. What now?")  # should validate step 1

    # === Trigger risk flow ===
    send_query("Cross contamination happened in the game. What should I do in the game?")  # new flow
    send_query("I threw away the contaminated food in the trash in the game. What now?")  # step 1 (risk)
    send_query("I wet the sponge in the game. What now?")                    # step 2 (risk)
    send_query("I scrubbed the cutting board with the sponge in the game. What now?")  # step 3 (risk)
    send_query("I washed my hands. What now?")                              # step 4 (risk)

    # === Free-form general query (not blocked) ===
    send_query("How do I make a pancake?")

    # === Resume recipe flow ===
    send_query("Resume my steak and fries. What now?")  # resume recipe (step 2 or 3)

if __name__ == "__main__":
    main()
