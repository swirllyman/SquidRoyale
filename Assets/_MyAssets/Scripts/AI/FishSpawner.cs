using Fusion;
using Fusion.Sockets;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FishSpawner : NetworkBehaviour
{
    [Header("Bounds")]
    [SerializeField] Vector2 bounds;
    [SerializeField] Vector2 spawnRateMinMax = new Vector2(.25f, 5.0f);
    [SerializeField] LayerMask hitMask;
    [SerializeField] NetworkPrefabRef[] npc_Prefabs;
    [SerializeField] bool autoSpawn = false;
    [SerializeField] int maxPopulation = 100;

    internal int currentPopulationCount = 0;

    public override void Spawned()
    {
        base.Spawned();

        if (Runner.IsServer)
        {
            if (autoSpawn)
            {
                //print("Auto Spawning");
                StartCoroutine(SpawnRoutine());
            }
        }
    }

    IEnumerator SpawnRoutine()
    {
        while (true)
        {
            if(currentPopulationCount < maxPopulation)
            {
                currentPopulationCount++;
                NetworkObject networkFish = Runner.Spawn(npc_Prefabs[Random.Range(0, npc_Prefabs.Length)], GetPositionInBounds(), Quaternion.identity);
                networkFish.transform.parent = transform;
                yield return new WaitForSeconds(Random.Range(spawnRateMinMax.x, spawnRateMinMax.y));
            }
            else
            {
                yield return new WaitForSeconds(5.0f);
            }
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

        //print("Unable to find an open position");
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
