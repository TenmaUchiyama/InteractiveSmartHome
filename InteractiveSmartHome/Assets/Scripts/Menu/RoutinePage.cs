using System.Collections;
using System.Collections.Generic;
using MRFlow.Core;
using NodeTypes;
using UnityEngine;



namespace MRFlow.UI 
{
public class RoutinePage : MonoBehaviour
{


    List<RoutineRow> routineRowList = new List<RoutineRow>();
    [SerializeField] private GameObject routineRowPrefab; 
    [SerializeField] private GameObject routineContentRoot;




    void Start()
    {
        List<MRRoutineEdgeData> mrRoutineEdgeDatas = RoutineEdgeManager.Instance.GetMRROutineEdgeDatas();
        AddRoutineRows(mrRoutineEdgeDatas);
    }

    public void AddRoutineRows(List<MRRoutineEdgeData> mrRoutineEdgeDatas)
    {

        Debug.Log($"[RoutinePage] AddRoutineRows {mrRoutineEdgeDatas.Count}");
        foreach (MRRoutineEdgeData mrRoutineEdgeData in mrRoutineEdgeDatas)
        {
            AddRoutineRow(mrRoutineEdgeData);
        }
    }

    public void AddRoutineRow(MRRoutineEdgeData mrRtoueinEdgeData)
    {
      

        GameObject routineRowObject = Instantiate(routineRowPrefab, routineContentRoot.transform);
        RoutineRow routineRow = routineRowObject.GetComponent<RoutineRow>();
        string no = (routineRowList.Count + 1).ToString();
        routineRow.SetRoutineData(no, mrRtoueinEdgeData);
        routineRowList.Add(routineRow);
    }
}
}