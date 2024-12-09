using System.Collections;
using System.Collections.Generic;
using NodeTypes;
using Oculus.Interaction;
using UnityEngine;


[System.Serializable]
public class NodeThemeEntry
{
    public ThemeType type;
    public UITheme theme;
}


[CreateAssetMenu (fileName = "NodeThemeMap", menuName = "NodeThemeMap")]
public class NodeThemeMapSO : ScriptableObject
{
   public List<NodeThemeEntry> nodeThemeEntries = new List<NodeThemeEntry>();
    

    public UITheme GetUITheme(ThemeType type)
    {
        return nodeThemeEntries.Find(x => x.type == type).theme;
    }
  
}
