using UnityEngine;

public class FireTask : MonoBehaviour
{
    public ParticleSystem fireFX;
    public Light fireLight;

    [HideInInspector] public bool active;

    void Awake()
    {
        Deactivate();
    }

    public void Activate()
    {
        active = true;
        gameObject.SetActive(true);

        if (fireFX && !fireFX.isPlaying) fireFX.Play();
        if (fireLight) fireLight.enabled = true;
    }

    public void Deactivate()
    {
        active = false;

        if (fireFX) fireFX.Stop(true, ParticleSystemStopBehavior.StopEmitting);
        if (fireLight) fireLight.enabled = false;
    }

    public void Extinguish()
    {
        if (!active) return;

        Deactivate();
        if (DayNightDirector.Instance != null)
            DayNightDirector.Instance.NotifyFireExtinguished();
    }
}
