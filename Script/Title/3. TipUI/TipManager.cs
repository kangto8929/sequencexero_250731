using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class TipManager : MonoBehaviour
{
    [Header("Tip")]
    public List<string> Tips = new List<string>();
    public TextMeshProUGUI TipTexts;
    public float TipChangeInterval = 3f;

    private int _lastTipIndex = -1;


    private void Start()
    {
        StartCoroutine(ChangeTipText());//팁을 바꿈
    }

    IEnumerator ChangeTipText()
    {

        int newTipIndex;

        //직전에 나온 팁은 다시 안 나오도록
        do
        {
            newTipIndex = Random.Range(0, Tips.Count);//리스트 안에서 무작위로 선택
        }
        while (newTipIndex == _lastTipIndex && Tips.Count > 1);//팁이 2개 이상 있고 방금 뽑은 인덱스가 이전 인덱스와 같으면 다시 뽑기

        _lastTipIndex = newTipIndex;
        TipTexts.text = Tips[newTipIndex];

        while (true)
        {
            yield return new WaitForSeconds(TipChangeInterval);

            //직전에 나온 팁은 다시 안 나오도록
            do
            {
                newTipIndex = Random.Range(0, Tips.Count);//리스트 안에서 무작위로 선택
            }
            while (newTipIndex == _lastTipIndex && Tips.Count > 1);//팁이 2개 이상 있고 방금 뽑은 인덱스가 이전 인덱스와 같으면 다시 뽑기

            _lastTipIndex = newTipIndex;
            TipTexts.text = Tips[newTipIndex];
        }
    }


}
