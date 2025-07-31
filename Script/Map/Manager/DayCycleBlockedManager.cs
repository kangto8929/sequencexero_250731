using UnityEngine;

public class DayCycleBlockedManager : MonoBehaviour
{
    [SerializeField]
    private TimeFlowManager _timeFlowManager;

    [SerializeField]
    private BlockedPlaceApplier _blockPlaceApplier;

    public void OnTimeAdvance()
    { 
        switch (_timeFlowManager.CurrentStep)
        {
            case 0:
                _blockPlaceApplier.ShowBlockedPlaces(0);//1���� �� �ȳ�
                //Debug.Log("1일차 금지구역 안내");
                break;
            case 1:
                _blockPlaceApplier.ApplyDay(0);
                //Debug.Log("1일차 금지구역 막음");
                break;
            case 2:
                _blockPlaceApplier.RestoreDay(0);
                _blockPlaceApplier.ShowBlockedPlaces(1);//2���� �� �ȳ�
                //Debug.Log("1일차 금자구역 개방");
                //Debug.Log("2일차 금지구역 안내");
                break;
            case 3:
                _blockPlaceApplier.ApplyDay(0);
                _blockPlaceApplier.ApplyDay(1);
                //Debug.Log("1, 2차 금지구역 막음");
                break;
            case 4:
                _blockPlaceApplier.RestoreDay(1);
                _blockPlaceApplier.ShowBlockedPlaces(2);//3���� �� �ȳ�
                //Debug.Log("2일차 금지구역 개방");
                //Debug.Log("3일차 금지구역 안내내");
                break;
            case 5:
                _blockPlaceApplier.ApplyDay(0);
                _blockPlaceApplier.ApplyDay(1);
                _blockPlaceApplier.ApplyDay(2);
                //Debug.Log("1, 2, 3일차 금지구역 막음음");
                break;
            case 6:
                _blockPlaceApplier.RestoreDay(2);
                _blockPlaceApplier.ShowBlockedPlaces(3);//4���� �� �ȳ�
                //Debug.Log("3일차 금지구역 개방방");
                //Debug.Log("4일차 금지구역 안내내");
                break;
            case 7:
                _blockPlaceApplier.ApplyDay(0);
                _blockPlaceApplier.ApplyDay(1);
                _blockPlaceApplier.ApplyDay(2);
                _blockPlaceApplier.ApplyDay(3);
               // Debug.Log("1, 2, 3, 4일차 금지구역 막음음");
                break;
            default:
                //Debug.Log("마지막 1곳만 남았습니다.");
                break;
        }
    }
}
