using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
     public CharacterController controller;

        public float speed = 5f;
        public float gravity = -15f;


        enum InputMode {
            MetaQuest3, 
            Keyboard,
        }

        [SerializeField] private  InputMode inputMode = InputMode.Keyboard;

        Vector3 velocity;

        bool isGrounded;

        // Update is called once per frame
        void Update()
        {
            


            float x = 0.0f; 
            float z = 0.0f;

            if (inputMode == InputMode.Keyboard)
            {
                x = Input.GetAxis("Horizontal");
                z = Input.GetAxis("Vertical");
            }else{
                Vector2 controllerInput = OVRInput.Get(OVRInput.Axis2D.SecondaryThumbstick);
                x = controllerInput.x;
                z = controllerInput.y;
            }

            
           Vector3 forward = Camera.main.transform.forward; // カメラの前方向
            Vector3 right = Camera.main.transform.right;     // カメラの右方向

            // 上方向の影響を取り除く（yを0にする）
            forward.y = 0f;
            right.y = 0f;

            // 正規化して方向ベクトルを調整
            forward.Normalize();
            right.Normalize();

            // 移動ベクトルを計算
            Vector3 move = right * x + forward * z;

            // 移動処理
            controller.Move(move * speed * Time.deltaTime);

        }

}
