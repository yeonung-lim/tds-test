using UnityEngine;

public class MonsterSpawner : MonoBehaviour
{
    public MonsterPool monsterPool;

    public Transform[] laneSpawnPoints;
    public float spawnInterval = 1.5f;

    public LayerMask[] groundMasks;
    public string[] monsterLayerNames;
    public string[] sortingLayerNames;

    private float timer;

    private void Update()
    {
        timer += Time.deltaTime;
        if (timer >= spawnInterval)
        {
            timer = 0f;
            SpawnMonsters();
        }
    }

    private void SpawnMonsters()
    {
        int laneIndex = Random.Range(0, laneSpawnPoints.Length);
        int monstersToSpawn = Random.Range(1, 3);
        float spacing = 1.5f;

        for (int i = 0; i < monstersToSpawn; i++)
        {
            GameObject monsterObj = monsterPool.GetMonster();
            Vector3 spawnPos = laneSpawnPoints[laneIndex].position;
            spawnPos.x += i * spacing;

            monsterObj.transform.position = spawnPos;
            monsterObj.transform.rotation = Quaternion.identity;
            monsterObj.SetActive(true);

            int monsterLayer = LayerMask.NameToLayer(monsterLayerNames[laneIndex]);
            monsterObj.layer = monsterLayer;

            Monster monster = monsterObj.GetComponent<Monster>();
            monster.SetGroundAndMonsterMask(groundMasks[laneIndex], (1 << monsterLayer));
            monster.ResetState(); // 반드시 초기화 함수 필요

            foreach (var sr in monsterObj.GetComponentsInChildren<SpriteRenderer>())
            {
                sr.sortingLayerName = sortingLayerNames[laneIndex];
            }
        }
    }
}