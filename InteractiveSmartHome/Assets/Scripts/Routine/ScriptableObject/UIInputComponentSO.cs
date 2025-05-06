using System.Collections;
using System.Collections.Generic;
using UnityEngine;



namespace MRFlow.UI{


public enum UIInputComponentType
{
    Toggle,
    Slider,
    Dropdown,
    InputField,
    NumericalInputFiel,
    Button
}

[CreateAssetMenu(fileName = "UIComponentSO")]
public class UIInputComponentSO : ScriptableObject
{
   [System.Serializable]
public struct UIInputComponentTypeMap
{
    public UIInputComponentType type; 
    public GameObject uiInputComponentPrefab; 
}



public List<UIInputComponentTypeMap> inputComponentTypeMap = new List<UIInputComponentTypeMap>();


public GameObject GetUIInputComponentPrefab(UIInputComponentType type)
{
    foreach (var map in inputComponentTypeMap)
    {
        if(map.type == type)
        {
            return map.uiInputComponentPrefab; 
        }
    }
    return null; 

}


}
}