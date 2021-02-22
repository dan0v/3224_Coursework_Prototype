using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IChildCollisionDetecting
{
    public void OnChildCollisionEnter(string childName, Collision collision);
    public void OnChildTriggerEnter(string childName, Collider other);
}
