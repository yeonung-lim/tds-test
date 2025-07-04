using System.Collections.Generic;
using UnityEngine;

public class MonsterPool : MonoBehaviour
{
    public GameObject[] monsterPrefabs;
    public int initialSize = 10;

    private List<GameObject> pooledMonsters = new List<GameObject>();
    private Transform poolParent;

    private void Awake()
    {
        poolParent = new GameObject("MonsterPool").transform;

        for (int i = 0; i < initialSize; i++)
        {
            GameObject prefab = monsterPrefabs[i % monsterPrefabs.Length];
            GameObject monster = Instantiate(prefab);
            monster.SetActive(false);
            monster.transform.SetParent(poolParent);
            pooledMonsters.Add(monster);
        }
    }

    public GameObject GetMonster()
    {
        foreach (var m in pooledMonsters)
        {
            if (!m.activeInHierarchy)
                return m;
        }

        // 부족하면 새로 생성
        GameObject prefab = monsterPrefabs[Random.Range(0, monsterPrefabs.Length)];
        GameObject newMonster = Instantiate(prefab);
        newMonster.SetActive(false);
        newMonster.transform.SetParent(poolParent);
        pooledMonsters.Add(newMonster);
        return newMonster;
    }
}