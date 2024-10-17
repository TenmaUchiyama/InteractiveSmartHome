using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(fileName = "MRRoutineEdge", menuName = "Routine/RoutineEdge")]
public class MRNodeSO : ScriptableObject
{
    public Guid id; 
    public string routine_id;
    public object action_data; 
    public Vector3 position;
}
