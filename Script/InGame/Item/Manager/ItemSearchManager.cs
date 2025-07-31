using TMPro;
using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using DG.Tweening;
using System.Collections.Generic;
public class ItemSearchManager : MonoBehaviour
{

public PlayerAttack PlayerAttack;
[Header("적 발견")]
    public GameObject FoundEnemy;


//테스트를 위해 만든 캐릭터

[Header("적 캐릭터")]
    public List<Enemy> AllEnemies = new List<Enemy>();
    public Enemy SelectedEnemy { get; private set; }
    


    public BagSlotClickHandler[] BagSlotClickHandlers;
    public static ItemSearchManager Instance;

    public Button SearchButton;
    public GameObject SearchPrefab;
    public GameObject ItemPopupPrefab;
    public Transform DiscoveryParent;

    public PlaceItemManager PlaceItemManager; // PlaceManager 참조

    public bool IsSearching = false;//조사중인지지
    private Vector2 _searchOriginalPos;
    public GameObject PopupObject;

    private void Start()
    {
        Instance = this;

        // 아이콘의 초기 위치 저장
        if (SearchPrefab != null)
        {
            RectTransform rect = SearchPrefab.GetComponent<RectTransform>();
            if (rect != null)
            {
                _searchOriginalPos = rect.anchoredPosition;
            }

            SearchPrefab.SetActive(false); // 시작 시 비활성화
        }

        SearchButton.onClick.AddListener(() =>
        {

           if (DiscoveryParent != null && DiscoveryParent.childCount > 0)
           {
            //Debug.Log("이미 자식 오브젝트가 존재합니다. 탐색 프리팹을 생성하지 않도록 설정합니다.");
            SearchButton.gameObject.SetActive(false);
            }
            else
            {
                SearchButton.gameObject.SetActive(true);
                StartCoroutine(SearchRoutine());
            }
        });
    }



