using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnManager : MonoBehaviour
{
    public static SpawnManager Instance;

    Dictionary<Role, List<SpawnPoint> > spawnDict = new Dictionary<Role, List<SpawnPoint> >();

    void Awake()
    {
        Instance = this;
        SpawnPoint[] spawnPoints = GetComponentsInChildren<SpawnPoint>();

        // build spawn point dictionary
        for (int i = 0; i < spawnPoints.Length; i++)
        {
            SpawnPoint s = spawnPoints[i];

            if (!spawnDict.ContainsKey(s.role))
            {
                spawnDict[s.role] = new List<SpawnPoint>();
            }

            spawnDict[s.role].Add(s);
        }
    }

    public Transform GetSpawnPoint(Role role, bool first = false)
    {
        if (!first)
        {
            // find a random spawn point
            return spawnDict[role][Random.Range(0, spawnDict[role].Count)].transform;
        }
        else
        {
            // find the "first" spawn point
            for (int i = 0; i < spawnDict[role].Count; i++)
            {
                if (spawnDict[role][i].first)
                {
                    return spawnDict[role][i].transform;
                }
            }

            // can't find the "first" spawn? return first in list then
            return spawnDict[role][0].transform;
        }
    }
}
