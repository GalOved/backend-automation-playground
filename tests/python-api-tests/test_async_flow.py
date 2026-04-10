import helper_function.http_helper as api
import helper_function.wait_helpers as helper
from conftest import SCAN_API_URL, WEBHOOK_URL


def test_full_scan_flow(unique_id):
    """POST scan → worker processes → status becomes COMPLETED → webhook received."""
    request = {
        "documentId": f"async-doc-{unique_id}",
        "text": "hello world",
        "callbackUrl": f"{WEBHOOK_URL}/webhooks/status"
    }
    create_response = api.post(f"{SCAN_API_URL}/scans", request)
    assert create_response.status_code == 202

    scan_id = create_response.json().get("id")
    assert scan_id is not None

    scan = helper.poll_scan_status(scan_id, "COMPLETED", SCAN_API_URL, timeout=30)
    assert scan["status"] == "COMPLETED"

    webhooks_response = api.get(f"{WEBHOOK_URL}/webhooks")
    assert webhooks_response.status_code == 200
    webhook_ids = [w["scanId"] for w in webhooks_response.json()]
    assert scan_id in webhook_ids


def test_failed_scan_flow(unique_id):
    """POST scan with 'fail' in text → status becomes FAILED → webhook has status FAILED."""
    request = {
        "documentId": f"async-doc-{unique_id}",
        "text": "this will fail",
        "callbackUrl": f"{WEBHOOK_URL}/webhooks/status"
    }
    create_response = api.post(f"{SCAN_API_URL}/scans", request)
    assert create_response.status_code == 202

    scan_id = create_response.json().get("id")
    assert scan_id is not None

    scan = helper.poll_scan_status(scan_id, "FAILED", SCAN_API_URL, timeout=30)
    assert scan["status"] == "FAILED"

    webhooks_response = api.get(f"{WEBHOOK_URL}/webhooks")
    assert webhooks_response.status_code == 200
    matching = [w for w in webhooks_response.json() if w["scanId"] == scan_id]
    assert len(matching) == 1
    assert matching[0]["status"] == "FAILED"
