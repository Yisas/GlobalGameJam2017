using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChildCollider : MonoBehaviour
{
    // This script is here to communicate a collision to a parent object that inherits from ParentCollider.

    public GameObject parent;

    void OnTriggerEnter(Collider col)
    {
        parent.GetComponent<ParentCollider>().OnTriggerEnterFromChild(col);
    }

    void OnCollisionEnter(Collision col)
    {
        parent.GetComponent<ParentCollider>().OnCollisionEnterFromChild(col);
    }
}