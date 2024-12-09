using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class UIToggle : MonoBehaviour, IUIComponent<bool>
{


     public UnityEvent<bool> OnValueChanged { get; private set; } = new UnityEvent<bool>();

    [SerializeField] private Toggle toggleButton;


     private void Awake() {
          
          toggleButton.onValueChanged.AddListener((bool value) => {OnValueChanged.Invoke(value); Debug.Log("It's called");});
     }

     public void SetInitialValue(bool value)
     {
          Debug.Log("Setting initial value");
          toggleButton.isOn = value;
     }




}
