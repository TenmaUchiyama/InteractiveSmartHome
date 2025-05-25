using UnityEditor;
using UnityEngine;
using SpatialLLM.Network;

[CustomEditor(typeof(PromptLLMQueryRequest))]
public class PromptLLMQueryRequestHelper : Editor // クラス名を変更
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        PromptLLMQueryRequest llmQueryRequest = (PromptLLMQueryRequest)target;

        if (GUILayout.Button("Send Query For Debug"))
        {
            PromptLLMQueryRequest.Instance.SendQueryForDebug("テキスト");
        }
    }
}