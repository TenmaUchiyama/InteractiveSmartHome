using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ShowLabel : MonoBehaviour
{
   [SerializeField] TextMeshPro labelText;
    [SerializeField] string initLabel = "";


    void Start()
    {
        labelText.text = initLabel == "" ? this.gameObject.name : initLabel;
    }

    public void DisplayLabel() 
   {
        labelText.gameObject.SetActive(true);
   }

   public void HideLabel() 
   {
    labelText.gameObject.SetActive(false);
   }


    void Update()
    {
        this.labelText.transform.LookAt(Camera.main.transform); 
        this.labelText.transform.Rotate(0,180,0);
    }
}
