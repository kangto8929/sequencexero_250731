// PlayerState.cs
using System.Collections.Generic;

public class PlayerState
{
    public int CurrentHealth;
    public int MaxHealth;
    public int CurrentStamina;
    public int MaxStamina;

    public int Defense;

    public Dictionary<WeaponType, int> WeaponSkills = new();
    public WeaponType CurrentWeaponType;
}
