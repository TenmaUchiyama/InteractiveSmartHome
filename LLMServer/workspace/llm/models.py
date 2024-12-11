# models.py
from uuid import UUID
from dataclasses import dataclass
from typing import Dict, Tuple

@dataclass
class DeviceData:
    device_id: UUID
    device_name: str
    distance_from_user: float
    device_position: Tuple[float, float, float]

@dataclass 
class DBDeviceData:
    device_id: str
    device_name: str
    device_type: str
    mqtt_topic: str
    device_position: Dict[str, float]
