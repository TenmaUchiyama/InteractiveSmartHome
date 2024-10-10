import TimerLogicNode from "@/nodes/logic/TimerLogicNode.svelte";
import ToggleBtnSensorNode from "@/nodes/device/sensor/ToggleBtnSensorNode.svelte";
import LightActuatorNode from "@/nodes/device/actuator/LightActuatorNode.svelte";
import { NodeType } from "./NodeType";
import SimpleComparatorLogicNode from "@/nodes/logic/SimpleComparatorLogicNode.svelte";
import RangeComparatorLogicNode from "@/nodes/logic/RangeComparatorLogicNode.svelte";
import ThermometerSensorNode from "@/nodes/device/sensor/ThermometerSensorNode.svelte";

export const nodeTypes = {
  [NodeType.Timer]: TimerLogicNode,
  [NodeType.SimpleComparator]: SimpleComparatorLogicNode,
  [NodeType.RangeComparator]: RangeComparatorLogicNode,
  [NodeType.GateLogic]: TimerLogicNode,
  [NodeType.NotGate]: TimerLogicNode,
  [NodeType.Scheduler]: TimerLogicNode,

  [NodeType.ToggleButton]: ToggleBtnSensorNode,
  [NodeType.Thermometer]: ThermometerSensorNode,

  [NodeType.Light]: LightActuatorNode,
};
