using System;
using System.Collections;
using System.Collections.Generic;
using NodeTypes;
using UnityEngine;

[CreateAssetMenu(fileName = "NodeObjectMap", menuName = "Node Object Map", order = 1)]

public class NodeObjectMapSO : ScriptableObject
{
    [Serializable]
    public class NodeObjectMap
    {
        public NodeType nodeType;
        public GameObject nodeObject;
    }


    public List<NodeObjectMap> nodeObjectMaps = new List<NodeObjectMap>();

    public GameObject GetNodeObject(NodeType nodeType)
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
