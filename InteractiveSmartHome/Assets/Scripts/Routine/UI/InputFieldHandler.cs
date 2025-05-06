using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using Newtonsoft.Json;
using UnityEngine.UI;
public class InputFieldHandler : MonoBehaviour, ISelectHandler, IDeselectHandler
{


    private InputField inputField;


    private void Start() {
        inputField = GetComponent<InputField>();
    }
    public void OnDeselect(BaseEventData eventData)
    {
        KeyboardManager.Instance.OnInputFieldDeselected();
    }

    public void OnSelect(BaseEventData eventData)
    {
      KeyboardManager.Instance.OnInputFieldSelected(this.inputField);
    }

   
 

    
}
