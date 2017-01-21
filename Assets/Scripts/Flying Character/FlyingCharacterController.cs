using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlyingCharacterController : MonoBehaviour
{

    public float groundSpeed;
    public float verticalTakeoffForce;
    [Tooltip("List of layers that act as ground for this character")]
    public LayerMask[] whatIsGround;
    public float angularTiltSpeed;
    public float maxTilt;

    // Public references
    public Camera mainCamera;

    // Private references
    private Rigidbody rb;
    private Transform groundCheck;

    // Input varibles
    private float horizontalInput;
    private float verticalInput;
    private bool takeOff = false;       // Starts as false

    // State variables
    bool grounded = true;
    Vector3 forward;                    // Direction vectors relative to camera
    Vector3 right;
    Vector3 up;

    // Other variables
    float distToGround;                 // Distance from the center of the bird to the bounds of its collider

    // Use this for initialization
    void Start()
    {
        // Setup Variables
        distToGround = GetComponent<CapsuleCollider>().bounds.extents.y;

        // Setup references
        rb = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        InputCollection();
        grounded = CheckIfGrounded();
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
        forward = mainCamera.transform.TransformDirection(Vector3.forward);
        forward.y = 0;
        forward = forward.normalized;
        right = new Vector3(forward.z, 0, -forward.x);
        up = mainCamera.transform.TransformDirection(Vector3.up);
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

            // Reset rotation (from tilt) if grounded
            transform.rotation = new Quaternion(0, 0, 0, 1);
        }

        // Airborne movement
        else
        {
            Tilt();

            // Only apply vertical input (forward or backwards)
            Vector3 moveDirection = (verticalInput * forward).normalized;
            rb.AddForce(moveDirection * groundSpeed);
        }
    }

    void TakeOff()
    {
        rb.AddForce(up * verticalTakeoffForce);
    }

    bool CheckIfGrounded()
    {
        return Physics.Raycast(transform.position, -Vector3.up, distToGround + 0.1f);
    }

    void Tilt()
    {
        // Calculate rotation
        float rotation = -1 * angularTiltSpeed * horizontalInput;

        // Transform euler angle to negative of positive
        float currentRotation = (transform.eulerAngles.z > 180) ? transform.eulerAngles.z - 360 : transform.eulerAngles.z;

        rotation = Mathf.Clamp(rotation + currentRotation, -maxTilt, maxTilt);

        transform.rotation = Quaternion.Euler(new Vector3(transform.eulerAngles.x, transform.eulerAngles.y, rotation));
    }
}
