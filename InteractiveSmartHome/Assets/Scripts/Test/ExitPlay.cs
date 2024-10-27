using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class ExitPlay : MonoBehaviour
{
     void Update()
    {
        if (Input.GetKeyDown(KeyCode.Backspace))
        {
            // UnityEditorでのみ動作
            #if UNITY_EDITOR
            EditorApplication.isPlaying = false;
            #endif
        }
    }
}
