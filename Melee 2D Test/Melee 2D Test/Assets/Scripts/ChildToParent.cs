using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChildToParent : MonoBehaviour
{
    public PlayerManager player;
    public GameObject blueLight;

    public void StopPlayerVelocity()
    {
        player.rb.velocity = Vector2.zero;
    }

    public void ShakeCamera(float intensity)
    {
        CameraShake.instance.ShakeCamera(intensity, 0.1f);
    }

    public void ToggleLight(float i)
    {
        if (i > 0) 
            blueLight.SetActive(true);
        
        else 
            blueLight.SetActive(false);
    }

    
}
