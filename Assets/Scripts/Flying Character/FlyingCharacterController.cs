using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlyingCharacterController : MonoBehaviour
{

    public float groundSpeed;
    public float groundedMaxSpeed;
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
    public float bombThrowInitialVelocity;
    public float aimingParabolaSpeed;
    public GameObject projectileVisualizer;

    // Public references
    public Camera mainCamera;

    // Private references
    private Rigidbody rb;
    private Transform groundCheck;
    private Transform bombDropPoint;        // Position where the bombs are dropped/thrown from
    private Transform overheadCameraTarget;

    [Header("------ Animator Variables ------")]
    // TODO: PRIVATE AND MAKE ME ASSIGN THIS BY MYSELF
    public Animator anim;
    public float runningMultiplier;
    public float flyingMultiplier;

    // Input varibles
    private float horizontalInput;
    private float verticalInput;
    private float horizontalRightStickInput;
    private float verticalRightStickInput;
    private bool takeOff = false;
    private bool dropBomb = false;
    private bool grabbingTreeButtonPressed = false;
    private bool aimingButtinPressed = false;

    // State variables
    bool grounded = true;
    bool lockMovement = false;
    bool lockRotation = false;
    bool withinTreeGrabZone = false;        // Flag gets reset at the end of FixedUpdate
    bool grabTreeAttempt = false;           // The player is trying to grab tree and within the legal conditions to do so
    bool grabbingTree = false;            // Character is actually grabbing the tree
    bool overheadCameraActivated = false;
    Vector3 relativeToCameraForward;        // Direction vectors relative to camera
    Vector3 relativeToCameraRight;
    Vector3 relativeToCameraUp;

    // Other variables
    float distToGround;                 // Distance from the center of the bird to the bounds of its collider
    float tiltSefcorrectionT = 0;       // t for the lerp back to 0 rotation, when there is no horizontal input
    Vector3 priorCameraPosition;        // To reset camera from overhead to third person
    Quaternion priorCameraRotation;

    // Use this for initialization
    void Start()
    {
        // Setup Variables
        distToGround = GetComponent<CapsuleCollider>().bounds.extents.y;
        priorCameraPosition = mainCamera.transform.localPosition;
        priorCameraRotation = mainCamera.transform.localRotation;

        // Setup references
        rb = GetComponent<Rigidbody>();
        overheadCameraTarget = transform.FindChild("Overhead Camera Target");
        bombDropPoint = transform.FindChild("Bomb Drop");

        // Setup animator
        anim.SetFloat("runningMultiplier", runningMultiplier);
        anim.SetFloat("flyingMultiplier", flyingMultiplier);
    }

    // Update is called once per frame
    void Update()
    {
        InputCollection();

        grounded = CheckIfGrounded();
        anim.SetBool("grounded", grounded);

        //grabTreeAttempt = CheckIfGrabTree(); moved to fixed update

        if (!grounded && dropBomb && !grabbingTree)
        {
            DropBomb();

            // Reset flag
            dropBomb = false;
        }

        // Camera work for when on the tree
        if (aimingButtinPressed && grabbingTree)
        {
            mainCamera.transform.position = overheadCameraTarget.position;
            mainCamera.transform.rotation = overheadCameraTarget.rotation;

            overheadCameraActivated = true;
        }
        else if (overheadCameraActivated)
        {
            mainCamera.transform.localPosition = priorCameraPosition;
            mainCamera.transform.localRotation = priorCameraRotation;
            
            overheadCameraActivated = false;
        }

        if(grabbingTree && dropBomb)
        {
            GameObject tempBomb = Instantiate(bomb, bombDropPoint.position, bombDropPoint.rotation);
            tempBomb.GetComponent<Rigidbody>().velocity = transform.forward * bombThrowInitialVelocity;
        }
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

        grabTreeAttempt = CheckIfGrabTree();
        GrabTree();
        AimParabola();

        Move();

        if (grounded && takeOff && !lockMovement)
            TakeOff();

        // Reset this flag every frame, and then let OnTriggerStay do the rest
        withinTreeGrabZone = false;
    }

    void InputCollection()
    {
        horizontalInput = Input.GetAxis("Horizontal Bird");
        verticalInput = Input.GetAxis("Vertical Bird");
        horizontalRightStickInput = Input.GetAxis("Horizontal Right Stick");
        verticalRightStickInput = Input.GetAxis("Vertical Right Stick");
        takeOff = Input.GetButtonDown("Take Off");
        dropBomb = Input.GetButtonDown("Drop Bomb");
        grabbingTreeButtonPressed = Input.GetButton("Grab Onto Tree");
        aimingButtinPressed = Input.GetButton("Aim");
    }

    void Move()
    {
        if (!lockMovement)
        {
            // Grounded movement
            if (grounded)
            {
                if (rb.velocity.magnitude <= groundedMaxSpeed)
                {
                    Vector3 moveDirection = transform.forward;
                    rb.AddForce(moveDirection * groundSpeed * verticalInput);
                    anim.SetFloat("moveInput", Mathf.Max(Mathf.Abs(horizontalInput), Mathf.Abs(verticalInput)));
                }
            }

            // Airborne movement
            else
            {
                HorizontalTilt();
                VecticalTilt();

                // Direction to apply for in
                Vector3 moveDirection = new Vector3();

                // Apply vertical movement in the direction of the nose
                if (verticalInput != 0)
                {
                    if (Mathf.Sign(verticalInput) == 1)
                    {
                        // Nose tilts down from vertical tilt and force is applied in the same direction of the nose
                        moveDirection = (transform.forward).normalized;
                    }
                    else
                    {
                        // Nose tilts up from vertical tilt and force is only aplied on the horizontal plane (0 is zeroed)
                        moveDirection = (transform.forward).normalized;
                        moveDirection = new Vector3(moveDirection.x, 0, moveDirection.z);
                    }

                    rb.AddForce(moveDirection * airborneVerticalSpeed * verticalInput);
                }

                // Apply horizontal movement
                moveDirection = (horizontalInput * transform.right).normalized;
                rb.AddForce(moveDirection * airborneHorizontalSpeed);
            }
        }

        if (!lockRotation)
            Turn();
    }

    // Turning with the right stick
    void Turn()
    {
        float previousVelocity = rb.velocity.magnitude;

        // Turning
        transform.Rotate(Vector3.up, turnSpeed * horizontalRightStickInput);

        if (grounded)
        {
            // If grounded apply prior velocity in the new direction
            rb.velocity = transform.forward * previousVelocity;
        }
    }

    void TakeOff()
    {
        rb.AddForce(transform.up * verticalTakeoffForce);
    }

    bool CheckIfGrounded()
    {
        //Debug.DrawRay(transform.position, -transform.up, Color.blue, 1);
        return Physics.Raycast(transform.position, -transform.up, distToGround + 0.5f);
    }

    // If you are whithin a Tree Grab Zone layer, airborne and pressing the grab tree button, returns true
    bool CheckIfGrabTree()
    {
        if (withinTreeGrabZone && grabbingTreeButtonPressed && !grounded)
            return true;
        else
            return false;
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

    void GrabTree()
    {
        if (grabbingTree && Input.GetButtonDown("Grab Onto Tree"))
        // Release Tree
        {
            grabbingTree = false;
            lockMovement = false;
            rb.useGravity = true;
            projectileVisualizer.SetActive(false);
            return;
        }


        if (!grabbingTree && grabTreeAttempt)
        {
            GameObject target = FindClosestTreeTarget();
            transform.position = target.transform.position;
            grabbingTree = true;

            rb.velocity = new Vector3(0, 0, 0);
            lockMovement = true;
            rb.useGravity = false;

            projectileVisualizer.SetActive(true);
            projectileVisualizer.GetComponent<RenderPath>().initialVelocity = bombThrowInitialVelocity;
        }
    }

    GameObject FindClosestTreeTarget()
    {
        GameObject[] gos;
        gos = GameObject.FindGameObjectsWithTag("Tree Perch Target");
        GameObject closest = null;
        float distance = Mathf.Infinity;
        Vector3 position = transform.position;
        foreach (GameObject go in gos)
        {
            Vector3 diff = go.transform.position - position;
            float curDistance = diff.sqrMagnitude;
            if (curDistance < distance)
            {
                closest = go;
                distance = curDistance;
            }
        }
        return closest;
    }

    void AimParabola()
    {
        if (grabbingTree)
        {
            bombThrowInitialVelocity += verticalInput * aimingParabolaSpeed;
            projectileVisualizer.GetComponent<RenderPath>().initialVelocity = bombThrowInitialVelocity;
        }
    }

    void OnCollisionEnter(Collision col)
    {

        grounded = CheckIfGrounded();

        if (1 << col.collider.gameObject.layer == whatIsGround && grounded)
        {
            //Debug.Log("Grounding");

            // Reset rotation (from tilt) if grounded
            transform.rotation = Quaternion.Euler(transform.rotation.eulerAngles.x, transform.rotation.eulerAngles.y, 0);
        }
    }

    void OnTriggerStay(Collider col)
    {
        if (col.gameObject.layer == LayerMask.NameToLayer("Tree Grab Zone"))
            withinTreeGrabZone = true;
    }
}
