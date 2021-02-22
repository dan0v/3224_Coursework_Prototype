using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Pickup_SloMo : MonoBehaviour
{
    public float SloMoSeconds = 3f;
    public float SloMoRatio = 0.8f;
    public Vector3 RotationSpeed = new Vector3(0,50,0);
    public float BounceStrength = 1f;
    public bool ScaleInsteadOfBar = true;

    private bool active = true;
    private bool pickedUp = false;
    private float pickupTimer = 0;
    private float constantTimer = 0;
    private Vector3 startingPosition;
    private GameObject hourglass;
    private GameObject timerBar;
    private Slider timerSlider;
    private float timeScaleSubtracted = 0;
    private Vector3 startingScale;
    private AudioSource activateAudioSource;
    private AudioSource deActivateAudioSource;

    private void Start()
    {
        startingPosition = transform.position;
        timerBar = transform.Find("SloMoUI").Find("TimerBar").gameObject;
        timerSlider = timerBar.GetComponent<Slider>();
        hourglass = transform.Find("Hourglass").gameObject;
        startingScale = hourglass.transform.localScale;
        activateAudioSource = GetComponents<AudioSource>()[0];
        deActivateAudioSource = GetComponents<AudioSource>()[1];
    }

    // Update is called once per frame
    void Update()
    {
        if (active)
        {
            constantTimer += Time.deltaTime;
            RotateAndBob();

            if (pickedUp)
            {
                pickupTimer += Time.unscaledDeltaTime;
                if (ScaleInsteadOfBar)
                {
                    hourglass.transform.localScale = startingScale * (SloMoSeconds - pickupTimer) / SloMoSeconds;
                }
                else
                {
                    timerBar.SetActive(true);
                    timerSlider.value = (SloMoSeconds - pickupTimer) / SloMoSeconds;
                }

                if (pickupTimer > SloMoSeconds)
                {
                    DestroyGracefully();
                }
            }
        }
    }

    private void RotateAndBob()
    {
        hourglass.transform.Rotate(RotationSpeed * Time.deltaTime);
        transform.position = new Vector3(startingPosition.x, startingPosition.y + (BounceStrength * Mathf.Sin(constantTimer)), startingPosition.z);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !pickedUp)
        {
            pickedUp = true;
            hourglass.transform.Find("Model").gameObject.GetComponent<Renderer>().materials[0].color = Color.red;
            timeScaleSubtracted = Time.timeScale * (1f - SloMoRatio);
            Time.timeScale -= timeScaleSubtracted;
            activateAudioSource.Play();
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

        StartCoroutine(DieGracefully(1.3f, 0, byDirector));
    }

    private IEnumerator DieGracefully(float duration, float targetVolume, bool byDirector = false)
    {
        if (!byDirector)
        {
            float currentTime = 0;
            float start = deActivateAudioSource.volume;
            deActivateAudioSource.Play();

            while (currentTime < duration)
            {
                currentTime += Time.unscaledDeltaTime;
                Time.timeScale += timeScaleSubtracted * (Time.unscaledDeltaTime / duration);
                //deActivateAudioSource.volume = Mathf.Lerp(start, targetVolume, currentTime / duration);
                yield return null;
            }
        }
        else
        {
            Time.timeScale += timeScaleSubtracted;
        }
        GameObject.Destroy(this.gameObject);
        yield break;
    }
}
