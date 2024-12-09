using System.Collections;
using System.Collections.Generic;
using MRFlow.Component;
using Oculus.Interaction;
using Oculus.Interaction.Input.Visuals;
using Unity.VisualScripting;
using UnityEngine;

public class SelectingLine : MonoBehaviour
{

    [SerializeField] GameObject selectingLinePrefab;
    [SerializeField] RayInteractor rayInteractor; 
    private NodeHandler selectingMrNode; 
    private GameObject line;
    private LineRenderer lineRenderer; 

    

    private MeshRenderer meshRenderer;

    public void SetNodeHandler(NodeHandler node)
    {
        

        if(node == null)
        {
            this.ClearLine();
            return;
        }



        this.line = Instantiate(selectingLinePrefab, this.transform);
        this.lineRenderer = line.GetComponent<LineRenderer>();
        this.selectingMrNode = node;

        
    }

    public void ClearLine() 
    {
        this.selectingMrNode = null;
       if (line != null) 
    {
        Destroy(this.line);  
        this.line = null;
    }
        this.line = null;

   

    }

    private void Update() {
        if(!lineRenderer || !this.selectingMrNode) return;
    
        lineRenderer.SetPosition(0, this.selectingMrNode.transform.position);
        lineRenderer.SetPosition(1, this.transform.position); 
     
    }

     void OnDisable()
    {
       this.ClearLine();
    }
}
