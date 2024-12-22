

import uuid

def generate_unique_id() -> str:
    """
    一意のリクエストIDを生成する関数。
    """
    return str(uuid.uuid4())
