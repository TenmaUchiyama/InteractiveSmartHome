using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class UIDropdown : MonoBehaviour,IUIComponent<string>
{
   public UnityEvent<string> OnValueChanged { get; private set; }= new UnityEvent<string>();

   [SerializeField]private TMP_Dropdown dropdown; 



   

    private void Start(){
         dropdown = GetComponent<TMP_Dropdown>();
          dropdown.onValueChanged.AddListener((int value)=> OnValueChanged.Invoke(dropdown.options[value].text));
    }

    public void SetInitialValue(string value)
    {
            for (int i = 0; i < dropdown.options.Count; i++)
            {
               if (dropdown.options[i].text == value)
               {
                     dropdown.value = i;
                     break;
               }
            }
    }
}
