using UnityEngine;

[CreateAssetMenu(fileName = "CharacterStatSO", menuName = "Scriptable Objects/CharacterStatSO")]
public class CharacterStatSO : ScriptableObject
{

  public string CharacterName;



  [Header("캐릭터")]
    public CharacterType CharacterType;
    public GameObject CharacterPrefab;

    public GameObject CharacterInfoGrayPrefab;

  [Header("기본 장착 무기")]
   public WeaponType DefaultWeaponType;
   public GameObject DefaultWeaponPrefab;


    [Header("기본 스탯: 체력 스테미너 방어력")]
    
public int Defense;

public int MaxStamina;

public int MaxHealth;


    [Header("무기 숙련도(공격력)")]
   
   public int Magic;//마법
     public int Sword;//검
   public int Blunt;//둔기
   public int Fist;//주먹
     
    public int Bow;//활
    public int Throwing;//던지기
    public int Gun;//총
    
   
    
    
}
