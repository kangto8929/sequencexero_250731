using UnityEngine;

public class DamageCalculator : MonoBehaviour
{
   public static int CalculateFinalDamage(int attackerPower, int defenderDefense)
    {
        float damageScale = 0.45f;
        int rawDamage = attackerPower - defenderDefense;
        int finalDamage = Mathf.FloorToInt(rawDamage * damageScale);
        return Mathf.Max(finalDamage, 1);
    }
}
