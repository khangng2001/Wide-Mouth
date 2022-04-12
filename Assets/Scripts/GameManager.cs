using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public GameObject pacman;
    public GameObject leftWarpNode;
    public GameObject rightWarpNode;

    public AudioSource siren;
    public AudioSource munch_1;
    public AudioSource munch_2;
    public int currentMunch = 0;

    public int score;
    public Text scoreText;

    public GameObject ghostNodeLeft;
    public GameObject ghostNodeRight;
    public GameObject ghostNodeCenter;
    public GameObject ghostNodeStart;

    public GameObject redGhost;
    public GameObject pinkGhost;
    public GameObject blueGhost;
    public GameObject orangeGhost;

    public EnemyController redGhostController;
    public EnemyController pinkGhostController;
    public EnemyController blueGhostController;
    public EnemyController orangeGhostController;

    public int totalPellets;
    public int pelletsLeft;
    public int pelletCollectedOnThisLife;

    public bool hadDeathOnThisLevel = false;

    public bool gameIsRunning;

    public List<NodeController> nodeControllers = new List<NodeController>();

    public bool newGame;
    public bool clearedLevel;

    public AudioSource startGameAudio;
    public AudioSource death;
    public AudioSource powerPelletAudio;
    public AudioSource respawningAudio;
    public AudioSource ghostEatenAudio;

    //life system
    public Image[] hearts;
    public int lives;
    public int currentLevel;

    public Image blackBackground;
    public Text gameOverText;

    public bool isPowerPelletRunning = false;
    public float currentPowerPelletTime = 0;
    public float powerPelletTimer = 8f;
    public int powerPelletMultiplier = 1; 

    public enum GhostMode
    {
        chase, scatter
    }

    public GhostMode currentGhostMode;

    public int[] ghostModeTimers = new int[] { 7, 20, 7, 20, 5, 20, 5 };
    public int ghostModeTimerIndex;
    public float ghostModeTimer = 0;
    public bool runningTimer;
    public bool completedTimer;

    void Awake()
    {
        newGame = true;
        clearedLevel = false;
        blackBackground.enabled = false;
        redGhostController = redGhost.GetComponent<EnemyController>();
        pinkGhostController = pinkGhost.GetComponent<EnemyController>();
        blueGhostController = blueGhost.GetComponent<EnemyController>();
        orangeGhostController = orangeGhost.GetComponent<EnemyController>();

        ghostNodeStart.GetComponent<NodeController>().isGhostStartingNode = true;

        pacman = GameObject.Find("Player");

        StartCoroutine(SetUp());
    }
    
    public IEnumerator SetUp()
    {
        ghostModeTimerIndex = 0;
        ghostModeTimer = 0;
        completedTimer = false;
        runningTimer = true;
        gameOverText.enabled = false;
        //If pacman clears a level, a background will appear covering the level, and the game will pause for 0.1 seconds
        if (clearedLevel)
        {
            blackBackground.enabled = true;
            //Activate background   
            yield return new WaitForSeconds(0.1f);
        }
        blackBackground.enabled = false;
        pelletCollectedOnThisLife = 0;
       // currentGhostMode = GhostMode.chase;
        currentGhostMode = GhostMode.scatter;
        gameIsRunning = false;
        currentMunch = 0;

        float waitTimer = 1f;

        if (clearedLevel || newGame)
        {
            pelletsLeft = totalPellets;
            waitTimer = 4f;
            //Pellets will respawn when pacman clears level or start a new game
            for (int i = 0; i < nodeControllers.Count; i++)
            {
                nodeControllers[i].RespawnPellet();
            }
        }

        if (newGame)
        {
            startGameAudio.Play();
            score = 0;
            scoreText.text = "Score: " + score.ToString();
            lives = 3;
            currentLevel = 1;
        }

        pacman.GetComponent<PlayerController>().SetUp();

        redGhostController.SetUp();
        pinkGhostController.SetUp();
        blueGhostController.SetUp();
        orangeGhostController.SetUp();

        newGame = false;
        clearedLevel = false;
        yield return new WaitForSeconds(waitTimer);

        StartGame();

    }

    private void StartGame()
    {
        gameIsRunning = true;
        siren.Play();
    }

    private void StopGame()
    {
        gameIsRunning = false;
        siren.Stop();
        powerPelletAudio.Stop();
        respawningAudio.Stop();
        pacman.GetComponent<PlayerController>().Stop();
    }

    
    void Update()
    {
        if (!gameIsRunning)
        {
            return; 
        }

        if (redGhostController.ghostNodeState == EnemyController.GhostNodeStateEnum.respawning 
        || pinkGhostController.ghostNodeState == EnemyController.GhostNodeStateEnum.respawning
        || blueGhostController.ghostNodeState == EnemyController.GhostNodeStateEnum.respawning
        || orangeGhostController.ghostNodeState == EnemyController.GhostNodeStateEnum.respawning)
        {
            if (!respawningAudio.isPlaying)
            {
                respawningAudio.Play();
            }
        }
        else
        {
            if (respawningAudio.isPlaying)
            {
                respawningAudio.Stop();
            }
        }

        if (!completedTimer && runningTimer)
        {
            ghostModeTimer += Time.deltaTime;
            if (ghostModeTimer >= ghostModeTimers[ghostModeTimerIndex])
            {
                ghostModeTimer = 0;
                ghostModeTimerIndex++;
                if (currentGhostMode == GhostMode.chase)
                {
                    currentGhostMode = GhostMode.scatter;
                }
                else
                {
                    currentGhostMode = GhostMode.chase;
                }
                if (ghostModeTimerIndex == ghostModeTimers.Length)
                {
                    completedTimer = true;
                    runningTimer = false;
                    currentGhostMode = GhostMode.chase;
                }
            }
        }
        if (isPowerPelletRunning)
        {
            currentPowerPelletTime += Time.deltaTime;
            if (currentPowerPelletTime >= powerPelletTimer)
            {
                isPowerPelletRunning = false;
                currentPowerPelletTime = 0;
                powerPelletAudio.Stop();
                siren.Play();
                powerPelletMultiplier += 1;
            }
        }
    }
    public void GotPelletFromNodeController(NodeController nodeController)
    {
        nodeControllers.Add(nodeController);
        totalPellets++;
        pelletsLeft++;
    }
    public void AddToScore(int amount)
    {
        score += amount;
        scoreText.text = "Score: "+ score.ToString();
    }

    public IEnumerator CollectedPellet(NodeController nodeController)
    {
        if (currentMunch == 0)
        {
            munch_1.Play();
            currentMunch = 1;
        }
        else if (currentMunch ==1)
        {
            munch_2.Play();
            currentMunch = 0;
        }
        pelletsLeft--;
        pelletCollectedOnThisLife++;

        int requiredBluePellets = 0;
        int requiredOrangePellets = 0;

        if (hadDeathOnThisLevel)
        {
            requiredBluePellets = 12;
            requiredOrangePellets = 32;
        }
        else
        {
            requiredBluePellets = 30;
            requiredOrangePellets = 60;
        }

        if (pelletCollectedOnThisLife >= requiredBluePellets && !blueGhost.GetComponent<EnemyController>().leftHomeBefore)
        {
            blueGhost.GetComponent<EnemyController>().readytoLeaveHome = true;
        }
        if (pelletCollectedOnThisLife >= requiredOrangePellets && !orangeGhost.GetComponent<EnemyController>().leftHomeBefore)
        {
            orangeGhost.GetComponent<EnemyController>().readytoLeaveHome = true;
        }
        //Add to our score
        AddToScore(10);

        //Check if there are any pellets left
        if (pelletsLeft==0)
        {
            currentLevel++;
            clearedLevel = true;
            StopGame();
            yield return new WaitForSeconds(1);
            StartCoroutine(SetUp());
        }

        //Is this a power pellet
        if (nodeController.isPowerPellet)
        {
            siren.Stop();
            powerPelletAudio.Play();
            isPowerPelletRunning = true;
            currentPowerPelletTime = 0;

            redGhostController.SetFrightened(true);
            pinkGhostController.SetFrightened(true);
            blueGhostController.SetFrightened(true);
            orangeGhostController.SetFrightened(true);

        }
    }

    public IEnumerator PauseGame(float timeToPause)
    {
        gameIsRunning = false;
        yield return new WaitForSeconds(timeToPause);
        gameIsRunning = true;
    }

    public void GhostEaten()
    {
        ghostEatenAudio.Play();
        AddToScore(400 * powerPelletMultiplier);
        powerPelletMultiplier++;
        StartCoroutine(PauseGame(1));
    }

   public IEnumerator PlayerEaten()
    {
        hadDeathOnThisLevel = true;
        StopGame();
        yield return new WaitForSeconds(1);

        redGhostController.SetVisible(false);
        pinkGhostController.SetVisible(false);
        blueGhostController.SetVisible(false);
        orangeGhostController.SetVisible(false);

        pacman.GetComponent<PlayerController>().Death();
        death.Play();
        yield return new WaitForSeconds(3);
        if (lives == 0)
        {
            newGame = true;
            //Display gameover text
            gameOverText.enabled = true;
            hearts[0].enabled = false;
            yield return new WaitForSeconds(3);
            for (int i = 0; i < hearts.Length; i++)
            {
                hearts[i].enabled = true;
            }
        }
        else 
        {
            hearts[lives].enabled = false;
            lives--;
        }

        StartCoroutine(SetUp());
    }
}
