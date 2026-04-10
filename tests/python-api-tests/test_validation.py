import helper_function.http_helper as api
from conftest import SCAN_API_URL, WEBHOOK_URL


def test_missing_document_id():
    request = {
        "text": "hello world",
        "callbackUrl": f"{WEBHOOK_URL}/webhooks/status"
    }
    response = api.post(f"{SCAN_API_URL}/scans", request)
    assert response.status_code == 400

def test_missing_text(unique_id):
    request = {
        "documentId": f"doc-{unique_id}",
        "callbackUrl": f"{WEBHOOK_URL}/webhooks/status"
    }
    response = api.post(f"{SCAN_API_URL}/scans", request)
    assert response.status_code == 400

def test_missing_callback_url(unique_id):
    request = {
        "documentId": f"doc-{unique_id}",
        "text": "test scan without callback URL"
    }
    response = api.post(f"{SCAN_API_URL}/scans", request)
    assert response.status_code == 400

def test_duplicate_scan_id(unique_id):
    request = {
        "documentId": f"doc-{unique_id}",
        "text": "test scan with duplicate ID",
        "callbackUrl": f"{WEBHOOK_URL}/webhooks/status"
    }
    response1 = api.post(f"{SCAN_API_URL}/scans", request)
    assert response1.status_code == 202

    response2 = api.post(f"{SCAN_API_URL}/scans", request)
    assert response2.status_code == 409

def test_empty_body():
    response = api.post(f"{SCAN_API_URL}/scans", {})
    assert response.status_code == 400
