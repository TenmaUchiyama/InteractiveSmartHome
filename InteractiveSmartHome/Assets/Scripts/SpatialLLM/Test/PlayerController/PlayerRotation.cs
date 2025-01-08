using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static PlayerMovement;

public class PlayerRotation : MonoBehaviour
{
   public float mouseXSensitivity = 100f;

        public Transform playerBody;

        float xRotation = 0f;


        private InputMode currentSelectedMode = InputMode.Keyboard;

        // Start is called before the first frame update
        void Start()
        {
          currentSelectedMode = GetComponent<PlayerMovement>().SelectedInputMode;
        }

        // Update is called once per frame
        void Update()
        {

            float mouseX = 0.0f;
            if(currentSelectedMode == InputMode.Keyboard)
            {
                mouseX = Input.GetAxis("Mouse X") * mouseXSensitivity * Time.deltaTime;
                float mouseY = Input.GetAxis("Mouse Y") * mouseXSensitivity * Time.deltaTime;

                xRotation -= mouseY; 
                xRotation = Mathf.Clamp(xRotation, -90f, 90f);

                transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
            }else{

            mouseX = OVRInput.Get(OVRInput.RawAxis2D.RThumbstick).x;
            }
       
            playerBody.Rotate(Vector3.up * mouseX);


            


        }

}
