using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChildToParent : MonoBehaviour
{
    public PlayerManager player;

    public void StopPlayerVelocity()
    {
        player.rb.velocity = Vector2.zero;
    }
}
