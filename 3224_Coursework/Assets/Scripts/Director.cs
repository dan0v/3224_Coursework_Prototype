using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Profiling;
using System.Linq;
using UnityEngine.Audio;

public class Director : MonoBehaviour
{
    public static bool StatsEnabled = true;
    public AudioMixer MasterMixer;

    [Header("Pickup Spawn")]
    public GameObject SlowMotionPickupPrefab;
    public GameObject CoinPickupPrefab;
    public GameObject PickupSpawnArea;
    public float SlowMotionPickupsPerLevel = 1.5f;
    public float CoinPickupsPerLevel = 1.5f;

    [Header("Enemy Spawn")]
    public GameObject EnemyPrefab;
    public GameObject EnemySpawnArea;
    public float SquareMetersPerEnemy = 1.5f;
    public float BulletDifficultyMultiplier = 0f;
    public int EnemySpawnCountMultiplier = 1;

    [Header("Terrain")]
    public GameObject Floor;
    public List<Color> FloorColorLevels = new List<Color>();

    private static int score = 0;
    private static int highScore = 0;
    private static int level = 1;
    private static bool audioMuted = false;

    private float levelTimer = 0;
    private TMP_Text liveScore;
    private TMP_Text tutorialText;
    private TMP_Text liveLevel;
    private GameObject liveHighscore;
    private PlayerController playerController;
    private GameObject deathMenu;
    private GameObject winMenu;

    // Start is called before the first frame update
    void Start()
    {
        liveScore = transform.Find("Canvas").Find("LiveScore").GetComponent<TMP_Text>();
        liveLevel = transform.Find("Canvas").Find("LiveLevel").GetComponent<TMP_Text>();
        tutorialText = transform.Find("Canvas").Find("Tutorial").Find("StarterMessage").GetComponent<TMP_Text>();
        liveHighscore = transform.Find("Canvas").Find("LiveHighscore").gameObject;
        deathMenu = transform.Find("Canvas").Find("DeathMenu").gameObject;
        winMenu = transform.Find("Canvas").Find("WinMenu").gameObject;
        playerController = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>();

        deathMenu.SetActive(false);
        winMenu.SetActive(false);
        liveHighscore.SetActive(false);

        if (level == 1)
        {
            tutorialText.enabled = true;
        }

        SpawnEnemies(level);
        SpawnPickups(level);

        int floorLevel = level % FloorColorLevels.Count;
        
        for (int i = 0; i < Floor.transform.childCount; i++)
        {
            GameObject floorTile = Floor.transform.GetChild(i).gameObject;
            floorTile.GetComponent<MeshRenderer>().material.color = FloorColorLevels[floorLevel];
        }
    }

    // Update is called once per frame
    void Update()
    {
        levelTimer += Time.deltaTime;
        if (Input.GetKey("escape"))
        {
            Application.Quit();
        }
        if (Input.GetButtonDown("Stats"))
        {
            StatsEnabled = !StatsEnabled;
        }
        if (Input.GetButtonDown("Clear") && !playerController.Dead)
        {
            GameObject[] enemies = GameObject.FindGameObjectsWithTag("BaseEnemy");
            
            for (int i = 0; i < enemies.Length; i++)
            {
                enemies[i].GetComponent<BaseEnemy>().Kill(true);
            }
        }
        if (Input.GetButtonDown("Mute"))
        {
            if (audioMuted)
            {
                MasterMixer.SetFloat("MasterVolume", 0);
                audioMuted = false;
            }
            else
            {
                MasterMixer.SetFloat("MasterVolume", -100);
                audioMuted = true;
            }
        }

        // Hide tutorial message after time
        if (level == 1 && levelTimer > 3 && tutorialText.enabled)
        {
            tutorialText.enabled = false;
        }

        liveScore.text = score + " points";
        liveLevel.text = "level " + level;

        if (score > highScore)
        {
            liveHighscore.SetActive(true);
        }

        if (playerController.Dead)
        {
            deathMenu.SetActive(true);
            if (score > highScore)
            {
                highScore = score;
                deathMenu.transform.Find("DeathMessage").GetComponent<TMP_Text>().text = "New Highscore!";
            }

            deathMenu.transform.Find("Scoreboard").GetComponent<TMP_Text>().text = score + " points";
            Time.timeScale = 0;

            if (Input.GetButtonDown("Restart"))
            {
                liveHighscore.SetActive(false);
                Time.timeScale = 1;
                level = 1;
                score = 0;
                SceneManager.LoadScene(SceneManager.GetActiveScene().name);
            }
        } else if (GameObject.FindGameObjectsWithTag("BaseEnemy").Length == 0 || GameObject.FindGameObjectsWithTag("BaseEnemy").All(e => e.GetComponent<BaseEnemy>().Dead == true))
        {
            Time.timeScale = 0;
            winMenu.SetActive(true);
            winMenu.transform.Find("Scoreboard").GetComponent<TMP_Text>().text = score + " points";
            winMenu.transform.Find("WinMessage").GetComponent<TMP_Text>().text = "You cleared level " + level + "!";
            if (Input.GetButtonDown("Restart"))
            {
                Time.timeScale = 1;
                level++;
                liveHighscore.SetActive(false);
                SceneManager.LoadScene(SceneManager.GetActiveScene().name);
            }
        }
    }

    public void ProjectileFired()
    {
        score++;
    }

