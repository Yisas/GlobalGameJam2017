using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class vel : MonoBehaviour
{
    void Start()
    {
        GetComponent<Rigidbody>().velocity = transform.forward * 10;
    }
}
