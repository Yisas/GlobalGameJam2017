using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowPlayer : MonoBehaviour
{
    public GameObject player;
    public float verticalOffset;
    public float horizontalOffset;


    private float horizontalRightStickInput;
    private float verticalRightStickInput;

    private Vector3 cameraOffset;

    // Use this for initialization
    void Start()
    {
        cameraOffset = new Vector3(player.transform.position.x, player.transform.position.y + verticalOffset, player.transform.position.z + horizontalOffset);
    }

    void Update()
    {
        //InputCollection();
    }

    // Update is called once per frame
    void LateUpdate()
    {
        /*
        cameraOffset = Quaternion.AngleAxis(horizontalRightStickInput * player.GetComponent<FlyingCharacterController>().turnSpeed, Vector3.up) * cameraOffset;

        transform.position = player.transform.position + cameraOffset;
        transform.LookAt(player.transform.position);
        */

    }

    void InputCollection()
    {
        horizontalRightStickInput = Input.GetAxis("Horizontal Right Stick");
        verticalRightStickInput = Input.GetAxis("Vertical Right Stick");
    }
}
