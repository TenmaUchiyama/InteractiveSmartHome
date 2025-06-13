using UnityEditor;
using UnityEngine;
using SpatialLLM.Network;

[CustomEditor(typeof(LLMQueryRequest))]
public class LLMQueryRequestHelper : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        LLMQueryRequest llmQueryRequest = (LLMQueryRequest)target;

        if (GUILayout.Button("Send Query For Debug"))
        {
            // llmQueryRequest.SendQueryForDebug("Debug Text");
        }
    }
}
