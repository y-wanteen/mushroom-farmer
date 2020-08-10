using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MushroomSpawnManager : MonoBehaviour
{
    [Header("Spawn Lists")]
    public List<MushroomSpawn> spawners = new List<MushroomSpawn>();
    [SerializeField] List<Mushroom> mushroomPool = new List<Mushroom>();

    public enum SpawnSpeed
    {
        Slow = 0,
        Normal = 1,
        Fast = 2
    }

    [Header("Spawn Speed")]
    public float firstSpawnTime = 3f;
    public SpawnSpeed currentSpawnSpeed;

    [Range(0, 100)]
    public float spawnSpeed_SlowMin = 5f;
    [Range(0, 100)]
    public float spawnSpeed_SlowMax = 30f;

    [Range(0, 100)]
    public float spawnSpeed_NormalMin = 5f;
    [Range(0, 100)]
    public float spawnSpeed_NormalMax = 25f;

    [Range(0, 100)]
    public float spawnSpeed_FastMin = 5f;
    [Range(0, 100)]
    public float spawnSpeed_FastMax = 10f;

    float minSpawnSpeed, maxSpawnSpeed;

    static System.Random random = new System.Random();

    #region Singleton
    private static MushroomSpawnManager _instance;
    private static bool _isInstanceNull = true;

    public static MushroomSpawnManager Instance
    {
        get
        {
            if (_isInstanceNull)
            {
                return null;
            }
            else
            {
                return _instance;
            }
        }

        set
        {
            _instance = value;
            _isInstanceNull = false;
        }
    }
    #endregion

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Instance = null;
            Destroy(this);
        }
    }

    private void Start()
    {
        UpdateSpawnSpeed(currentSpawnSpeed);

        ShuffleMushroomPool();
        ShuffleMushroomSpawners();
    }


    public void StartGame()
    {
        StartCoroutine(InitializeSpawners());
    }

    private void ShuffleMushroomPool()
    {
        int poolSize = mushroomPool.Count;

        for (int i = 0; i < poolSize; i++)
        {
            int randomIndex = random.Next(poolSize - 1);

            Mushroom temp = mushroomPool[randomIndex];
            mushroomPool[randomIndex] = mushroomPool[i];
            mushroomPool[i] = temp;
        }
    }

    private void ShuffleMushroomSpawners()
    {
        int poolSize = spawners.Count;

        for (int i = 0; i < poolSize; i++)
        {
            int randomIndex = random.Next(poolSize - 1);

            MushroomSpawn temp = spawners[randomIndex];
            spawners[randomIndex] = spawners[i];
            spawners[i] = temp;
        }
    }

    public IEnumerator InitializeSpawners()
    {
        for (int i = 0; i < spawners.Count; i++)
        {
            if (i == 0)
                spawners[i].WaitForSpawn(firstSpawnTime);
            else
                spawners[i].WaitForSpawn();

            // yield return new WaitForSeconds(Random.Range(0, 4)); //stagger spawn starting
        }

        yield return new WaitUntil(() => !GameManager.Instance.gameStarted);

        for (int i = 0; i < spawners.Count; i++)
        {
            spawners[i].EndGame();
        }
    }

    public void RemoveSpawner(MushroomSpawn spawner)
    {
        spawners.Remove(spawner);

        if (spawners.Count < 1)
            GameManager.Instance.GameOver();
    }

    public Mushroom RandomShroomSelector()
    {
        if (mushroomPool.Count > 0)
        {
            int index = Random.Range(0, mushroomPool.Count);

            Mushroom shroom = mushroomPool[index];

            mushroomPool.Remove(shroom);

            return shroom;
        }
        else return null;
    }

    public float RandomSpawnFrequency()
    {
        return (int)Random.Range(minSpawnSpeed, maxSpawnSpeed);
    }

    public void UpdateSpawnSpeed(SpawnSpeed newSpeed)
    {
        switch (newSpeed)
        {
            case SpawnSpeed.Slow:
                minSpawnSpeed = spawnSpeed_SlowMin;
                maxSpawnSpeed = spawnSpeed_SlowMax;
                break;
            case SpawnSpeed.Normal:
                minSpawnSpeed = spawnSpeed_NormalMin;
                maxSpawnSpeed = spawnSpeed_NormalMax;
                break;
            case SpawnSpeed.Fast:
                minSpawnSpeed = spawnSpeed_FastMin;
                maxSpawnSpeed = spawnSpeed_FastMax;
                break;
        }
    }

}
