using System.ComponentModel;
using UnityEditor;
using UnityEngine;

public class FindComponentInScene : EditorWindow
{
    [MenuItem("Tools/Find Specific Component")]
    private static void FindSpecificComponent()
    {
        // 例: Scene内のすべての "MyComponent" を持つオブジェクトを探す
        MoveToPosition[] components = FindObjectsOfType<MoveToPosition>(true);

        foreach (var component in components)
        {
            Debug.Log("Found: " + component.gameObject.name, component.gameObject);
        }
    }
}
