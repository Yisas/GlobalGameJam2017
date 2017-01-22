﻿using SWS;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OnTriggerGoTo : MonoBehaviour {
    public PathManager path;
    public int currentWaypoint;

    private Vector3 target;
    public float damping = 1;
    private Collider lastCollider;
    public Transform boundariesRotation;
    private bool updatingTransforms;

    void Start()
    {
        target = path.GetPathPoints()[currentWaypoint];
    }
    // Update is called once per frame
    void Update()
    {
        var lookPos = target - transform.parent.position;
        lookPos.y = 0;
        var rotation = Quaternion.LookRotation(lookPos);
        transform.parent.rotation = Quaternion.Slerp(transform.parent.rotation, rotation, Time.deltaTime * damping);

        Vector3 targetPostition = new Vector3(target.x,
                                       boundariesRotation.position.y,
                                       target.z);
        boundariesRotation.transform.LookAt(targetPostition);
    }

    void OnTriggerEnter(Collider other)
    {
        if (other != lastCollider)
        {
            currentWaypoint++;
            if (path.GetPathPoints().Length > currentWaypoint)
            {
                target = path.GetPathPoints()[currentWaypoint];
            }
            lastCollider = other;

        }
    }
}