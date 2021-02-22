using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseEnemy : MonoBehaviour
{
    public float TrackingSpeed = 5;
    public GameObject Projectile;
    public float ProjectileSpeed = 10;
    public float ProjectileInterval = 0.5f;
    public bool Dead = false;

    private GameObject player;
    private float shootTime = 0;
    private Director director;
    private AudioSource fireAudioSource;
    private AudioSource deathAudioSource;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        director = Camera.main.GetComponent<Director>();
        fireAudioSource = GetComponents<AudioSource>()[0];
        deathAudioSource = GetComponents<AudioSource>()[1];
    }

    // Update is called once per frame
    void Update()
    {
        if (!Dead)
        {
            shootTime += Time.deltaTime;
            // Choose new target direction (without changing on y axis)
            Vector3 targetDirection = player.transform.position - transform.position;
            targetDirection.y = 0;

            // The step size is equal to speed times frame time.
            float singleStep = TrackingSpeed * Time.deltaTime * (1 / Vector3.Distance(transform.position, new Vector3(player.transform.position.x, transform.position.y, player.transform.position.z)));

            // Rotate the forward vector towards the target direction by one step
            Vector3 newDirection = Vector3.RotateTowards(transform.forward, targetDirection, singleStep, 0.0f);

            // Calculate a rotation a step closer to the target and applies rotation to this object
            transform.rotation = Quaternion.LookRotation(newDirection);

            if (shootTime > ProjectileInterval)
            {
                FireProjectile();
                shootTime = 0;
            }
        }
    }

    public void Kill(bool byDirector = false)
    {
        Dead = true;
        
        foreach (Renderer r in GetComponentsInChildren<Renderer>())
        {
            r.enabled = false;
        }
        foreach (Collider c in GetComponentsInChildren<Collider>())
        {
            c.enabled = false;
        }

        StartCoroutine(DieGracefully(0.5f, 0, byDirector));
    }

    private IEnumerator DieGracefully(float duration, float targetVolume, bool byDirector = false)
    {
        if (!byDirector)
        {
            float currentTime = 0;
            float start = deathAudioSource.volume;
            deathAudioSource.Play();

            while (currentTime < duration)
            {
                currentTime += Time.unscaledDeltaTime;
                deathAudioSource.volume = Mathf.Lerp(start, targetVolume, currentTime / duration);
                yield return null;
            }
        }
        GameObject.Destroy(this.gameObject);
        yield break;
    }

private void FireProjectile()
    {
        director.ProjectileFired();
        GameObject bullet = Instantiate(Projectile);
        bullet.transform.position = transform.position;
        bullet.GetComponent<Rigidbody>().AddForce(transform.forward * ProjectileSpeed);
        fireAudioSource.Play();
    }
}
