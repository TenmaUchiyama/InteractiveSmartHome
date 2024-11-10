export interface IMqttDataType {
  value_type: "string" | "number" | "boolean" | "json";
  value: string | number | boolean | object;
}
