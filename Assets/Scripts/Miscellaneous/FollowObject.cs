using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowObject : MonoBehaviour {
    public bool followX = true;
    public GameObject target;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        if (followX)
        {
            transform.position = new Vector3(target.transform.position.x, transform.position.y, transform.position.z);
        }	
	}
}
