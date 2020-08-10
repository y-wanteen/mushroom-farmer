using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MushroomSpawn : MonoBehaviour
{
    public float nextSpawnTime;

    [HideInInspector] public new Transform transform;

    public bool canSpawnShrooms = true;

    Mushroom newShroom;

    Coroutine spawnCountdownRoutine;

    private void Start()
    {
        transform = gameObject.transform;
    }

    public void EndGame()
    {
        StopAllCoroutines();
    }

    public void WaitForSpawn(float spawnTime = 0)
    {
        canSpawnShrooms = true;
#if UNITY_EDITOR
        Debug.Log($"{gameObject.name} started spawn");
#endif
        if (spawnCountdownRoutine == null)
            spawnCountdownRoutine = StartCoroutine(SpawnCountdown(spawnTime));
    }

    private IEnumerator SpawnCountdown(float spawnTime = 0)
    {
        if (spawnTime != 0)
            nextSpawnTime = spawnTime;
        else
            nextSpawnTime = MushroomSpawnManager.Instance.RandomSpawnFrequency();

        float timer = 0;

        while (timer < nextSpawnTime)
        {
            timer += Time.deltaTime;

            if (timer >= nextSpawnTime)
            {
                Spawn();
                spawnCountdownRoutine = null;
                yield break;
            }
            yield return null;
        }
    }

    private void Spawn()
    {
        newShroom = MushroomSpawnManager.Instance.RandomShroomSelector();

        if (newShroom != null)
        {
            newShroom.originalSpawner = this;
            newShroom.gameObject.SetActive(true);

            canSpawnShrooms = false;
        }
#if UNITY_EDITOR
        Debug.Log("New Shroom!");
#endif
    }

    public void BlockSpawning()
    {
#if UNITY_EDITOR
      //  Debug.Log($"{gameObject.name} spawner is blocked");
#endif
        if (spawnCountdownRoutine != null)
        {
            StopCoroutine(spawnCountdownRoutine);
            spawnCountdownRoutine = null;
        }
        canSpawnShrooms = false;
        MushroomSpawnManager.Instance.RemoveSpawner(this);
        gameObject.SetActive(false);
    }

}
