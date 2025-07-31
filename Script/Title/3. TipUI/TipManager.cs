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
        StartCoroutine(ChangeTipText());//���� �ٲ�
    }

    IEnumerator ChangeTipText()
    {

        int newTipIndex;

        //������ ���� ���� �ٽ� �� ��������
        do
        {
            newTipIndex = Random.Range(0, Tips.Count);//����Ʈ �ȿ��� �������� ����
        }
        while (newTipIndex == _lastTipIndex && Tips.Count > 1);//���� 2�� �̻� �ְ� ��� ���� �ε����� ���� �ε����� ������ �ٽ� �̱�

        _lastTipIndex = newTipIndex;
        TipTexts.text = Tips[newTipIndex];

        while (true)
        {
            yield return new WaitForSeconds(TipChangeInterval);

            //������ ���� ���� �ٽ� �� ��������
            do
            {
                newTipIndex = Random.Range(0, Tips.Count);//����Ʈ �ȿ��� �������� ����
            }
            while (newTipIndex == _lastTipIndex && Tips.Count > 1);//���� 2�� �̻� �ְ� ��� ���� �ε����� ���� �ε����� ������ �ٽ� �̱�

            _lastTipIndex = newTipIndex;
            TipTexts.text = Tips[newTipIndex];
        }
    }


}
