import requests
import time

# Step 1: Submit a query
submit_url = "https://lviubjhdkcolf6ihebsg3aohf40ocbxs.lambda-url.eu-central-1.on.aws/submit_query"
submit_data = {"query_text": "I filled the water, what should I do next?"}

submit_response = requests.post(submit_url, json=submit_data)

if submit_response.status_code == 200:
    submit_result = submit_response.json()
    print("Query submitted successfully!")
    print("Response:", submit_result)

    # Extract query_id
    query_id = submit_result.get("query_id")

    # Step 2: Poll for completion
    if query_id:
        get_query_url = "https://lviubjhdkcolf6ihebsg3aohf40ocbxs.lambda-url.eu-central-1.on.aws/get_query"
        get_query_params = {"query_id": query_id}

        print("Polling for query completion...")
        while True:
            get_response = requests.get(get_query_url, params=get_query_params)
            if get_response.status_code == 200:
                query_details = get_response.json()
                print("Current Query Details:", query_details)

                # Check if processing is complete
                if query_details.get("is_complete", False):
                    print("\nQuery processing complete!\n")
                    print("Step-by-step Breakdown:")
                    
                    # Print query details step by step
                    print(f"1. Query ID: {query_details['query_id']}")
                    print(f"2. Query Submitted Time: {query_details['create_time']}")
                    print(f"3. Query Text: {query_details['query_text']}")
                    print(f"4. Answer Text: \n{query_details['answer_text']}")
                    
                    # Display the sources used
                    print(f"5. Sources: ")
                    for source in query_details.get('sources', []):
                        print(f"   - {source}")

                    break
            else:
                print("Failed to fetch query details:", get_response.status_code, get_response.text)
                break

            # Wait before polling again
            time.sleep(3)  # Adjust the polling interval as needed
    else:
        print("No query_id returned in response.")
else:
    print("Failed to submit query:", submit_response.status_code, submit_response.text)
