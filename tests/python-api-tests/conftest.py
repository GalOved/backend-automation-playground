import pytest
import helper_function.id_generator as id_gen

SCAN_API_URL = "http://localhost:5094"
WEBHOOK_URL = "http://localhost:5000"

@pytest.fixture
def unique_id():
    """Returns a fresh unique 3-char ID for each test."""
    return id_gen.generate_id()