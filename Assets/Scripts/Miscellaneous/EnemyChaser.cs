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
	public override void Update () {
        base.Update();

        float step = speed * Time.deltaTime * fictitiousTimeScale;
        transform.position = Vector3.MoveTowards(transform.position, target.position, step);
    }
}
