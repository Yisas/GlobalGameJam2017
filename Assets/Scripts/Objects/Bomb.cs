﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bomb : MonoBehaviour
{
    public float aoeRadius;
    public float slowdownMultiplier;
    public float slowdownPeriod;
    public float aoeTimer;
    public GameObject explosionEffect;

    // Private references
    SphereCollider aoe;

    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    void OnTriggerEnter(Collider col)
    {

        //Debug.Log("Bomb collided with " + col);

        Collider[] collisions = Physics.OverlapSphere(transform.position, aoeRadius);

        foreach (Collider hit in collisions)
        {
            //Debug.Log("Bomb aoe hit " + hit);

            if(hit.tag == "Slowable")
            {
                //Debug.Log("Bomb hit slowable object " + hit);

                hit.gameObject.GetComponent<Slowable>().Slow(slowdownMultiplier, slowdownPeriod);
            }
        }

        Explode();
    }

    void Explode()
    {
        GameObject aoeEffect = Instantiate(explosionEffect, transform.position, explosionEffect.transform.rotation);
        aoe.GetComponent<AOESlow>().slowdownMultiplier = slowdownMultiplier;
        aoe.GetComponent<AOESlow>().slowdownPeriod = slowdownPeriod;
        aoe.GetComponent<AOESlow>().TimerStart(aoeTimer);

        Destroy(this.gameObject);
    }
}
