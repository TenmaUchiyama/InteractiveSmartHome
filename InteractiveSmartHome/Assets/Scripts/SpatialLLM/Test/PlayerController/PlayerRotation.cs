using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerRotation : MonoBehaviour
{
   public float mouseXSensitivity = 100f;

        public Transform playerBody;

        float xRotation = 0f;

        // Start is called before the first frame update
        void Start()
        {
          
        }

        // Update is called once per frame
        void Update()
        {
            float mouseX = OVRInput.Get(OVRInput.RawAxis2D.RThumbstick).x;
       
            playerBody.Rotate(Vector3.up * mouseX);


            


        }

}
