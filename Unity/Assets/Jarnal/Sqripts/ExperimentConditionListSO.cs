using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 実験条件（波モード×アクチュエータスケール）のリストと進行状況を保持するアセット。
/// ExperimentManager と JarnalWaitController の両方から同じアセットを参照することで、
/// シーンをまたいで「今どの条件を実行するか」を共有する。
/// </summary>
[CreateAssetMenu(fileName = "ExperimentConditionList", menuName = "ScriptableObjects/ExperimentConditionListSO", order = 1)]
public class ExperimentConditionListSO : ScriptableObject
{
    [Serializable]
    public struct Condition
    {
        public ExperimentManager.WaveMode waveMode;
        [Range(0f, 100f)] public float actuatorScale;
    }

    [Tooltip("実施する条件を上から順番に実行する")]
    public List<Condition> conditions = new List<Condition>();

    [Tooltip("次に実行する条件のインデックス（0始まり）。新しい参加者を始める前に0に戻すこと")]
    public int currentIndex = 0;

    public bool HasCurrent => currentIndex >= 0 && currentIndex < conditions.Count;

    public Condition Current => conditions[currentIndex];

    public void Advance() => currentIndex++;

    public void ResetProgress() => currentIndex = 0;
}
