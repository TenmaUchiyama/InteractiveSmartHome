import dotenv
from EXPERIMENT.task_manager import ExperimentTaskResultManager
dotenv.load_dotenv("../.env")
import os
from agent_runner_spoperate import runner
from sr_app_types.agent_types import State
from agents.device_operator_agent.operator_tool import operateDevice
from fastapi import FastAPI
import httpx
from starlette.middleware.cors import CORSMiddleware
import uvicorn
from pydantic import BaseModel
import os 
