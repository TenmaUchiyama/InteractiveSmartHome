using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SpatialLLM.Device;


#if UNITY_EDITOR
using UnityEditor;
#endif


namespace SpatialLLM.Debugging
{
    
public class DebugLight : MonoBehaviour
{
  
 public GameObject parentObject; // Assign in Inspector
    public GameObject basePrefab; // Assign in Inspector
    public Light spotLight; // Assign in Inspector
    public string baseName = "LackedLight";



    #if UNITY_EDITOR

    [ContextMenu("入れ替える")]
    public void SwapObjects()
    {
        this.CopyChildren(); 
        this.ProcessChildObjects();
    }

    [ContextMenu("Process Child Objects")]
    void ProcessChildObjectsEditor()
    {
        ProcessChildObjects();
    }


    
 

    private int objectCount = 0; 
    void ProcessChildObjects()
    {
        Transform[] children = new Transform[parentObject.transform.childCount];

            for (int i = 0; i < parentObject.transform.childCount; i++)
            {
                children[i] = parentObject.transform.GetChild(i);
            }
        foreach (Transform child in children)
        {  
            
            
            objectCount++;

            Debug.Log("Processing " + child.name);
    
            MeshRenderer meshRenderer = child.GetComponent<MeshRenderer>();
            BoxCollider childCollider = child.GetComponent<BoxCollider>();

            if (meshRenderer == null || childCollider == null)
            {
                Debug.LogError($"Skipping {child.name} as it does not have both MeshRenderer and BoxCollider.");
                continue;
            }

            // Create a new object from basePrefab
            GameObject newBaseObject = Instantiate(basePrefab, child.position, child.rotation, parentObject.transform);
            newBaseObject.name = baseName + objectCount;
            newBaseObject.AddComponent<SALight>();
            // Copy BoxCollider properties from child to new base object
            BoxCollider newCollider = newBaseObject.GetComponent<BoxCollider>();
            if (newCollider != null)
            {
                CopyBoxColliderProperties(childCollider, newCollider);
            }
            else
            {
                Debug.LogWarning("Base prefab does not have a BoxCollider. Please ensure it has one.");
            }

            DestroyImmediate(childCollider);

   
            // Make child a child of the new base object
            child.SetParent(newBaseObject.transform);

            // Instantiate a new spotlight and make it a child of the new base object
            Light newLight = Instantiate(spotLight, newBaseObject.transform);
            newLight.transform.localPosition = Vector3.zero; // Adjust as necessary
            newLight.name = "SpotLight_" + child.name;
        }

        objectCount = 0;
    }

    void CopyBoxColliderProperties(BoxCollider source, BoxCollider target)
    {
        target.center = source.center;
        target.size = source.size;
        target.isTrigger = source.isTrigger;
        target.contactOffset = source.contactOffset;
    }



    public GameObject FromParentObject;
    public GameObject ToParentObject;


    [ContextMenu("Copy Children")]
    public void CopyChildren()
    {
        if (FromParentObject == null || ToParentObject == null)
        {
            Debug.LogWarning("One or both parent objects are null.");
            return;
        }


        Transform[] toChildren = new Transform[ToParentObject.transform.childCount];
        for (int i = 0; i < ToParentObject.transform.childCount; i++)
        {
            toChildren[i] = ToParentObject.transform.GetChild(i);
        }

        // Step 1: Delete all existing child objects of ToParentObject
        foreach (Transform child in toChildren)
        {
            DestroyImmediate(child.gameObject);
        }

        Transform[] fromChildren = new Transform[FromParentObject.transform.childCount];
        for (int i = 0; i < FromParentObject.transform.childCount; i++)
        {
            fromChildren[i] = FromParentObject.transform.GetChild(i);
        }

        // Step 2: Copy all child objects from FromParentObject to ToParentObject
        foreach (Transform child in fromChildren)
        {
            GameObject newChild = Instantiate(child.gameObject, ToParentObject.transform);
            newChild.name = child.name; // 名前を保持（オプション）
        }

        Debug.Log("Copied all child objects from FromParentObject to ToParentObject.");
    }

       #endif
}
}