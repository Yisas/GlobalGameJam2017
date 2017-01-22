using UnityEngine;

public interface ParentCollider
{
    // A script that inherits from  this interface can recieve collision calls from an object with a ChildCollider script.
    void OnTriggerEnterFromChild(Collider col);
    void OnCollisionEnterFromChild(Collision col);
}