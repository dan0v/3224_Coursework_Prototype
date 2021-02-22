using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour, IChildCollisionDetecting
{
    [Header("Debug")]
    public static bool GodMode = false;
    public bool Dead = false;

    [Header("Control")]
    public float Speed = 25.0F;
    public float SpeedShieldMultiplier = 0.75f;
    public static float CursorRelativeScale = 20;
    public List<float> CursorRelativeScaleOptions = new List<float>();
    public float MaxCursorDistance = 5;
    public Vector2 StartCursorPosition;
    public static int SelectedCursorSetting = 2;

    [Header("UI")]
    public Color FullShieldBarColor;
    public Color DepletedShieldBarColor;
    public GameObject PlayerUI;

    [Header("Damage")]
    public float ShieldBulletDamage = 0.1f;
    public float ShieldPercentage = 1;
    public float ShieldLossRate = 0.3f;
    public float ShieldChargeRate = 0.2f;
    public bool ShieldExhausted = false;
    public int ShieldHits = 0;
    private Vector3 startingShieldScale;
    private Vector3 input = Vector3.zero;
    private Rigidbody body;
    private GameObject cursor;
    private Vector2 cursorPosition;
    private GameObject shield;
    private GameObject shieldBar;
    private Slider shieldBarSlider;
    private AudioSource deathAudioSource;


    private void Awake()
    {
        body = this.GetComponent<Rigidbody>();
    }

    private void Start()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        cursorPosition = StartCursorPosition;
        cursor = transform.Find("Cursor").gameObject;
        shield = transform.Find("Shield").gameObject;
        startingShieldScale = shield.transform.localScale;
        shieldBar = PlayerUI.transform.Find("ShieldBar").gameObject;
        shieldBarSlider = shieldBar.GetComponent<Slider>();
        deathAudioSource = GetComponent<AudioSource>();
    }

    private void Update()
    {
        if (!Dead && Time.timeScale != 0)
        {
            float adjustedSpeed = Speed;

            float time = Time.deltaTime;

            HandleSettings();

            adjustedSpeed = HandleShield(time, adjustedSpeed);

            HandleMouseAim(time);

            HandleMovement(time, adjustedSpeed);
        }
    }

    public void ShieldHit()
    {
        ShieldHits++;
        shield.GetComponent<AudioSource>().Play();
    }

    private void UpdateShieldUI()
    {
        shieldBarSlider.value = ShieldPercentage;
        if (ShieldPercentage < 0.98)
        {
            shieldBar.SetActive(true);
            if (ShieldExhausted)
            {
                shieldBarSlider.fillRect.gameObject.GetComponent<Image>().color = DepletedShieldBarColor;
            }
            else
            {
                shieldBarSlider.fillRect.gameObject.GetComponent<Image>().color = FullShieldBarColor;
            }
        }
        else
        {
            shieldBar.SetActive(false);
        }
    }

    private void HandleSettings()
    {
        if (Input.GetButtonDown("GodMode"))
        {
            GodMode = !GodMode;
        }
        if (Input.GetKeyDown("1"))
        {
            CursorRelativeScale = CursorRelativeScaleOptions[0];
            SelectedCursorSetting = 1;
        }
        if (Input.GetKeyDown("2"))
        {
            CursorRelativeScale = CursorRelativeScaleOptions[1];
            SelectedCursorSetting = 2;
        }
        if (Input.GetKeyDown("3"))
        {
            CursorRelativeScale = CursorRelativeScaleOptions[2];
            SelectedCursorSetting = 3;
        }
    }

    private float HandleShield(float time, float adjustedSpeed)
    {

        if (ShieldExhausted || !Input.GetButton("Fire1"))
        {
            shield.GetComponent<BoxCollider>().enabled = false;
            shield.GetComponent<MeshRenderer>().enabled = false;
            ShieldPercentage += time * ShieldChargeRate;
            UpdateShieldUI();
        }
        else
        {
            adjustedSpeed = Speed * SpeedShieldMultiplier;
            shieldBar.SetActive(false);
            if (ShieldHits > 0)
            {
                ShieldPercentage -= ShieldHits * ShieldBulletDamage;
                ShieldHits = 0;
            }
            shield.GetComponent<BoxCollider>().enabled = true;
            shield.GetComponent<MeshRenderer>().enabled = true;
            ShieldPercentage -= time * ShieldLossRate;
            shield.transform.localScale = new Vector3(startingShieldScale.x * ShieldPercentage, startingShieldScale.y, startingShieldScale.z);
        }

        if (ShieldPercentage > 1)
        {
            ShieldPercentage = Mathf.Clamp(ShieldPercentage, 0, 1);
            ShieldExhausted = false;
        }
        else if (ShieldPercentage < 0)
        {
            ShieldPercentage = Mathf.Clamp(ShieldPercentage, 0, 1);
            ShieldExhausted = true;
        }

        return adjustedSpeed;
    }

    private void HandleMouseAim(float time)
    {
        // Mouse aim
        cursorPosition += new Vector2(Input.GetAxisRaw("Mouse X") * CursorRelativeScale * time, Input.GetAxisRaw("Mouse Y") * CursorRelativeScale * time);
        cursorPosition = Vector2.ClampMagnitude(cursorPosition, MaxCursorDistance);
        cursor.transform.position = new Vector3(cursorPosition.x + transform.position.x, transform.position.y, cursorPosition.y + transform.position.z);

        transform.LookAt(cursor.transform);
    }

    private void HandleMovement(float time, float adjustedSpeed)
    {
        float speedAtTimeStep = time * adjustedSpeed * 100;
        Vector3 impulse = new Vector3(Input.GetAxisRaw("Horizontal") * speedAtTimeStep, 0, Input.GetAxisRaw("Vertical") * speedAtTimeStep);

        // Cap speed
        if (impulse.magnitude != 0)
        {
            impulse *= speedAtTimeStep / impulse.magnitude;
            body.AddForce(impulse);
        }
    }

    public void OnChildCollisionEnter(string childName, Collision collision)
    {
    }

    public void Kill()
    {
        Dead = true;
        deathAudioSource.Play();
    }

    public void OnChildTriggerEnter(string childName, Collider other)
    {
        if (childName == "PlayerBody")
        {
            if (other.CompareTag("Bullet"))
            {
                if (GodMode)
                {
                    Debug.LogWarning("Player should have died, but has GodMode on!");
                }
                else
                {
                    Kill();
                }
            }
        }
    }
}
