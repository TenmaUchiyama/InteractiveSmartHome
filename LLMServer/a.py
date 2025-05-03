import dotenv

dotenv.load_dotenv() 
import os 
from langchain_openai import ChatOpenAI

print(ChatOpenAI(model="gpt-4o", verbose=True).invoke("Hello! how are you doing?"))