import requests
from concurrent.futures import ThreadPoolExecutor, as_completed

def post_many(url, requests_list):
    """Send multiple POST requests to the same URL concurrently.
    Returns a list of responses in completion order."""
    with ThreadPoolExecutor(max_workers=len(requests_list)) as pool:
        futures = [pool.submit(post, url, req) for req in requests_list]
        return [f.result() for f in as_completed(futures)]

def post(url, request):
    try:
        print(f" POST function started: URL: {url}, Payload: {request}")
        response = requests.post(url, json=request)
        return response
    except requests.RequestException as e:
        print(f"Error occurred while making POST request: {e}")
        raise e

def get_many(urls):
    """Send multiple GET requests concurrently.
    Returns a list of responses in completion order."""
    with ThreadPoolExecutor(max_workers=len(urls)) as pool:
        futures = [pool.submit(get, url) for url in urls]
        return [f.result() for f in as_completed(futures)]

def get(url):
    try:
        print(f" GET function started: URL: {url}")
        response = requests.get(url)
        return response
    except requests.RequestException as e:
        print(f"Error occurred while making GET request: {e}")
        raise e
