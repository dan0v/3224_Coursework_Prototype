using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pickup_Coin : MonoBehaviour
{
    public Vector3 RotationSpeed = new Vector3(0, 50, 0);
    public float BounceStrength = 1f;

    private AudioSource audioSource;
    private bool active = true;
    private float constantTimer = 0;
    private GameObject coin;
    private Vector3 startingPosition;

    private void Start()
    {
        coin = transform.Find("Model").gameObject;
        startingPosition = transform.position;
        audioSource = GetComponent<AudioSource>();
    }
    // Update is called once per frame
    void Update()
    {
        constantTimer += Time.deltaTime;
        RotateAndBob();
    }

    private void RotateAndBob()
    {
        coin.transform.Rotate(RotationSpeed * Time.deltaTime);
        transform.position = new Vector3(startingPosition.x, startingPosition.y + (BounceStrength * Mathf.Sin(constantTimer)), startingPosition.z);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && active)
        {
            Camera.main.GetComponent<Director>().CoinPickup();
            DestroyGracefully();
        }
    }

    public void DestroyGracefully(bool byDirector = false)
    {
        active = false;

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
            float start = audioSource.volume;
            audioSource.Play();

            while (currentTime < duration)
            {
                currentTime += Time.unscaledDeltaTime;
                audioSource.volume = Mathf.Lerp(start, targetVolume, currentTime / duration);
                yield return null;
            }
        }
        GameObject.Destroy(this.gameObject);
        yield break;
    }
}
