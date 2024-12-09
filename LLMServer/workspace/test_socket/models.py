
from typing import Any, Dict, List, Union, Literal
from pydantic import BaseModel, ValidationError

# デバイスメタデータの構造を定義
class DeviceMetadata(BaseModel):
    id: int
    type: str
    status: str
    # 必要に応じて他のフィールドを追加

# "llm_agent" アクション用のメッセージモデル
class LLMActionMessage(BaseModel):
    action: Literal["llm_agent"]
    body: str

# "device" アクション用のメッセージモデル
class DeviceActionMessage(BaseModel):
    action: Literal["device"]
    body: List[DeviceMetadata]

# 全てのアクションメッセージを統一するためのUnion型
ActionMessage = Union[LLMActionMessage, DeviceActionMessage]

# サーバーからクライアントへのリクエストメッセージ
class ServerRequestMessage(BaseModel):
    action: str
    request_id: str
    body: Any

# クライアントからサーバーへのレスポンスメッセージ
class ClientResponseMessage(BaseModel):
    request_id: str
    status: str
    body: Any = None
    error: str = None