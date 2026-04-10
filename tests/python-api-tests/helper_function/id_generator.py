import uuid


def generate_id():
    """Generate a unique 3-character string."""
    return uuid.uuid4().hex[:3]
