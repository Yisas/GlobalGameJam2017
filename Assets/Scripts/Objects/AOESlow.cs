using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AOESlow : MonoBehaviour {

    // These variables will be assigned by the bomb so don't bother modifying them
    [HideInInspector]
    public float slowdownMultiplier;
    [HideInInspector]
    public float slowdownPeriod;

    private SphereCollider aoeBoundingCollider;

    private float aoeTimer;
    private bool timerStarted = false;

	// Use this for initialization
	void Start () {
        aoeBoundingCollider = GetComponent<SphereCollider>();
	}
	
	// Update is called once per frame
	void Update () {

        if (timerStarted)
        {
            aoeTimer -= Time.deltaTime;

            if (aoeTimer <= 0)
                Destroy(gameObject);
        }
	}

    public void TimerStart(float aoeTimer)
    {
        this.aoeTimer = aoeTimer;
        timerStarted = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.tag == "Slowable")
        {
            Slowable slowableScript = other.GetComponent<Slowable>();

            if(!slowableScript.slowed)
            {
                slowableScript.Slow(slowdownMultiplier,slowdownPeriod);
            }
        }
    }
}
