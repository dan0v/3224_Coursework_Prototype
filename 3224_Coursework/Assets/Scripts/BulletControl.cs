using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletControl : MonoBehaviour
{
    public float MaxProjectileSpeed = 12f;
    public bool Reflected = false;

    private Rigidbody body;

    void Awake()
    {
        body = GetComponent<Rigidbody>();
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.CompareTag("BaseEnemy") && Reflected)
        {
            collision.gameObject.GetComponent<BaseEnemy>().Kill();
            Destroy(this.gameObject);
        }
        if (collision.collider.CompareTag("Shield"))
        {
            body.velocity *= 100;
            body.velocity = Vector3.ClampMagnitude(body.velocity, MaxProjectileSpeed);
            this.gameObject.GetComponent<MeshRenderer>().material.SetColor("_EmissionColor", Color.red);
            Reflected = true;
            this.gameObject.layer = LayerMask.NameToLayer("ReflectedBullet");
            this.gameObject.tag = "ReflectedBullet";
            collision.gameObject.GetComponent<PlayerController>().ShieldHit();
        }
        if (!collision.collider.CompareTag("Shield") && !collision.collider.CompareTag("Bullet") && !collision.collider.CompareTag("BaseEnemy") && !(collision.collider.CompareTag("Player") && Reflected))
        {
            GameObject.Destroy(this.gameObject);
        }
    }
}
