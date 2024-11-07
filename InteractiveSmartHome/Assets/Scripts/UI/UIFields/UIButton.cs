
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class UIButton : MonoBehaviour ,IUIComponent<bool>
{
   public UnityEvent<bool> OnValueChanged { get; private set; } = new UnityEvent<bool>();

   [SerializeField]private Button button; 

    private void Start(){
         button = GetComponent<Button>();
         button.onClick.AddListener(()=> OnValueChanged.Invoke(true));
    }

     public void SetInitialValue(bool value)
     {
          // Do nothing
     }


}
