import helper_function.http_helper as api
from conftest import SCAN_API_URL, WEBHOOK_URL

import helper_function.id_generator as id_gen

def test_post_10_scans_concurrently():
    """Send 10 POST /scans requests at the same time and assert all return 202."""
    payloads = [
        {
            "documentId": f"load-doc-{id_gen.generate_id()}-{i}",
            "text": f"load test request {i}",
            "callbackUrl": f"{WEBHOOK_URL}/webhooks/status",
        }
        for i in range(10)
    ]

    responses = api.post_many(f"{SCAN_API_URL}/scans", payloads)

    non_202 = [r for r in responses if r.status_code != 202]
    assert not non_202, f"Some requests failed: {[r.status_code for r in non_202]}"


def test_get_10_scans_concurrently():
    """Create 10 scans, then GET all of them at the same time and assert all return 200."""
    payloads = [
        {
            "documentId": f"load-doc-{id_gen.generate_id()}-{i}",
            "text": f"load test request {i}",
            "callbackUrl": f"{WEBHOOK_URL}/webhooks/status",
        }
        for i in range(10)
    ]
    create_responses = api.post_many(f"{SCAN_API_URL}/scans", payloads)
    scan_ids = [r.json()["id"] for r in create_responses]

    urls = [f"{SCAN_API_URL}/scans/{scan_id}" for scan_id in scan_ids]
    responses = api.get_many(urls)

    non_200 = [r for r in responses if r.status_code != 200]
    assert not non_200, f"Some GET requests failed: {[r.status_code for r in non_200]}"
