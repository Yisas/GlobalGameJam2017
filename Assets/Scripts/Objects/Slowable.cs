using UnityEngine;

public class Slowable : MonoBehaviour
{
    [HideInInspector]
    public float fictitiousTimeScale = 1;

    // State variables
    protected bool slowed;

    // Timers
    protected float slowdownTimer;

    // Called from external script to slow down this object
    public void Slow(float slowdownMultiplier, float slowdownPeriod)
    {
        fictitiousTimeScale = slowdownMultiplier;
        slowdownTimer = slowdownPeriod;
        slowed = true;
    }

    protected void UndoSlowdown()
    {
        fictitiousTimeScale = 1;
        slowdownTimer = 0;
        slowed = false;
    }

    public virtual void Update()
    {
        if (Input.GetButton("Test Button"))
        {
            Debug.Log("Slowdonw test");
            Slow(0.1f, 3.0f);
        }

        if (slowed)
            slowdownTimer -= Time.deltaTime;

        if (slowdownTimer <= 0)
            UndoSlowdown();
    }
}
