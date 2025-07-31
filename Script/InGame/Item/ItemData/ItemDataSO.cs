using UnityEngine;

[CreateAssetMenu(fileName = "ItemDataSO", menuName = "Scriptable Objects/ItemDataSO")]
public class ItemDataSO : ScriptableObject
{
    public string itemName;//나중에 아이템 뿌릴 때 사용할 예정    public ItemType itemType;
    public ItemType ItemType;
    public bool CanEquip;//True이면 장착 가능
    public bool IsMaterial;//재료 여부

 [Header("캐릭터 장착에 들어갈 프리팹")]
   public WeaponType WeaponType;//무기 타입

   [Header("캐릭터 장착 무기 프리팹")]
   public GameObject WeaponPrefab;

   [Header("캐릭터 장착 방어구 프리팹")]
   public GameObject ArmorPrefab; 
   // 
   // //팝업 한정 프리팹팹
   public GameObject ItemPopupPrefab;  // 상황에 따라 재사용할 단일 프리팹

    [Header("가방에 들어갈 프리팹")]
    public GameObject BagItemPrefab;

    [Header("아이템별 능력치")]
      public int HealAmount;        // 체력 회복
    public int StaminaAmount;     // 스태미너 회복
    public int AttackBoost;       // 공격력 증가
    public int DefenseBoost;      // 방어력 증가
}
