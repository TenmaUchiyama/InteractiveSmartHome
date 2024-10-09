export interface IDBNode {
  id: string;
  type: string;
  data_action_id: string;
  position: { x: number; y: number };
}

export interface IEdge {
  id: string;
  source: string;
  target: string;
}

export interface IDBEdge {
  id: string;
  associated_routine_id: string;
  edges: IEdge[];
}
