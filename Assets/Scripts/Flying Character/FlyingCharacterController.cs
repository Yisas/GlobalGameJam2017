using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlyingCharacterController : MonoBehaviour
{

    public float groundSpeed;
    public float verticalTakeoffForce;
    public float airborneVerticalSpeed;
    public float airborneHorizontalSpeed;
    public float turnSpeed;

    public float horizontalAngularTiltSpeed;
    public float horizontalMaxTilt;
    public float horizontalTiltSelfCorrectionSpeed; // Value between 0 and 1, determines how fast the character tilts back to rotation 0 when there is no airborne horizontal input

    public float verticalAngularTiltSpeed;
    public float verticalMaxTilt;
    public float verticalTiltSelfCorrectionSpeed; // Value between 0 and 1, determines how fast the character tilts back to rotation 0 when there is no airborne vertical input

    [Tooltip("List of layers that act as ground for this character")]
    public LayerMask whatIsGround;

    public GameObject bomb;
    public float bombThrowForce;

    // Public references
    public Camera mainCamera;

    // Private references
    private Rigidbody rb;
    private Transform groundCheck;
    private Transform bombDropPoint;        // Position where the bombs are dropped/thrown from

    // Input varibles
    private float horizontalInput;
    private float verticalInput;
    private float horizontalRightStickInput;
    private float verticalRightStickInput;
    private bool takeOff = false;
    private bool dropBomb = false;

    // State variables
    bool grounded = true;
    Vector3 relativeToCameraForward;        // Direction vectors relative to camera
    Vector3 relativeToCameraRight;
    Vector3 relativeToCameraUp;

    // Other variables
    float distToGround;                 // Distance from the center of the bird to the bounds of its collider
    float tiltSefcorrectionT = 0;       // t for the lerp back to 0 rotation, when there is no horizontal input

    // Use this for initialization
    void Start()
    {
        // Setup Variables
        distToGround = GetComponent<CapsuleCollider>().bounds.extents.y;

        // Setup references
        rb = GetComponent<Rigidbody>();
        bombDropPoint = transform.FindChild("Bomb Drop");
    }

    // Update is called once per frame
    void Update()
    {
        InputCollection();
        grounded = CheckIfGrounded();

        if (!grounded && dropBomb)
        {
            DropBomb();

            // Reset flag
            dropBomb = false;
        }
    }

    void InputCollection()
    {
        horizontalInput = Input.GetAxis("Horizontal Bird");
        verticalInput = Input.GetAxis("Vertical Bird");
        horizontalRightStickInput = Input.GetAxis("Horizontal Right Stick");
        verticalRightStickInput = Input.GetAxis("Vertical Right Stick");
        takeOff = Input.GetButtonDown("Take Off");
        dropBomb = Input.GetButtonDown("Drop Bomb");
    }

    void FixedUpdate()
    {
        // Setup directions relative to camera
        relativeToCameraForward = mainCamera.transform.TransformDirection(Vector3.forward);
        relativeToCameraForward.y = 0;
        relativeToCameraForward = relativeToCameraForward.normalized;
        relativeToCameraRight = new Vector3(relativeToCameraForward.z, 0, -relativeToCameraForward.x);
        relativeToCameraUp = mainCamera.transform.TransformDirection(Vector3.up);
        relativeToCameraUp = relativeToCameraUp.normalized;

        Move();

        if (grounded && takeOff)
            TakeOff();
    }

    void Move()
    {
        // Grounded movement
        if (grounded)
        {
            Vector3 moveDirection = (horizontalInput * relativeToCameraRight + verticalInput * relativeToCameraForward).normalized;
            rb.AddForce(moveDirection * groundSpeed);
        }

        // Airborne movement
        else
        {
            HorizontalTilt();
            VecticalTilt();

            // Apply vertical movement in the direction of the nose
            Vector3 moveDirection = (verticalInput * transform.forward).normalized;
            rb.AddForce(moveDirection * airborneVerticalSpeed);

            // Apply horizontal movement
            moveDirection = (horizontalInput * transform.right).normalized;
            rb.AddForce(moveDirection * airborneHorizontalSpeed);
        }

        // Turning
        transform.Rotate(Vector3.up, turnSpeed * horizontalRightStickInput);
    }

    void TakeOff()
    {
        rb.AddForce(transform.up * verticalTakeoffForce);
    }

    bool CheckIfGrounded()
    {
        return Physics.Raycast(transform.position, -Vector3.up, distToGround + 0.1f);
    }

    void HorizontalTilt()
    {
        // Transform euler angle to negative of positive
        float currentRotation = (transform.eulerAngles.z > 180) ? transform.eulerAngles.z - 360 : transform.eulerAngles.z;

        if (horizontalInput != 0)
        {
            tiltSefcorrectionT = 0;     // Reset lerping back to 0

            // Calculate rotation
            float rotation = -1 * horizontalAngularTiltSpeed * horizontalInput;

            rotation = Mathf.Clamp(rotation + currentRotation, -horizontalMaxTilt, horizontalMaxTilt);

            transform.rotation = Quaternion.Euler(new Vector3(transform.eulerAngles.x, transform.eulerAngles.y, rotation));
        }
        else
        {
            transform.rotation = Quaternion.Euler(new Vector3(transform.eulerAngles.x, transform.eulerAngles.y, Mathf.Lerp(currentRotation, 0, tiltSefcorrectionT)));
            tiltSefcorrectionT += horizontalTiltSelfCorrectionSpeed;

            // Has to be a percentile for lerp to work
            Mathf.Clamp(tiltSefcorrectionT, 0, 1);
        }
    }

    void VecticalTilt()
    {
        // Transform euler angle to negative of positive
        float currentRotation = (transform.eulerAngles.x > 180) ? transform.eulerAngles.x - 360 : transform.eulerAngles.x;

        if (verticalInput != 0)
        {
            tiltSefcorrectionT = 0;     // Reset lerping back to 0

            // Calculate rotation
            float rotation = verticalAngularTiltSpeed * verticalInput;

            rotation = Mathf.Clamp(rotation + currentRotation, -verticalMaxTilt, verticalMaxTilt);

            transform.rotation = Quaternion.Euler(new Vector3(rotation, transform.eulerAngles.y, transform.eulerAngles.z));
        }
        else
        {
            transform.rotation = Quaternion.Euler(new Vector3(Mathf.Lerp(currentRotation, 0, tiltSefcorrectionT), transform.eulerAngles.y, transform.eulerAngles.z));
            tiltSefcorrectionT += verticalTiltSelfCorrectionSpeed;

            // Has to be a percentile for lerp to work
            Mathf.Clamp(tiltSefcorrectionT, 0, 1);
        }
    }

    void DropBomb()
    {
        GameObject tempBomb = Instantiate(bomb, bombDropPoint.position, bombDropPoint.rotation);
        tempBomb.GetComponent<Rigidbody>().AddForce(new Vector3(0, -1, 0) * bombThrowForce);
    }

    void OnCollisionEnter(Collision col)
    {

        if (1 << col.collider.gameObject.layer == whatIsGround && grounded)
        {
            // Reset rotation (from tilt) if grounded
            transform.rotation = Quaternion.Euler(transform.rotation.x, transform.rotation.y, 0);
        }
    }
}
