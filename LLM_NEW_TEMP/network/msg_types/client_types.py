from enum import Enum
from typing import TypedDict

class ClientMessageType(Enum):
    REQUEST_DATA = "requestData"
    GET_NAME = "getName" 
    RESULT = "result"
    ERROR = "error"

class RequestDataMessage(TypedDict):
    type: str
    request_id: str

class ResultMessage(TypedDict):
    type: str
    data: str

class ErrorMessage(TypedDict):
    type: str
    message: str
