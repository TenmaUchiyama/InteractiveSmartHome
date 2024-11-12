from dataclasses import dataclass
from typing import Union, Literal, Any

@dataclass
class ISignalData:
    action_id: str
    data_type: Literal["string", "number", "boolean", "json", "trigger", "request"]
    value: Union[str, int, bool, dict, None]
