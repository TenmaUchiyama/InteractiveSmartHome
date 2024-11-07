using System.Collections;
using System.Collections.Generic;
using ActionDataTypes.Logic;
using NodeTypes;
using UnityEngine;
using UnityEngine.UI;

public class EditorTimerNode : MonoBehaviour
{
  [SerializeField] private InputField nameInputField; 

  [SerializeField] private InputField descriptionInputField;
  [SerializeField] private InputField durationInputField; 



 private MRNodeData mrNodeData;

    public void SetMRNodeData(MRNodeData newMRNodeData)
    {
        mrNodeData = newMRNodeData;

        if (mrNodeData != null)
        {
            // 初期値を UI に表示
            nameInputField.text = mrNodeData.action_data.name;
            descriptionInputField.text = mrNodeData.action_data.description;
            durationInputField.text = (mrNodeData.action_data as TimerBlockData).waitTime.ToString();

            // InputField の onEndEdit イベントにリスナーを追加
            nameInputField.onEndEdit.AddListener(OnNameChanged);
            descriptionInputField.onEndEdit.AddListener(OnDescriptionChanged);
            durationInputField.onEndEdit.AddListener(OnDurationChanged);
        }
    }

    private void OnNameChanged(string newName)
    {
        if (mrNodeData != null)
        {
            mrNodeData.action_data.name = newName;
        }
    }

    private void OnDescriptionChanged(string newDescription)
    {
        if (mrNodeData != null)
        {
            mrNodeData.action_data.description = newDescription;
        }
    }

    private void OnDurationChanged(string newDuration)
    {
        if (mrNodeData != null && int.TryParse(newDuration, out int duration))
        {
            (mrNodeData.action_data as TimerBlockData).waitTime = duration;
        }
    }
    




  

}
