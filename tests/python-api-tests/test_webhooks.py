import helper_function.http_helper as api
from conftest import SCAN_API_URL, WEBHOOK_URL


def test_get_webhook_status():
    response = api.get(f"{WEBHOOK_URL}/webhooks")
    assert response.status_code == 200

def test_webhooks_receive_after_scan(unique_id):
    scan_id = f"testID-{unique_id}-1"
    request = {
        "scanId": scan_id,
        "status": "COMPLETED",
        "errorMessage": None
    }
    create_response = api.post(f"{WEBHOOK_URL}/webhooks/status", request)
    assert create_response.status_code == 200

    webhook_response = api.get(f"{WEBHOOK_URL}/webhooks")
    assert webhook_response.status_code == 200

    scan_data_list = [scan for scan in webhook_response.json() if scan["scanId"] == scan_id]
    assert len(scan_data_list) == 1
    assert scan_data_list[0]["status"] == "COMPLETED"
    assert scan_data_list[0]["errorMessage"] is None

def test_duplicate_webhook_rejected(unique_id):
    request = {
        "scanId": f"testID-{unique_id}-2",
        "status": "COMPLETED",
        "errorMessage": None
    }
    response1 = api.post(f"{WEBHOOK_URL}/webhooks/status", request)
    assert response1.status_code == 200

    response2 = api.post(f"{WEBHOOK_URL}/webhooks/status", request)
    assert response2.status_code == 409
