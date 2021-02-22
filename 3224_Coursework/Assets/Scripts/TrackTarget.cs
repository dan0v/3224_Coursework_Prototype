using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrackTarget : MonoBehaviour
{
    public GameObject Target;
    public Vector3 Offset;

    // Update is called once per frame
    void Update()
    {
        transform.position = Target.transform.position + Offset;
    }
}
