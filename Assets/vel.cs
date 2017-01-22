using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class vel : MonoBehaviour
{
    void Start()
    {
        transform.position = Camera.main.transform.position;
        //print(transform.position);
        //Debug.Break();
        GetComponent<Rigidbody>().velocity = transform.forward * 10;
    }
}
