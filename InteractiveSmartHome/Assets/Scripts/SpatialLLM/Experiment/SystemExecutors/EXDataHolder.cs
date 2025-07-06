using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EXDataHolder : Singleton<EXDataHolder>
{
    [Header("Experiment Metadata")]
    [SerializeField] private bool isSystemEvaluation = false;
    public bool IsSystemEvaluation => isSystemEvaluation;

    [SerializeField] private string participantName = "P1";
    [SerializeField] private string taskSetName = "A";

    [SerializeField] private string conditionNum = "ConditionA" ;

    // 外部アクセス用プロパティ
    public string ParticipantName => participantName;
    public string ConditionName => conditionNum;
    public string TaskSetName => taskSetName;

    // Singleton化されたインスタンスは `EXDataHolder.Instance` でアクセス可能
    

    


}