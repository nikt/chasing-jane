using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnPoint : MonoBehaviour
{
    [SerializeField] public bool first;
    [SerializeField] public Role role;
    [SerializeField] GameObject graphics;

    void Awake()
    {
        graphics.SetActive(false);
    }
}
