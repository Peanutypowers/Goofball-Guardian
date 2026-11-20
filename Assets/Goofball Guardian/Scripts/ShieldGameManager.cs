using UnityEngine;
using TMPro;

public class ShieldGameManager : MonoBehaviour
{
    public ShieldController shield;
public Transform creature;
    public GameObject projectilePrefab;

    public GameObject[] hearts;
      public GameObject titleScreen;
    public GameObject winScreen;
     public GameObject loseScreen;
    public GameObject hud; // timer, score and hearts while playing

    // UI texts
    public TMP_Text timerText;
    public TMP_Text scoreText;
    public TMP_Text messageText; // GO! BLOCKED! OUCH!
    public TMP_Text finalScoreText; // shows on win/lose

    public AudioSource audioSource;
    public AudioClip blockSound;
    public AudioClip hitSound;
     public AudioClip gameOverSound;

    public int startLives = 3;
       public float roundTime = 90f;
    public float spawnDistance = 6f;

    [HideInInspector] public bool playing = false;
     bool gameOver = false;
    int lives;
    int score;
    int highScore;
    float messageTimer;
     float timeLeft;
    float timePlayed;
     float spawnTimer;
    float gameOverTime;
    // feedback
    float creatureFlashTimer;
    float shakeTimer;
    Vector3 cameraStartPos;
    Renderer creatureRend;
    Color creatureColor;
    void Start()
    {
        cameraStartPos = Camera.main.transform.position;

        creatureRend = creature.GetComponent<Renderer>();
        creatureColor = creatureRend.material.color;

        highScore = PlayerPrefs.GetInt("HighScore", 0);
        lives = startLives;
        UpdateHearts();
        ShowScreen(titleScreen);
    }
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.R)) StartGame();

        if (playing)
        {
            timePlayed += Time.deltaTime;
            timeLeft -= Time.deltaTime;

            if (timeLeft <= 0) EndGame(true);

            timerText.text = "Time: " + Mathf.CeilToInt(timeLeft);
            scoreText.text = "Score: " + score;

            // spaw faster 
            float difficulty = Mathf.Clamp01(timePlayed / 60f);

            spawnTimer -= Time.deltaTime;
            if (spawnTimer <= 0)
            {
                SpawnProjectile(Mathf.Lerp(2.2f, 5f, difficulty));
                spawnTimer = Mathf.Lerp(1.8f, 0.6f, difficulty);
            }
        }
        else if (gameOver)
        {
            if (shield.buttonPressed && Time.time - gameOverTime > 1.5f)
            {
                gameOver = false;
                lives = startLives;
                UpdateHearts();
                ShowScreen(titleScreen);
            }
        }
        else if (shield.buttonPressed)
        {
            StartGame();
        }

        // message pops up for a sec
        messageTimer -= Time.deltaTime;
        messageText.gameObject.SetActive(messageTimer > 0);

        // flashes red when hit
        creatureFlashTimer -= Time.deltaTime;
        creatureRend.material.color = creatureFlashTimer > 0 ? Color.red : creatureColor;
        // screen shake hehe
        shakeTimer -= Time.deltaTime;

        if (shakeTimer > 0)
            Camera.main.transform.position = cameraStartPos + (Vector3)Random.insideUnitCircle * 0.2f;
        else
            Camera.main.transform.position = cameraStartPos;
    }
    void StartGame()
    {
        foreach (ShieldProjectile p in FindObjectsByType<ShieldProjectile>(FindObjectsSortMode.None))
            Destroy(p.gameObject);

        playing = true;
        gameOver = false;
        lives = startLives;
        score = 0;
        timeLeft = roundTime;
        timePlayed = 0;
        spawnTimer = 1.5f;

        shield.Calibrate();
        UpdateHearts();
        ShowScreen(null);   
        ShowMessage("GO!");
    }

    void SpawnProjectile(float speed)
    {
        // 8 directions 
        float rad = Random.Range(0, 8) * 45f * Mathf.Deg2Rad;
        Vector3 pos = creature.position + new Vector3(Mathf.Cos(rad), Mathf.Sin(rad), 0) * spawnDistance;

        GameObject p = Instantiate(projectilePrefab, pos, Quaternion.identity);

        ShieldProjectile projectile = p.GetComponent<ShieldProjectile>();
        projectile.game = this;
        projectile.speed = speed;
    }
    public void Blocked()
    {
        score += 10;
        PlaySound(blockSound);
        ShowMessage("BLOCKED! +10");
    }
    public void CreatureHit()
    {
        lives--;
        UpdateHearts();

        PlaySound(hitSound);
        ShowMessage("OUCH!");
        creatureFlashTimer = 0.4f;
        shakeTimer = 0.3f;

        if (lives <= 0) EndGame(false);
    }
    void EndGame(bool survived)
    {
        playing = false;
        gameOver = true;
        gameOverTime = Time.time;

        if (score > highScore)
        {
            highScore = score;
            PlayerPrefs.SetInt("HighScore", highScore);
        }
        finalScoreText.text = "Score: " + score + "\nHigh score: " + highScore + "\nPress the button to continue";

        ShowScreen(survived ? winScreen : loseScreen);
        PlaySound(gameOverSound);
    }

    void UpdateHearts()
    {
        for (int i = 0; i < hearts.Length; i++)
        {
            if (hearts[i] != null)
                hearts[i].SetActive(i < lives);
        }
    }
    // shows one screen and hides the others
    void ShowScreen(GameObject screen)
    {
        if (titleScreen != null) titleScreen.SetActive(screen == titleScreen);
    if (winScreen != null) winScreen.SetActive(screen == winScreen);
        if (loseScreen != null) loseScreen.SetActive(screen == loseScreen);
        if (hud != null) hud.SetActive(screen == null); // null = playing
        finalScoreText.gameObject.SetActive(screen == winScreen || screen == loseScreen);
    }

    void ShowMessage(string text)
    {
        messageText.text = text;
        messageTimer = 0.8f;
    }

    void PlaySound(AudioClip clip)
    {
        if (audioSource != null && clip != null)
            audioSource.PlayOneShot(clip);
    }
}