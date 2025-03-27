from abc import ABC, abstractmethod


class DeviceOperator(ABC):

    
    @abstractmethod
    def send_operate_request(self, devices):
        pass
        


