using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(DeviceEditor))]
public class DeviceEditor : Editor
{


     public GameObject targetObject;


    void OnSceneGUI()
    {
        // Sceneビューでクリックされたオブジェクトを取得
        if (Event.current.type == EventType.MouseDown && Event.current.button == 0)
        {
            Ray ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit))
            {
                // 選択したオブジェクトをインスペクターに表示
                DeviceEditor component = (DeviceEditor)target;
                component.targetObject = hit.collider.gameObject;
                Debug.Log("Selected Object: " + component.targetObject.name);
            }
        }
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        DeviceEditor component = (DeviceEditor)target;
        EditorGUILayout.ObjectField("Target Object", component.targetObject, typeof(GameObject), true);
    }
}
