import TimerLogicNode from "@/nodes/logic/TimerLogicNode.svelte";
import ToggleBtnSensorNode from "@/nodes/device/sensor/ToggleBtnSensorNode.svelte";
import LightActuatorNode from "@/nodes/device/actuator/LightActuatorNode.svelte";
import { NodeType } from "./NodeType";
import SimpleComparatorNode from "@/nodes/logic/SimpleComparatorNode.svelte";

export const nodeTypes = {
  [NodeType.Timer]: TimerLogicNode,
  [NodeType.Light]: LightActuatorNode,
  [NodeType.ToggleButton]: ToggleBtnSensorNode,
  [NodeType.SimpleComparator]: SimpleComparatorNode,
};
