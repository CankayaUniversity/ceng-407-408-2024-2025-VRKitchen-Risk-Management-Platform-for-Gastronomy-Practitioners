import requests

# Step 1: Submit the first query
query_url = "https://lviubjhdkcolf6ihebsg3aohf40ocbxs.lambda-url.eu-central-1.on.aws/query_with_memory"

# First query
query_data_1 = {
    "query_text": "How to make rice and chicken in the game?",
    "session_id": "user_123"  # Use a unique session ID for each user (optional)
}

# Submit the first query
submit_response_1 = requests.post(query_url, json=query_data_1)

# Check if the response status code is 500 (Internal Server Error)
if submit_response_1.status_code == 500:
    print("Failed to submit first query. Error details:", submit_response_1.text)  # Prints the error response body
else:
    # Handle success
    if submit_response_1.status_code == 200:
        submit_result_1 = submit_response_1.json()
        print("First query submitted successfully!")
        #print("Response:\n", submit_result_1)

        # Extract session_id (if not provided)
        session_id = submit_result_1.get("session_id", query_data_1.get("session_id"))

        # Step 2: Submit the second query (using the same session_id)
        query_data_2 = {
            "query_text": "I drained the chicken, what's the next step in the game?",
            "session_id": session_id  # Use the same session_id for context
        }

        # Submit the second query
        submit_response_2 = requests.post(query_url, json=query_data_2)

        if submit_response_2.status_code == 200:
            submit_result_2 = submit_response_2.json()
            print("\nSecond query submitted successfully!")
            #print("Response:\n", submit_result_2)

            # Display the final response
            print("\nFinal Step-by-step Breakdown:")
            print(f"1. Query: {query_data_1['query_text']}")
            print(f"   Response:\n {submit_result_1['response_text']}")
            print(f"   Sources: {submit_result_1['sources']}")
            print(f"2. Query: {query_data_2['query_text']}")
            print(f"   Response:\n {submit_result_2['response_text']}")
            print(f"   Sources: {submit_result_2['sources']}")
        else:
            print("Failed to submit second query:", submit_response_2.status_code, submit_response_2.text)
    else:
        print("Failed to submit first query:", submit_response_1.status_code, submit_response_1.text)
