using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlyingCharacterController : MonoBehaviour {

    public float groundSpeed;
    public float verticalTakeoffForce;

    // Public references
    public Camera camera;

    // Private references
    private Rigidbody rb;

    // Input varibles
    private float horizontalInput;
    private float verticalInput;
    private bool takeOff = false;       // Starts as false

    // State variables
    bool grounded = true;
    Vector3 forward;                    // Direction vectors relative to camera
    Vector3 right;
    Vector3 up;              

	// Use this for initialization
	void Start () {

        // Setup references
		rb= GetComponent<Rigidbody>();
	}
	
	// Update is called once per frame
	void Update () {
        InputCollection();
	}

    void InputCollection()
    {
        horizontalInput = Input.GetAxis("Horizontal Bird");
        verticalInput = Input.GetAxis("Vertical Bird");
        takeOff = Input.GetButtonDown("Take Off");
    }

    void FixedUpdate()
    {
        // Setup directions relative to camera
        forward = camera.transform.TransformDirection(Vector3.forward);
        forward.y = 0;
        forward = forward.normalized;
        right = new Vector3(forward.z, 0, -forward.x);
        up = camera.transform.TransformDirection(Vector3.up);
        up = up.normalized;

        Move();

        if (grounded && takeOff)
            TakeOff();
    }

    void Move()
    {
        // Grounded movement
        if (grounded)
        {
            Vector3 moveDirection = (horizontalInput * right + verticalInput * forward).normalized;
            rb.AddForce(moveDirection * groundSpeed);
        }
    }

    void TakeOff()
    {
        rb.AddForce(up * verticalTakeoffForce);
    }
}
