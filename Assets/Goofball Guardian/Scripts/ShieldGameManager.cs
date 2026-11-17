using UnityEngine;

public class ShieldGameManager : MonoBehaviour
{
    public ShieldController shield;
public Transform creature;
    public GameObject projectilePrefab;

    public GameObject[] hearts;
      public GameObject titleScreen;
    public GameObject winScreen;
     public GameObject loseScreen;

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
     float timeLeft;
    float timePlayed;
     float spawnTimer;
    float gameOverTime;
    // feedback
    float creatureFlashTimer;
    float shakeTimer;
    Vector3 cameraStartPos;
    SpriteRenderer creatureSprite;
    Color creatureColor;
    void Start()
    {
        cameraStartPos = Camera.main.transform.position;

        creatureSprite = creature.GetComponent<SpriteRenderer>();
        creatureColor = creatureSprite.color;

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

        // flashes red when hit
        creatureFlashTimer -= Time.deltaTime;
        creatureSprite.color = creatureFlashTimer > 0 ? Color.red : creatureColor;
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
        timeLeft = roundTime;
        timePlayed = 0;
        spawnTimer = 1.5f;

        shield.Calibrate();
        UpdateHearts();
        ShowScreen(null);   
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
        PlaySound(blockSound);
    }
    public void CreatureHit()
    {
        lives--;
        UpdateHearts();

        PlaySound(hitSound);
        creatureFlashTimer = 0.4f;
        shakeTimer = 0.3f;

        if (lives <= 0) EndGame(false);
    }
    void EndGame(bool survived)
    {
        playing = false;
        gameOver = true;
        gameOverTime = Time.time;

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
    }

    void PlaySound(AudioClip clip)
    {
        if (audioSource != null && clip != null)
            audioSource.PlayOneShot(clip);
    }
}