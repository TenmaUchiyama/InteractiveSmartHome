from enum import Enum
from typing import TypedDict



class ServerMessageType(Enum):
    INITIAL_DATA = "initialData"
    ADDITIONAL_DATA = "additionalData"
    Greeting = "greeting"



"""

データを受け取る時の型。
以下で指定した型ででデータを受け取ることができる。
"""

class InitialDataMessage(TypedDict):
    type: str
    data: str

class AdditionalDataMessage(TypedDict):
    type: str
    request_id: str
    data: str



class TestComplexRequestMessage(TypedDict):
    type: str