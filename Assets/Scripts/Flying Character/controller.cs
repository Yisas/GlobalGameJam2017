using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class controller : MonoBehaviour
{

    public int velocity = 3;
    private int turnSpeed = 80;
    private int tiltAmount = 60;

    // Update is called once per frame
    void Update()
    {
        transform.Translate(Vector3.forward * velocity * Time.deltaTime);
        float horizontalMovement;
        horizontalMovement = Input.GetAxis("Horizontal Bird");
        float turn = turnSpeed * Time.deltaTime * horizontalMovement;
        transform.Rotate(new Vector3(0, turn, 0), Space.World);

        float verticalAxis = Input.GetAxis("Vertical Bird");
        UpAndDown(verticalAxis);
        Tilt(horizontalMovement);
    }

    void UpAndDown(float input)
    {
        transform.Translate(Vector3.up * input * velocity * Time.deltaTime);
        //float turn = tiltAmount * -input;
        //transform.rotation = Quaternion.Euler(new Vector3(turn, transform.eulerAngles.y, transform.eulerAngles.z));
    }

    void Tilt(float input)
    {
        float turn = tiltAmount * -input;
        transform.rotation = Quaternion.Euler(new Vector3(transform.eulerAngles.x, transform.eulerAngles.y, turn));
    }
}