    public void CoinPickup()
    {
        score += 10;
    }

    private void SpawnEnemies(int difficulty)
    {
        float area = EnemySpawnArea.transform.lossyScale.x * EnemySpawnArea.transform.lossyScale.z;

        // Spawn enemies per level
        for (int j = 0; j < difficulty * EnemySpawnCountMultiplier; j++)
        {
            float x = EnemySpawnArea.transform.position.x + (Random.Range((EnemySpawnArea.transform.lossyScale.x / 2.5f), (-1) * (EnemySpawnArea.transform.lossyScale.x / 2.5f)));
            float y = EnemySpawnArea.transform.position.y + (Random.Range((EnemySpawnArea.transform.lossyScale.y / 2.5f), (-1) * (EnemySpawnArea.transform.lossyScale.y / 2.5f)));
            float z = EnemySpawnArea.transform.position.z + (Random.Range((EnemySpawnArea.transform.lossyScale.z / 2.5f), (-1) * (EnemySpawnArea.transform.lossyScale.z / 2.5f)));

            GameObject enemy = Instantiate(EnemyPrefab, new Vector3(x, y, z), Quaternion.identity);
            enemy.GetComponent<BaseEnemy>().ProjectileInterval = enemy.GetComponent<BaseEnemy>().ProjectileInterval - (difficulty * BulletDifficultyMultiplier);
        }
    }

    private void SpawnPickups(int level)
    {
        float area = PickupSpawnArea.transform.lossyScale.x * PickupSpawnArea.transform.lossyScale.z;

        for (int j = 0; j < (int)Random.Range(0, (level * SlowMotionPickupsPerLevel)); j++)
        {
            float x = PickupSpawnArea.transform.position.x + (Random.Range((PickupSpawnArea.transform.lossyScale.x / 2.5f), (-1) * (PickupSpawnArea.transform.lossyScale.x / 2.5f)));
            float y = PickupSpawnArea.transform.position.y + (Random.Range((PickupSpawnArea.transform.lossyScale.y / 2.5f), (-1) * (PickupSpawnArea.transform.lossyScale.y / 2.5f)));
            float z = PickupSpawnArea.transform.position.z + (Random.Range((PickupSpawnArea.transform.lossyScale.z / 2.5f), (-1) * (PickupSpawnArea.transform.lossyScale.z / 2.5f)));
            GameObject slowMotion = Instantiate(SlowMotionPickupPrefab, new Vector3(x, y, z), Quaternion.identity);
        }

        for (int j = 0; j < (int)Random.Range(0, (level * CoinPickupsPerLevel)); j++)
        {
            float x = PickupSpawnArea.transform.position.x + (Random.Range((PickupSpawnArea.transform.lossyScale.x / 2.5f), (-1) * (PickupSpawnArea.transform.lossyScale.x / 2.5f)));
            float y = PickupSpawnArea.transform.position.y + (Random.Range((PickupSpawnArea.transform.lossyScale.y / 2.5f), (-1) * (PickupSpawnArea.transform.lossyScale.y / 2.5f)));
            float z = PickupSpawnArea.transform.position.z + (Random.Range((PickupSpawnArea.transform.lossyScale.z / 2.5f), (-1) * (PickupSpawnArea.transform.lossyScale.z / 2.5f)));
            GameObject coin = Instantiate(CoinPickupPrefab, new Vector3(x, y, z), Quaternion.identity);
        }
    }

    void OnGUI()
    {
        if (StatsEnabled)
        {
            int w = Screen.width, h = Screen.height;

            GUIStyle style = new GUIStyle();

            float time = Time.unscaledDeltaTime;

            Rect guiContainer = new Rect(w / 200, 0, w, h * 2 / 100);
            style.alignment = TextAnchor.UpperLeft;
            style.fontSize = h * 2 / 100;
            style.normal.textColor = new Color(1f, 1f, 1f, 1.0f);
            float msec = time * 1000.0f;
            float fps = 1.0f / time;
            float ramReserved = Profiler.GetTotalReservedMemoryLong() / 1048576; // Converted to megabytes
            float ramAllocated = Profiler.GetTotalAllocatedMemoryLong() / 1048576; // Converted to megabytes
            string label = string.Format("{0:0.0} ms ({1:0.} fps)\n{2}MB Ram reserved\n{3}MB Ram allocated", msec, fps, ramReserved, ramAllocated);
            GUI.Label(guiContainer, label, style);
        }

        int w2 = Screen.width, h2 = Screen.height;

        GUIStyle style2 = new GUIStyle();

        Rect guiContainer2 = new Rect(w2 / 200, h2 - (h2 * 2 / 100), w2, h2 * 2 / 100);
        style2.alignment = TextAnchor.LowerLeft;
        style2.fontSize = h2 * 2 / 100;
        style2.normal.textColor = new Color(1f, 1f, 1f, 1.0f);
        string label2 = string.Format("Toggle Stats - T\nMovement - WASD\nActivate Shield - left mouse\nChange mouse sensitivity - 1, 2, 3 ({0})\nToggle God mode - G ({1})\nSkip Current Stage - C\nToggle Audio - M ({2})\nQuit - \'Escape\'", PlayerController.SelectedCursorSetting, PlayerController.GodMode ? "Enabled" : "Disabled", audioMuted ? "Disabled" : "Enabled");
        GUI.Label(guiContainer2, label2, style2);
    }
}
