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
