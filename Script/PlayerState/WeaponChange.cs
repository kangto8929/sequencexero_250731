using NUnit.Framework;
using UnityEngine;

public class WeaponChange : MonoBehaviour
{
    public int WeaponCount = 0;
    public GameObject[] WeaponList;

    public void ChangeCount()
    {
        if(WeaponCount < WeaponList.Length- 1)
        {
            WeaponCount++;

            for(int i = 0; i< WeaponList.Length; i++)
            {
                WeaponList[i].SetActive(false);
            }

            WeaponList[WeaponCount].SetActive(true);
        }

        else
        {
            WeaponCount = 0;

            for (int i = 0; i < WeaponList.Length; i++)
            {
                WeaponList[i].SetActive(false);
            }

            WeaponList[WeaponCount].SetActive(true);
        }
    }
}
