
import json
import os
import dotenv 
from langchain_openai import ChatOpenAI

dotenv.load_dotenv()

LOG_FILE_PATH_LOCATION = os.getenv("PROJECT_PATH")
LOG_DIR = os.path.join(LOG_FILE_PATH_LOCATION, "log", "test")
LOG_FILE = os.path.join(LOG_DIR, "Label",  "TEST_test_id.json")



# フォルダが存在しなければ作成
os.makedirs(LOG_DIR, exist_ok=True)
if __name__ == "__main__":
    with open(LOG_FILE, "r") as f:
        data = f.read()
        data = json.loads(data)
        output = json.loads(data['llm_output'])
        print(output)

    with open(".", "w") as f:
        f.write()

    
    # llm = ChatOpenAI(model="gpt-4o-mini", verbose="True")
    # response = llm.invoke("Hello, how are you?")
    # print(response.dict())





    

    