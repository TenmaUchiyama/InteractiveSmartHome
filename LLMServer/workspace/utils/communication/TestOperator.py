import os
# from DeviceOperator import DeviceOperator
import httpx


class TestOperator():
    def __init__(self):
        
        self.request_url = os.getenv("XR_SERVER_API") + "/device/operate"
        print(self.request_url)

    def send_operate_request(self, devices):
        print("Sending Operate Request to Test Server.") 
        response = httpx.post(self.request_url, json=devices)
        output = ""
        print("CODE: ", response.status_code)
        if response.status_code == 200:
            output = f"[Status] {response.status_code}\n[Message] {response.json()['message']}"
            print("All Devices Are Operated Successfully.")
        else:
            output = f"[Status] {response.status_code}\n[Message] Failed to Operate Devices."
            print("Failed to Operate Devices.")

        return output
    



if __name__ == "__main__":
    test = TestOperator()
    test.send_operate_request([{"id": "test_curtain_id", "state": True, "intensity": 100}])
    