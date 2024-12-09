using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class UIInputField : MonoBehaviour,IUIComponent<string>
{
   public UnityEvent<string> OnValueChanged { get; private set; } = new UnityEvent<string>();

   [SerializeField] private InputField inputFieldComponent; 

    private void Start(){
         this.inputFieldComponent = GetComponent<InputField>();
        this.inputFieldComponent.onValueChanged.AddListener((string value)=> OnValueChanged.Invoke(value));
    }

    public void SetInitialValue(string value)
    {
        this.inputFieldComponent.text = value;
    }

    public string GetValue()
    {
        return this.inputFieldComponent.text;
    }




}
