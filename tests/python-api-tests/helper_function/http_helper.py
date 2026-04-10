import requests
def post(url, request):
    try:
        print(f" POST function started: URL: {url}, Payload: {request}")
        response = requests.post(url, json=request)
        return response
    except requests.RequestException as e:
        print(f"Error occurred while making POST request: {e}")
        raise e

def get(url):
    try:
        print(f" GET function started: URL: {url}")
        response = requests.get(url)
        return response
    except requests.RequestException as e:
        print(f"Error occurred while making GET request: {e}")
        raise e
