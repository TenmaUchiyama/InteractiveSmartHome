using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EXDataHolder : Singleton<EXDataHolder>
{
    [Header("Experiment Metadata")]
    [SerializeField] private string participantName = "P1";
    [SerializeField] private string conditionName = "ConditionA";
    [SerializeField] private string taskSetName = "A";

    // 外部アクセス用プロパティ
    public string ParticipantName => participantName;
    public string ConditionName => conditionName;
    public string TaskSetName => taskSetName;

    // Singleton化されたインスタンスは `EXDataHolder.Instance` でアクセス可能
}