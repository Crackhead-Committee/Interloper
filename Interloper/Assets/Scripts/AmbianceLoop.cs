using UnityEngine;

public class AmbienceLoop : MonoBehaviour
{
    public AudioSource ambientSource;

    void Start()
    {
        if (ambientSource)
        {
            ambientSource.loop = true;
            ambientSource.Play();
        }
    }
}
