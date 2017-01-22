using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyChaser :  Slowable {

    public Transform target;
    public float speed;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {

        // deleteme
        if (Input.GetButtonDown("Test Button"))
            fictitiousTimeScale *= 0.5f;

        float step = speed * Time.deltaTime * fictitiousTimeScale;
        transform.position = Vector3.MoveTowards(transform.position, target.position, step);
    }
}
