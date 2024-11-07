
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class UINumericalInputField : MonoBehaviour ,IUIComponent<float>
{
   public UnityEvent<float> OnValueChanged { get; private set; } = new UnityEvent<float>();

   [SerializeField] private InputField inputFieldComponent; 

    private void Start(){
         this.inputFieldComponent = GetComponent<InputField>();
        this.inputFieldComponent.onValueChanged.AddListener((string value)=> OnValueChanged.Invoke(float.Parse(value)));
    }

   
    public void SetInitialValue(float value)
    {
        this.inputFieldComponent.text = value.ToString();
    }


}
