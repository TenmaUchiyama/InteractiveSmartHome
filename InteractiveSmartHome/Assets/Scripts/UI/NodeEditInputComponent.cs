using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class NodeEditInputComponent : MonoBehaviour
{
   [SerializeField] private TextMeshProUGUI titleText; 

   [SerializeField] private GameObject inputCompoentObject; 






   public void SetNodeEditorComponent<T>(string title, Action<T> onValueChangedCallback, T initValue)
   {
      Debug.Log($"Setting Node Editor Component: {title} Initial Value: {initValue}");
       titleText.text = title;
       if(inputCompoentObject.TryGetComponent<IUIComponent<T>>(out IUIComponent<T> inputComponent))
       {
            Debug.Log("Toggle component found" + inputComponent);
           inputComponent.SetInitialValue(initValue);
            inputComponent.OnValueChanged.AddListener((T val) => 
            { 
                Debug.Log("value changed: " + val);
                Debug.Log("Value changed: " + val); 
                onValueChangedCallback.Invoke(val);
                
            });
       }
   }
}
