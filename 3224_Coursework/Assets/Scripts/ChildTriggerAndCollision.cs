using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;


public class ChildTriggerAndCollision : MonoBehaviour
{
    public string ChildName;
    private IChildCollisionDetecting parentScript;

    private void Awake()
    {
        var scripts = transform.parent.gameObject.GetComponents<MonoBehaviour>().OfType<IChildCollisionDetecting>();
        parentScript = scripts.First();
    }
    private void OnCollisionEnter(Collision collision)
    {
        parentScript.OnChildCollisionEnter(ChildName, collision);
    }
    private void OnTriggerEnter(Collider other)
    {
        parentScript.OnChildTriggerEnter(ChildName, other);
    }
}
