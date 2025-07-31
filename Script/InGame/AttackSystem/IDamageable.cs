using UnityEngine;

public interface IDamageable
{
    public interface IDamageable
{
    bool IsDead { get; }                    // 죽었는지 여부 확인
    int CurrentDefense { get; }             // 방어력
    string GameObjectName { get; }          // 로그 출력용 이름

    void TakeDamage(int damage, bool isCritical = false, bool isMiss = false);
}

    //void TakeDamage(int damage, bool isCritical = false, bool isMiss = false);
}


