

from typing import List
from connection import ConnectionManager
import asyncio

class ConnectionManagerList:
    def __init__(self):
        self.managers: List[ConnectionManager] = []
        self.lock = asyncio.Lock()
    
    async def add_manager(self, manager: ConnectionManager):
        async with self.lock:
            self.managers.append(manager)
    
    async def remove_manager(self, manager: ConnectionManager):
        async with self.lock:
            self.managers.remove(manager)
    
    async def get_managers(self) -> List[ConnectionManager]:
        async with self.lock:
            return list(self.managers)
