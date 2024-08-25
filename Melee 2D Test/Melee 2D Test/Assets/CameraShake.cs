using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class CameraShake : MonoBehaviour
{
    public CinemachineVirtualCamera mCam;
    [SerializeField] float shakeTimer;
    [SerializeField] float initialTime;
    [SerializeField] CinemachineBasicMultiChannelPerlin cinemachineBasicMultiChannelPerlin;
    public static CameraShake instance;
    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(gameObject);

        mCam = GetComponent<CinemachineVirtualCamera>();
        cinemachineBasicMultiChannelPerlin = mCam.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
    }

    private void Update()
    {

        if (shakeTimer > 0)
        {
            shakeTimer -= Time.deltaTime;

        }
        else
        {
            cinemachineBasicMultiChannelPerlin.m_AmplitudeGain = 0;
        }
    }
    public void ShakeCamera(float intensity, float time)
    {

        cinemachineBasicMultiChannelPerlin.m_AmplitudeGain = intensity;
        cinemachineBasicMultiChannelPerlin.m_FrequencyGain = intensity;
        initialTime = time;
        shakeTimer = time;
    }
}
