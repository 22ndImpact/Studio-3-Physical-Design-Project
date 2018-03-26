using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour
{
    public static GameController instance;

    public GameObject spinner;

    public int Player1Score;
    public int Player2Score;

    public bool BallSpinning;
    public float BallSpinSpeed;
    public float BallSpinTimer;
    public float BallSpinTimerVariance;

    public float pauseTime;
    public float pauseTimer;

    public float BallSpinTime;
    public float BallSpinTimeExtraMin;
    public float BallSpinTimeExtraMax;

    public float PossibleLaunchAngle;

    public enum GameState
    {
        PreGame,
        GameStarting,
        InGame,
        GameOver
    }
    public GameState gameState;


    Ball ball;

    private void Awake()
    {
        instance = this;
    }

    // Use this for initialization
    void Start ()
    {
        gameState = GameState.PreGame;

        //Find the ball object and track it
        ball = GameObject.FindGameObjectWithTag("Ball").GetComponent<Ball>();
        //BallSpinning = true;

        //BallSpinTimer = BallSpinTime + Random.Range(0, BallSpinTimerVariance);
        //BallSpinning = true;
    }
	
	// Update is called once per frame
	void Update ()
    {
        
        UpdateStates();
    }

    void UpdateStates()
    {
        switch (gameState)
        {
            case GameState.PreGame:
                UpdatePreGame();
                break;
            case GameState.GameStarting:
                UpdateGameStarting();
                break;
            case GameState.InGame:
                UpdateInGame();
                break;
            case GameState.GameOver:
                UpdateGameOver();
                break;
        }
    }

    void UpdatePreGame()
    {
        if(Input.GetKeyDown(KeyCode.G) || Input.GetKeyDown(KeyCode.DownArrow))
        {
            //Debug.Log(Input.inputString);
            StartNewGame();
        }
    }

    void StartNewGame()
    {
        gameState = GameState.GameStarting;

        BallSpinTimer = BallSpinTime + Random.Range(0, BallSpinTimerVariance);
        BallSpinning = true;
    }

    void UpdateGameStarting()
    {
        SpinBall();
    }

    void UpdateInGame()
    {

    }

    void UpdateGameOver()
    {
        if(pauseTimer > 0)
        {
            pauseTimer -= Time.deltaTime;
        }
        else
        {
            if (Input.GetKeyDown(KeyCode.G) || Input.GetKeyDown(KeyCode.DownArrow))
            {
                //Debug.Log(Input.inputString);
                ResetField();
            }
        }
        
    }

    public void EndGame(int WinningPlayer)
    {
        gameState = GameState.GameOver;
        Debug.Log("Player " + WinningPlayer + " wins!");

        pauseTimer = pauseTime;
    }

    void ResetField()
    {
        spinner.GetComponent<MeshRenderer>().enabled = true;

        Debug.Log("Resetting field");
        ball.transform.position = new Vector3(0, 0, 0);
        ball.velocity = new Vector3(0, 0, 0);

        gameState = GameState.PreGame;
    }

    void SpinBall()
    {
        if(BallSpinning)
        {
            //Spin the ball
            this.transform.Rotate(0, 0, -1 * BallSpinSpeed * Time.deltaTime);

            float CurrentRotation = (this.transform.rotation).eulerAngles.z;

            //It the timer is running
            if (BallSpinTimer > 0)
            {
                //Reduce timer
                BallSpinTimer -= Time.deltaTime;
            }
            //If the ball is outside the allowed ranges
            else if ((CurrentRotation > 60  - PossibleLaunchAngle && CurrentRotation < 60  + PossibleLaunchAngle) ||
                     (CurrentRotation > 120 - PossibleLaunchAngle && CurrentRotation < 120 + PossibleLaunchAngle) ||
                     (CurrentRotation > 240 - PossibleLaunchAngle && CurrentRotation < 240 + PossibleLaunchAngle) ||
                     (CurrentRotation > 300 - PossibleLaunchAngle && CurrentRotation < 300 + PossibleLaunchAngle))
            {
                BallSpinning = false;
                pauseTimer = pauseTime;
            }
            else
            {
                //Add a random amount of extra time from 0.1f and Ball
                //Debug.Log("adding extra time");
                BallSpinTimer = 0.001f;
            }
        }

        if (BallSpinning == false)
        {
            if (pauseTimer > 0)
            {
                pauseTimer -= Time.deltaTime;
            }
            else
            {
                LaunchBall();
            }
        }
    }

    void LaunchBall()
    {
        //Debug.Log("LaunchBall");

        spinner.GetComponent<MeshRenderer>().enabled = false;

        ball.velocity = transform.up * ball.InitialSpeed;

        gameState = GameState.InGame;
    }
}
