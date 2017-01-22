using UnityEngine;

public class Slowable : MonoBehaviour
{
    [HideInInspector]
    public float fictitiousTimeScale = 1;

    // State variables
    protected bool slowed;
    protected bool triggerSlow = false;         // Used on child scripts to separate state from trigger.
    protected bool triggerReturn = false;

    // Timers
    protected float slowdownTimer;

    // Called from external script to slow down this object
    public void Slow(float slowdownMultiplier, float slowdownPeriod)
    {
        fictitiousTimeScale = slowdownMultiplier;
        slowdownTimer = slowdownPeriod;
        triggerSlow = true;
        slowed = true;
    }

    protected void UndoSlowdown()
    {
        slowdownTimer = 0;
        triggerReturn = true;
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
        {
            slowdownTimer -= Time.deltaTime;

            if (slowdownTimer <= 0)
            {
                UndoSlowdown();
            }
        }
    }

    public virtual void LateUpdate()
    {
        // WARNING TO SELF: not sure about this giving problems
        triggerSlow = false;
        triggerReturn = false;
    }
}
