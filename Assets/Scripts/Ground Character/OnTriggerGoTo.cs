using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SWS;

public class OnTriggerGoTo : MonoBehaviour
{
    public PathManager centerPath;
    public PathManager leftPath;
    public PathManager rightPath;

    private PathManager path;

    public int currentWaypoint;

    private Vector3 target;
    public float damping = 14;
    private Collider lastCollider;

    public GameObject particlesPrefab;
    private GameObject lastWaypointParticles;

    private float moveDelay = 0.2f;
    public float currentDelay = 0;

    void Start()
    {
        path = centerPath;
        target = path.GetPathPoints()[currentWaypoint];
        addParticlesOnWaypoint();
    }
    // Update is called once per frame
    void Update()
    {
        currentDelay -= Time.deltaTime;
        if(currentDelay < 0)
        { 
            if (Input.GetAxis("Horizontal Ground") > 0)
            {
                currentDelay = moveDelay;
                if (path == centerPath)
                {
                    transform.parent.GetComponent<Rigidbody>().AddForce(transform.right * 9000);
                    path = rightPath;
                }
                else if (path == leftPath)
                {
                    transform.parent.GetComponent<Rigidbody>().AddForce(transform.right * 9000);
                    path = centerPath;
                }

                target = path.GetPathPoints()[currentWaypoint];
                Destroy(lastWaypointParticles);
                addParticlesOnWaypoint();

                currentDelay += moveDelay;
            }
            else if (Input.GetAxis("Horizontal Ground") < 0)
            {
                currentDelay = moveDelay;
                if (path == centerPath)
                {
                    transform.parent.GetComponent<Rigidbody>().AddForce(-transform.right * 9000);
                    path = leftPath;
                }
                if (path == rightPath)
                {
                    transform.parent.GetComponent<Rigidbody>().AddForce(-transform.right * 9000);
                    path = centerPath;
                }

                target = path.GetPathPoints()[currentWaypoint];
                Destroy(lastWaypointParticles);
                addParticlesOnWaypoint();

                currentDelay += moveDelay;
            }
        }

        var lookPos = target - transform.parent.position;
        lookPos.y = 0;
        var rotation = Quaternion.LookRotation(lookPos);
        transform.parent.rotation = Quaternion.Slerp(transform.parent.rotation, rotation, Time.deltaTime * damping);
    }

    void OnTriggerEnter(Collider other)
    {
        if (other != lastCollider)
        {
            currentWaypoint++;
            if (path.GetPathPoints().Length > currentWaypoint)
            {
                target = path.GetPathPoints()[currentWaypoint];
                ScoreManager.Instance.addScore(100);
                //increase forward speed by 5%
                transform.parent.GetComponent<PlayerController>().movement.forwardSpeed *= 1.05f;
            }
            else //end round
            {
                //increase forward speed by 20%
                transform.parent.GetComponent<PlayerController>().movement.forwardSpeed *= 1.20f;
                ScoreManager.Instance.addScore(1000);
                currentWaypoint = 0;
                target = path.GetPathPoints()[currentWaypoint];
            }
            lastCollider = other;
            Destroy(lastWaypointParticles);
            addParticlesOnWaypoint();
        }
    }

    void addParticlesOnWaypoint()
    {
        lastWaypointParticles = Instantiate(particlesPrefab, target + Vector3.up, Quaternion.identity) as GameObject;
    }
}
