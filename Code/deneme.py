import requests
import time

# Step 1: Submit a query
submit_url = "https://lviubjhdkcolf6ihebsg3aohf40ocbxs.lambda-url.eu-central-1.on.aws/submit_query"
submit_data = {"query_text": "How to use a fire extinguisher step by step?"}

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
                    print("Query processing complete!")
                    print("Final Response:", query_details)
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
