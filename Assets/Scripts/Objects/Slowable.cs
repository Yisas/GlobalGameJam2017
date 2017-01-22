using UnityEngine;

public class Slowable : MonoBehaviour
{
    [HideInInspector]
    public float fictitiousTimeScale = 1;

    // State variables
    protected bool slowed;

    // Called from external script to slow down this object
    public void Slow(float slowdownMultiplier)
    {
        fictitiousTimeScale *= slowdownMultiplier;
        slowed = true;
    }

    public virtual void Update()
    {
        if (Input.GetButton("Test Button"))
        {
            Debug.Log("tewst");
            Slow(0.1f);
        }
    }
}
