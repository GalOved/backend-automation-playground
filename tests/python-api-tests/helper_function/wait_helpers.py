import time
import helper_function.http_helper as api


def poll_scan_status(scan_id, expected_status, base_url, timeout=30, interval=2):
    """Poll GET /scans/{id} until status matches or timeout."""
    elapsed = 0
    status = None
    while elapsed < timeout:
        response = api.get(f"{base_url}/scans/{scan_id}")
        assert response.status_code == 200
        status = response.json().get("status")
        if status == expected_status:
            return response.json()
        time.sleep(interval)
        elapsed += interval
    raise TimeoutError(f"Scan {scan_id} did not reach '{expected_status}' within {timeout}s. Last status: {status}")


def poll_webhook_received(scan_id, webhook_base_url, timeout=10, interval=1):
    """Poll GET /webhooks until scan_id appears or timeout."""
    elapsed = 0
    while elapsed < timeout:
        response = api.get(f"{webhook_base_url}/webhooks")
        assert response.status_code == 200
        webhook_ids = [w["scanId"] for w in response.json()]
        if scan_id in webhook_ids:
            return [w for w in response.json() if w["scanId"] == scan_id][0]
        time.sleep(interval)
        elapsed += interval
    raise TimeoutError(f"Webhook for scan {scan_id} was not received within {timeout}s")
