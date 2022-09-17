using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FishSpawner : MonoBehaviour
{
    [Header("Bounds")]
    [SerializeField] Vector2 bounds;
    [SerializeField] Vector2 spawnRateMinMax = new Vector2(.25f, 5.0f);
    [SerializeField] LayerMask hitMask;
    [SerializeField] GameObject[] npc_Prefabs;
    [SerializeField] bool autoSpawn = false;
    [SerializeField] int populationCount = 50;
    [SerializeField] int maxPopulation = 100;

    int currentPopulationCount = 0;


    // Start is called before the first frame update
    void Start()
    {
        if (autoSpawn)
        {
            int[] fishTypes = new int[maxPopulation];
            for (int i = 0; i < maxPopulation; i++)
                fishTypes[i] = Random.Range(0, npc_Prefabs.Length);
            StartCoroutine(SpawnRoutine(fishTypes));
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    [ContextMenu("Spawn Fish")]
    void SpawnFish()
    {
        int[] fishTypes = new int[populationCount];
        for (int i = 0; i < populationCount; i++)
            fishTypes[i] = Random.Range(0, npc_Prefabs.Length);

        StartCoroutine(SpawnRoutine(fishTypes));
    }

    IEnumerator SpawnRoutine(int[] fishTypes)
    {
        for (int i = 0; i < fishTypes.Length; i++)
        {
            currentPopulationCount++;
            if (currentPopulationCount >= maxPopulation)
                yield break;

            Transform t = Instantiate(npc_Prefabs[fishTypes[i]], transform).transform;
            t.position = GetPositionInBounds();
            yield return new WaitForSeconds(Random.Range(spawnRateMinMax.x, spawnRateMinMax.y));
        }
    }

    Vector2 GetPositionInBounds()
    {
        Vector2 returnPosition = transform.position;
        
        int tries = 50;
        while(returnPosition == (Vector2)transform.position && tries > 0)
        {
            tries--;
            Vector2 randomPointInsideBounds = GetRandomPointInsideBounds();
            Collider2D hitCollider = Physics2D.OverlapCircle(randomPointInsideBounds, .5f, hitMask);
            if (hitCollider == null)
            {
                returnPosition = randomPointInsideBounds;
                return returnPosition;
            }
        }

        print("Unable to find an open position");
        return returnPosition;
    }

    Vector2 GetRandomPointInsideBounds()
    {
        return (Vector2)transform.position + new Vector2((Random.value - 0.5f) * bounds.x, (Random.value - 0.5f) * bounds.y);
    }

    bool InBounds(Vector2 returnPosition)
    {
        return Mathf.Abs(returnPosition.x) < bounds.x / 2 && Mathf.Abs(returnPosition.y) < bounds.y / 2;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireCube(transform.position, bounds);
    }
}
