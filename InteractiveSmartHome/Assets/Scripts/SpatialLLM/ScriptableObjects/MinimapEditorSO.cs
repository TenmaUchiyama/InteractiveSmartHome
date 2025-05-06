using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "MinimapEditorSO", menuName = "Minimap/MinimapEditorSO", order = 1)]
public class MinimapEditorSO : ScriptableObject
{
    public string title; 
    public GameObject editorPrefab;
    
}
