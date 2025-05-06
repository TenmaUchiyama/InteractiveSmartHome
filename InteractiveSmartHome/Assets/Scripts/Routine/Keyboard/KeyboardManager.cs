using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class KeyboardManager : Singleton<KeyboardManager>
{
    

    [SerializeField] private Transform cameraRigTransform;
   
    [SerializeField] private Transform mainCameraTransform;

    [SerializeField] private OVRVirtualKeyboard keyboard;
    [SerializeField] private KeyboardVisual keyboardVisual;
    
    public Vector3 offsetFromCamera = new Vector3(0, -0.5f, 0.1f);


    private OVRVirtualKeyboardInputFieldTextHandler inputFieldTextHandler;



    private void Start() {
        this.inputFieldTextHandler = this.GetComponent<OVRVirtualKeyboardInputFieldTextHandler>();
        this.CloseKeybaord(); 

        this.keyboardVisual = GetComponent<KeyboardVisual>();

        keyboard.EnterEvent.AddListener(OnEnterKeyboard); 
        keyboard.KeyboardHiddenEvent.AddListener(CloseKeybaord);
    }


    void OnDestroy()
    {
        keyboard.EnterEvent.RemoveListener(OnEnterKeyboard);
    }

    private void OnEnterKeyboard()
    {

        this.OpenKeyboard();
    }

    void TransformKeyboard()
    {
        Vector3 offsetPosition = mainCameraTransform.TransformPoint(offsetFromCamera);
        keyboard.transform.position = offsetPosition;


        keyboard.transform.LookAt(mainCameraTransform);
        keyboard.transform.Rotate(0, 180, 0);
        
        keyboard.UseSuggestedLocation(OVRVirtualKeyboard.KeyboardPosition.Far);
    }




    public void OnInputFieldSelected(InputField inputField)
    {
        this.inputFieldTextHandler.InputField = inputField;
        this.OpenKeyboard();

    }


    public void OnInputFieldDeselected()
    {
      this.CloseKeybaord();
    }



    public void CloseKeybaord() 
    {
        keyboard.gameObject.SetActive(false);
        this.inputFieldTextHandler.InputField = null;
        this.keyboardVisual.ToggleKeyboardVisual(false);    
    }

    public void OpenKeyboard()
    {

        this.TransformKeyboard();
        keyboard.gameObject.SetActive(true);
        Debug.Log($"<color=green>{this.keyboardVisual != null} </color>");
        this.keyboardVisual.ToggleKeyboardVisual(true);
    }
}
