using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldGameObjectStorage : MonoBehaviour
{
    public PlayerManager player;

    public static WorldGameObjectStorage Instance;
    private void Awake()
    {
        if (Instance == null)
            Instance = this;
    }
}
