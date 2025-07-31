using UnityEngine;

[CreateAssetMenu(fileName = "SkillDataSO", menuName = "Scriptable Objects/SkillDataSO")]
public class SkillDataSO : ScriptableObject
{
    public string SkillName;
    public GameObject LeftTopSkill; 
    public float CooldownTime;
    public bool IsCombatOnly; // 전투 중에만 사용 가능한가?
}
