using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RenderPath : MonoBehaviour
{
    [SerializeField]
    private float initialVelocity = 10f;
    [SerializeField]
    private float timeResolution = 0.02f;
    [SerializeField]
    private float maxTime = 5.0f;

    private float epsilon = 0.0005f;

    private LineRenderer lineRenderer;


    // Use this for initialization
    void Start()
    {
        lineRenderer = GetComponent<LineRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 velocityVector = transform.forward * initialVelocity;
        lineRenderer.numPositions = ((int)(maxTime / timeResolution));
        //lineRenderer.numPositions = 3;

        int index = 0;
        Vector3 currentPosition = transform.position;
        //for (float t = 0.0f; t <= 4; t += 2)
        for (float t = 0.0f; t < maxTime - epsilon; t += timeResolution)
        {
            lineRenderer.SetPosition(index, currentPosition);
            currentPosition += velocityVector * timeResolution;
            velocityVector += Physics.gravity * timeResolution;
            ++index;
            //print(t);
            //if (index == 250)
            //    Debug.Break();
        }
    }
}
