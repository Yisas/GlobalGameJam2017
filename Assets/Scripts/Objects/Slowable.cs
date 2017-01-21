using UnityEngine;

public class Slowable : MonoBehaviour
{
    [HideInInspector]
    public float fictitiousTimeScale = 1;

    // State variables
    private bool slowed;

    // Called from external script to slow down this object
    public void Slow(float slowdownMultiplier)
    {
        fictitiousTimeScale *= slowdownMultiplier;
    }

}
