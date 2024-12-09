using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class UISlider : MonoBehaviour , IUIComponent<float>
{
   public UnityEvent<float> OnValueChanged { get; private set; } = new UnityEvent<float>();

   [SerializeField]private Slider slider; 

    private void Start(){
         slider = GetComponent<Slider>();
         slider.onValueChanged.AddListener((float value)=> OnValueChanged.Invoke(value));
    }

    public void SetInitialValue(float value)
    {
     slider.value = value;
    }
}