    public IEnumerator SearchRoutine()
{
    if (!PrepareSearch())
        yield break;

    yield return StartCoroutine(ExecuteSearch());
}

public bool PrepareSearch()
{
    var currentPlace = MovePlaceManager.Instance.CurrentPlaceName;
    if (currentPlace == null)
    {
        //Debug.LogWarning("현재 장소가 설정되어 있지 않습니다.");
        return false;
    }

   /// Debug.Log($"[탐색 준비] 현재 장소명: {currentPlace.PlaceNameSetting}");

    PlaceItemManager.Instance.SetCurrentRegionByName(currentPlace.PlaceNameSetting.ToString());
    return true;
}


/*public IEnumerator ExecuteSearch()
{
    //Debug.Log("탐색 시작");

    IsSearching = true;
    SearchButton.gameObject.SetActive(false);

    RectTransform rect = SearchPrefab.GetComponent<RectTransform>();
    if (rect != null)
    {
        rect.anchoredPosition = _searchOriginalPos;
        SearchPrefab.SetActive(true);

        Vector2 originalPos = _searchOriginalPos;

        Sequence seq = DOTween.Sequence();
        for (int i = 0; i < 3; i++)
        {
            seq.Append(rect.DOAnchorPosY(originalPos.y + 30f, 0.4f).SetEase(Ease.OutQuad));
            seq.Append(rect.DOAnchorPosY(originalPos.y, 0.4f).SetEase(Ease.InQuad));
        }

        yield return seq.WaitForCompletion();
    }
    else
    {
        yield return new WaitForSeconds(2.4f);
    }

    SearchPrefab.SetActive(false);

    // 적이 현재 장소에 있으면 바로 등장
    if (IsEnemyInSamePlace())
    {
        EnemyShows();
        SearchButton.gameObject.SetActive(false);
        IsSearching = false;
        yield break;
    }

    // 🎯 아이템 탐색 로직
    float chance = Random.Range(0f, 1f);
    if (chance <= 0.9f)
    {
        var place = PlaceItemManager.Instance.CurrentRegion;
        if (place == null)
        {
            //Debug.LogWarning("탐색할 장소가 선택되지 않았습니다.");
            IsSearching = false;
            yield break;
        }

        ItemDataSO foundItem = place.GetRandomAvailableItem();

        place.DebugPrintAllItemCounts();

        if (foundItem == null)
        {
            //Debug.Log("조사 성공했으나 발견할 아이템이 없음.");
            SearchButton.gameObject.SetActive(true);
            IsSearching = false;
            yield break;
        }

       // Debug.Log("아이템 발견! 팝업 생성 시도");

        if (ItemPopupPrefab == null || DiscoveryParent == null)
        {
            //Debug.LogError("ItemPopupPrefab 또는 DiscoveryParent가 null입니다!");
            IsSearching = false;
            yield break;
        }

        GameObject popupObject = Instantiate(ItemPopupPrefab, DiscoveryParent);
        popupObject.SetActive(true);

        ItemPopupUI popupUI = popupObject.GetComponent<ItemPopupUI>();
        GameObject xButton = popupUI.XButton;
        xButton.SetActive(false);
        //Debug.Log("XButton 비활성화 완료.");

        foreach (var handler in BagSlotClickHandlers)
        {
            if (handler.PopupInstance != null && handler.PopupInstance.activeSelf)
            {
                Destroy(handler.PopupInstance);
                handler.PopupInstance = null;
                //Debug.Log($"{handler.name}의 팝업을 삭제했습니다.");
            }
        }

        popupUI.Setup(foundItem, ItemPopupContext.Discovery);

        if (foundItem.ItemPopupPrefab == null)
        {
           // Debug.LogError("아이템에 연결된 팝업 프리팹이 없습니다!");
            IsSearching = false;
            yield break;
        }

        GameObject actualPopup = Instantiate(foundItem.ItemPopupPrefab, popupUI.ItemPopupParent);
        actualPopup.SetActive(true);
    }
    else
    {
        //Debug.Log("조사 실패! 아이템을 발견하지 못함.");
        SearchButton.gameObject.SetActive(true);
    }

    IsSearching = false;
}*/
public IEnumerator ExecuteSearch()
{
    IsSearching = true;
    SearchButton.gameObject.SetActive(false);

    RectTransform rect = SearchPrefab.GetComponent<RectTransform>();
    if (rect != null)
    {
        rect.anchoredPosition = _searchOriginalPos;
        SearchPrefab.SetActive(true);

        Vector2 originalPos = _searchOriginalPos;

        Sequence seq = DOTween.Sequence();
        for (int i = 0; i < 2; i++)//3
        {
            seq.Append(rect.DOAnchorPosY(originalPos.y + 30f, 0.4f).SetEase(Ease.OutQuad));
            seq.Append(rect.DOAnchorPosY(originalPos.y, 0.4f).SetEase(Ease.InQuad));
        }

        yield return seq.WaitForCompletion();
    }
    else
    {
        yield return new WaitForSeconds(2.4f);
    }

    SearchPrefab.SetActive(false);

    // 플레이어와 같은 장소에 있는 적이 있는지 확인
    Enemy enemyInSamePlace = GetEnemyInSamePlace();
    if (enemyInSamePlace != null)
    {
        ShowEnemyCharacter(enemyInSamePlace);
        SearchButton.gameObject.SetActive(false);
        IsSearching = false;
        yield break;
    }

    // 아이템 탐색 로직
    float chance = Random.Range(0f, 1f);
    if (chance <= 0.9f)
    {
        var place = PlaceItemManager.Instance.CurrentRegion;
        if (place == null)
        {
            IsSearching = false;
            yield break;
        }

        ItemDataSO foundItem = place.GetRandomAvailableItem();
        place.DebugPrintAllItemCounts();

        if (foundItem == null)
        {
            SearchButton.gameObject.SetActive(true);
            IsSearching = false;
            yield break;
        }

        if (ItemPopupPrefab == null || DiscoveryParent == null)
        {
            IsSearching = false;
            yield break;
        }

        GameObject popupObject = Instantiate(ItemPopupPrefab, DiscoveryParent);
        popupObject.SetActive(true);

        ItemPopupUI popupUI = popupObject.GetComponent<ItemPopupUI>();
        GameObject xButton = popupUI.XButton;
        xButton.SetActive(false);

        foreach (var handler in BagSlotClickHandlers)
        {
            if (handler.PopupInstance != null && handler.PopupInstance.activeSelf)
            {
                Destroy(handler.PopupInstance);
                handler.PopupInstance = null;
            }
        }

        popupUI.Setup(foundItem, ItemPopupContext.Discovery);

        if (foundItem.ItemPopupPrefab == null)
        {
            IsSearching = false;
            yield break;
        }

        GameObject actualPopup = Instantiate(foundItem.ItemPopupPrefab, popupUI.ItemPopupParent);
        actualPopup.SetActive(true);
    }
    else
    {
        // 아이템 발견 실패
        SearchButton.gameObject.SetActive(true);
    }

    IsSearching = false;
}


//적 관련
//적 관련
private Enemy GetEnemyInSamePlace()
{
    var playerPlace = MovePlaceManager.Instance?.CurrentPlaceName?.PlaceNameSetting;
    if (playerPlace == null) return null;

    foreach (var enemy in AllEnemies)
    {
        // ✅ 먼저 적이 죽었는지 확인
        if (enemy.IsDead)
        {
            Debug.Log($"[탐색] {enemy.name}은 이미 죽어있어서 탐색에서 제외됩니다.");
            //SearchButton.gameObject.SetActive(true);
            continue; // 죽은 적은 건너뛰기
        }

        var enemyAI = enemy.GetComponent<AIMovePlaceManager>();
        if (enemyAI != null && enemyAI.CurrentPlaceNameType == playerPlace)
        {
            if (enemy.EnemyCharacter != null && !enemy.EnemyCharacter.activeSelf)
            {
                SelectedEnemy = enemy; // 여기서 할당
                Debug.Log($"[탐색] 살아있는 적 발견: {enemy.name}");
                SearchButton.gameObject.SetActive(false);
                return enemy;
            }
        }
    }

    SelectedEnemy = null; // 적 없을 때 null 처리
    Debug.Log("[탐색] 같은 장소에 살아있는 적이 없습니다.");
    SearchButton.gameObject.SetActive(true);
    return null;
}
/*private Enemy GetEnemyInSamePlace()
{
    var playerPlace = MovePlaceManager.Instance?.CurrentPlaceName?.PlaceNameSetting;
    if (playerPlace == null) return null;

    foreach (var enemy in AllEnemies)
    {
        var enemyAI = enemy.GetComponent<AIMovePlaceManager>();
        if (enemyAI != null && enemyAI.CurrentPlaceNameType == playerPlace)
        {
            if (enemy.EnemyCharacter != null && !enemy.EnemyCharacter.activeSelf)
            {
                SelectedEnemy = enemy; // 여기서 할당
                return enemy;
            }
        }
    }

    SelectedEnemy = null; // 적 없을 때 null 처리
    return null;
}
*/

private void ShowEnemyCharacter(Enemy enemy)
{
    Debug.Log($"적 발견! - {enemy.name}");

    if (enemy.EnemyCharacter != null)
    {
        RectTransform enemyRect = enemy.EnemyCharacter.GetComponent<RectTransform>();

        if (enemyRect != null)
        {
            enemy.EnemyCharacter.SetActive(true);
            FoundEnemy.SetActive(true);
            PlayerAttack.AttackButton.interactable = true;
            enemyRect.anchoredPosition = new Vector2(1500f, enemyRect.anchoredPosition.y);
            enemyRect.DOAnchorPosX(0f, 0.3f).SetEase(Ease.OutExpo);
        }
        else
        {
            enemy.EnemyCharacter.SetActive(true);
        }
    }
}
}