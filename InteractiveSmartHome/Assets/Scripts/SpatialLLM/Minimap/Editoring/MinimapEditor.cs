using System;
using System.Collections;
using System.Collections.Generic;
using Meta.XR.MRUtilityKit;
using SpatialLLM.Device;
using TMPro;
using Unity.PlasticSCM.Editor.WebApi;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.iOS;
using UnityEngine.UI;
using static SpatialLLM.Network.NetworkDataType;

public class MinimapEditor : MonoBehaviour
{
    
    

    
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private GameObject editorParent;

    [SerializeField] private Button closeButton;



    private void Start() {
        closeButton.onClick.AddListener(() => {
           this.gameObject.SetActive(false);
        });
    }

    

    public void SpawnEditor(MinimapEditorSO minimapEditorSO, SADevice device) 
    {
        titleText.text = device.gameObject.name;

        if (editorParent.transform.childCount > 0)
        {
            foreach (Transform child in editorParent.transform)
            {
                Destroy(child.gameObject);
            }
        }

        GameObject editorPrefab = Instantiate(minimapEditorSO.editorPrefab, editorParent.transform);


        foreach(Transform child in editorPrefab.transform)
        {
            IMinimapEditor minimapEditor = child.GetComponent<IMinimapEditor>();
            
            if (minimapEditor != null)
            {
                minimapEditor.OnUIValueChanged(device);
            }
        }
       


        
    }
}
