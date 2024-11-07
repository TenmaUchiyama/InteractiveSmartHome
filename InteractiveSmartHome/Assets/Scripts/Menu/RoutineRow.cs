using System.Collections;
using System.Collections.Generic;
using MRFlow.Core;
using MRFlow.ServerController;
using NodeTypes;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RoutineRow : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI noText; 
    [SerializeField] private TextMeshProUGUI routineName; 
    [SerializeField] private Button toggleButton; 

    [SerializeField] private TextMeshProUGUI statusText; 





    public void SetRoutineData(string no, MRRoutineEdgeData routineEdgeData)
    {
        noText.text = no;
        routineName.text = routineEdgeData.routine_name;
        statusText.text = "Not Active";

        toggleButton.onClick.AddListener(() =>
        {
            RoutineEdgeManager.Instance.StartRoutine(routineEdgeData);
        });
    }



    public void SetStatusText(string status)
    {
        statusText.text = status;
    }

     





}
