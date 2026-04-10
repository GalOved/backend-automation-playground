import helper_function.http_helper as api
from conftest import SCAN_API_URL, WEBHOOK_URL


def test_health_check():
    response = api.get(f"{SCAN_API_URL}/health")
    assert response.status_code == 200

def test_create_scan(unique_id):
    request = {
        "documentId": f"doc-{unique_id}",
        "text": "hello world",
        "callbackUrl": f"{WEBHOOK_URL}/webhooks/status"
    }
    response = api.post(f"{SCAN_API_URL}/scans", request)
    assert response.status_code == 202

def test_get_scan_by_id(unique_id):
    create_request = {
        "documentId": f"doc-{unique_id}",
        "text": "test scan",
        "callbackUrl": f"{WEBHOOK_URL}/webhooks/status"
    }
    create_response = api.post(f"{SCAN_API_URL}/scans", create_request)
    assert create_response.status_code == 202

    scan_id = create_response.json()['id']
    response = api.get(f"{SCAN_API_URL}/scans/{scan_id}")

    assert response.status_code == 200
    assert response.json().get("documentId") == f"doc-{unique_id}"
    assert response.json().get("text") == "test scan"
    assert response.json().get("callbackUrl") == f"{WEBHOOK_URL}/webhooks/status"

def test_get_all_scans(unique_id):
    create_request_1 = {
        "documentId": f"doc-{unique_id}-1",
        "text": "another test scan",
        "callbackUrl": f"{WEBHOOK_URL}/webhooks/status"
    }
    create_request_2 = {
        "documentId": f"doc-{unique_id}-2",
        "text": "yet another test scan",
        "callbackUrl": f"{WEBHOOK_URL}/webhooks/status"
    }
    create_response_1 = api.post(f"{SCAN_API_URL}/scans", create_request_1)
    create_response_2 = api.post(f"{SCAN_API_URL}/scans", create_request_2)
    assert create_response_1.status_code == 202 and create_response_2.status_code == 202

    response = api.get(f"{SCAN_API_URL}/scans")
    assert response.status_code == 200
    assert len(response.json()) >= 2

    scan_ids = [scan["id"] for scan in response.json()]
    assert create_response_1.json()['id'] in scan_ids
    assert create_response_2.json()['id'] in scan_ids

    scan_document_ids = [scan["documentId"] for scan in response.json()]
    assert create_request_1["documentId"] in scan_document_ids
    assert create_request_2["documentId"] in scan_document_ids

def test_get_nonexistent_scan():
    response = api.get(f"{SCAN_API_URL}/scans/nonexistent-id")
    assert response.status_code == 404
