using UnityEngine;

public class BaitingMonsterSingleton : Singleton<BaitingMonsterSingleton>
{
    [SerializeField] private MonsterSpawner spawner;

    public MonsterSpawner Spawner => spawner;
}