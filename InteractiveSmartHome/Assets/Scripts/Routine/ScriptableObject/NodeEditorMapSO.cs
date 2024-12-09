using System;
using System.Collections;
using System.Collections.Generic;
using NodeTypes;
using UnityEngine;




[CreateAssetMenu (fileName = "NodeEditorMap", menuName = "NodeEditorMap")]
public class NodeEditorMapSO : ScriptableObject
{
  [Serializable]
    public class NodeEditorMap
    {
        public NodeType nodeType;
        public GameObject nodeObject;
    }


    public List<NodeEditorMap> nodeObjectMaps = new List<NodeEditorMap>();

    public GameObject GetNodeEditor(NodeType nodeType)
    {
        foreach (var nodeObjectMap in nodeObjectMaps)
        {
            if (nodeObjectMap.nodeType == nodeType)
            {
                return nodeObjectMap.nodeObject;
            }
        }

        return null;
    }   
}
