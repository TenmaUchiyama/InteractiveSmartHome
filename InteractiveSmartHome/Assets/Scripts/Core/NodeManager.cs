using System.Collections;
using System.Collections.Generic;
using MRFlow.Component;
using UnityEngine;

public class NodeManager : Singleton<NodeManager>
{
    private List<MRNode> nodeList = new List<MRNode>();
    public List<MRNode> NodeList => nodeList;
    private List<MRNode> currentNodes = new List<MRNode>();

    public List<MRNode> CurrentNodes => currentNodes;

    [SerializeField] private Transform nodeSpanwPoint; 



    public void AddNode(MRNode node)
    {
        
    }

    


}
