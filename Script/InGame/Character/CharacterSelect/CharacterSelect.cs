using UnityEngine;

public class CharacterSelect : MonoBehaviour
{
    [SerializeField]
    private CharacterType _characterType;  // 인스펙터에서 설정

    public void CharacterSelectButton()
    {
        CharacterManager.Instance.SelectCharacter(_characterType);

         // 캐릭터 선택 이후 기본 무기 장착 및 UI 갱신
        //CharacterManager.instance.EquipDefaultWeaponFromStat();
        // UI 갱신은 CharacterManager 내부에서 자동 호출됨


    }
}
