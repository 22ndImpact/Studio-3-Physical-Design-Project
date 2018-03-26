using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cannon : MonoBehaviour
{
    public bool ChargeShot = false;

    //The key map to activate the cannon.
    public KeyCode fireKey;

    //The projectile the cannon shoots
    public GameObject prefab_Projectile;

    public GameObject block;

    public Player player;

    public AnimationCurve ReachCurve;
    private AnimationCurve RetractCurve;
    public AnimationCurve EmptyRetractCurve;
    public AnimationCurve HitRetractCurve;

    Ball ball;

    public enum ReachState
    {
        Static,
        Reaching,
        Retracting
    }
    public ReachState reachState;

    //Variables
    public float MaxCharge;
    public float Charge;
    public float ChargeRate;

    public float ReachRate;
    public float RetractRate;
    public float CurrentReach;
    public float EvaluatedReach;

    public float StartY;
    public float EndY;
    public float EmptyEndY;
    public float HitEndY;

	public bool StaticHitStall;
	public float StaticStallTime;
    public float StallTime;

    public float stallMinimumVelocity;
    public float hitForce;
    public float hitStallMultiplier;

    public bool OverrideBounce;
    bool hitBall = false;

    public BoxCollider boxCollider;

    public Vector3 InitialColliderCenter;
    public Vector3 CurrentColliderOffset;

    public Vector3[] PositionStates = new Vector3[3];

    //ZAC
    public float ChargeTime;
    public float ChargeReachY = 10f;
    private float ChargePullbackY;
    public float ChargePullbackOffset;
    float curCharge = 0f;
    float chargeStartTime;
    bool charging = false;
    ParticleSystem particles;

    // Use this for initialization
    void Start ()
    {
        //Connects the parent object
        player = transform.parent.gameObject.GetComponent<Player>();
        player.cannons.Add(this);

        //Stores the local starting position of the cannon
        StartY = transform.localPosition.y;

        //Stores the local charged position of the cannon
        ChargePullbackY = transform.localPosition.y - ChargePullbackOffset;

        //Sets the initial retract curve to be the empthy one
        RetractCurve = EmptyRetractCurve;

        //Sets the initial EndY to be Empthy
        EndY = EmptyEndY;

        //Set the bos collider
        boxCollider = GetComponent<BoxCollider>();

        //Store the initial center of the box collider
        InitialColliderCenter = boxCollider.center;

        //Set all the initial position states
        for (int i = 0; i < PositionStates.Length; i++)
        {
            PositionStates[i] = transform.localPosition;
        }

        //Find the ball object and track it
        ball = GameObject.FindGameObjectWithTag("Ball").GetComponent<Ball>();

        FindBlock();

        //ZAX
        particles = GetComponentInChildren<ParticleSystem>();
    }

    void FindBlock()
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            if(transform.GetChild(i).name == "Block")
            {
                block = transform.GetChild(i).gameObject;
            }
        }
    }
	
	// Update is called once per frame
	void Update ()
    {
        UpdateInput();
        
        if(StallTime <= 0)
        {
            //If a ball has been hit since last update  launch it
            if (hitBall)
            {
                LaunchBall();
            }

            //Updates the reach state
            UpdateReach();

            //Moves the actual cannon
            UpdatePosition();

            //Updates the positions states to track velocity
            UpdatePositionStates();

            //Adjusts the position of the cannon collider
            //UpdateColliderPosition();
        }
        else
        {
            StallTime -= Time.deltaTime;
        }
	}

    void UpdatePositionStates()
    {
        //Shits the "current position [0]" to the "previous position [1]"
        PositionStates[1] = PositionStates[0];

        //Updates the "current position [0]" to the current transform
        PositionStates[0] = transform.localPosition;

        //Calculates the "velocity [2]" by comparing [0] and [1]
        PositionStates[2] = PositionStates[0] - PositionStates[1];
    }

    void UpdateInput()
    {
        if(GameController.instance.gameState == GameController.GameState.InGame)
        {
            if (ChargeShot)
            {
                bool otherCharging = false;
                //If another cannon on the same player isnt currrently charging
                foreach(Cannon cannon in player.cannons)
                {
                    if(cannon != this && cannon.charging)
                    {
                        otherCharging = true;
                    }
                }

                if(!otherCharging)
                {
                    #region Charge Firing
                    if (Input.GetKeyDown(fireKey) && reachState == ReachState.Static)
                    {
                        chargeStartTime = Time.time;
                        charging = true;
                        particles.Play();
                    }


                    if (Input.GetKey(fireKey) && reachState == ReachState.Static && charging)
                    {
                        curCharge = Mathf.Clamp01((Time.time - chargeStartTime) / ChargeTime);
                    }

                    //When you let go of the fire key
                    if (Input.GetKeyUp(fireKey) && charging)
                    {
                        //If not on cooldown, start Reaching
                        if (reachState == ReachState.Static)
                        {
                            //Debug.Log("attempt Reach");
                            charging = false;
                            particles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                            Reach();
                        }
                    }
                    #endregion
                }
            }
            else
            {
                #region Instant Firing
                if (Input.GetKeyDown(fireKey))
                {
                    //If not on cooldown, start Reaching
                    if (reachState == ReachState.Static)
                    {
                        curCharge = 1;
                        //Debug.Log("attempt Reach");
                        charging = false;
                        particles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                        Reach();
                    }
                }
                #endregion
            }
        }
    }

    void Reach()
    {
        reachState = ReachState.Reaching;
    }

    void Retract(bool BallHit)
    {
        if (BallHit)
        {
            //Set the current reach to one
            CurrentReach = 1;

            //Set the retract curve to the hit one
            RetractCurve = HitRetractCurve;

            //Determine the HitEndY and then apply it to the HitY
            //Set the current Y to be the HitEndY
            HitEndY = transform.localPosition.y;// Mathf.LerpUnclamped(StartY, Mathf.Lerp(ChargeReachY, EndY, curCharge), EvaluatedReach);

            //Set the EndY to the Hit End Y
            EndY = HitEndY;
        }
        else
        {
            reachState = ReachState.Retracting;
        }
        
    }

    void ResetToStatic()
    {
        //Swap to static
        CurrentReach = 0;
        reachState = ReachState.Static;

        //ZEK
        curCharge = 0;


        //Resets the initial retract curve to be the empty one
        RetractCurve = EmptyRetractCurve;

        //Resets the initial EndY to be Empthy
        EndY = EmptyEndY;
    }

    void UpdateReach()
    {
        //Update the current reach based on reach state
        if(reachState == ReachState.Reaching)
        {
            CurrentReach += ReachRate * Time.deltaTime;
            EvaluatedReach = ReachCurve.Evaluate(CurrentReach);
        }
        else if(reachState == ReachState.Retracting)
        {
            CurrentReach -= RetractRate * Time.deltaTime;
            EvaluatedReach = RetractCurve.Evaluate(CurrentReach);
        }
        else
        {
            EvaluatedReach = 0;
        }

        //If you are reaching and have reached maximum
        if (reachState == ReachState.Reaching && CurrentReach >= 1)
        {
            //Swap to retracting
            CurrentReach = 1;

            //Retract
            Retract(false);
        }
        //If you are retracting and have reaching minimum
        else if(reachState == ReachState.Retracting && CurrentReach <= 0)
        {
            ResetToStatic();
        }
    }

    void UpdatePosition()
    {
        //Debug.Log(curCharge);
        Vector3 storedPosition = transform.localPosition;

        //If charging
        if (Input.GetKey(fireKey) && reachState == ReachState.Static)
        {
            storedPosition.y = Mathf.LerpUnclamped(StartY, ChargePullbackY, curCharge);
        }
        //Otherwise while moving
        else
        {
            storedPosition.y = Mathf.LerpUnclamped(StartY, Mathf.Lerp(ChargeReachY, EndY, curCharge), EvaluatedReach);
        }

        transform.localPosition = storedPosition;

        //Adjusting the position of the cannon if it shoots past the ball
        float offset = (ball.transform.position - transform.position).x;

        if(reachState == ReachState.Reaching)
        {
            if(ball.transform.position.y < transform.position.y + transform.lossyScale.x / 2 && ball.transform.position.y > transform.position.y - transform.lossyScale.x / 2)
            //If its a left side cannon
            if (transform.up.x > 0)
            {
                if (offset < 0)
                {
                    //Debug.Log("Offset: " + gameObject.name);
                    AdjustCannonCollisionPosition(ball.transform.position);
                }
            }
            //If it is a right side cannon
            else if (transform.up.x < 0)
            {
                if (offset > 0)
                {
                    //Debug.Log("Offset: " + gameObject.name);
                    AdjustCannonCollisionPosition(ball.transform.position);
                }
            }
        }
    }

    void BallCollision(Ball ball)
    {
        float currentXVelocity = Mathf.Abs(ball.velocity.x);

        //Flip direction
        if(ball.useStaticSpeeds)
        {
            ball.velocity.x = transform.up.x;// * ball.CannonBounceSpeed;
            //ball.velocity.y = 0;

            if (OverrideBounce)
            {
                //ball.velocity.y
                Debug.Log((transform.position.y - ball.transform.position.y) / (transform.localScale.x/2));
                ball.velocity.y = (ball.transform.position.y - transform.position.y) / (transform.localScale.x / 2);
                ball.velocity *= ball.CannonBounceSpeed;
            }
        }
        else
        {
            ball.velocity.x = transform.up.x * (currentXVelocity + hitForce);
        }


        
    }

    void UpdateColliderPosition()
    {
        //If you are reaching
        if(reachState == ReachState.Reaching)
        {
            //Dividing by delta time makes shiz consistent yo. First frame out though is faster because its making up fo the charge distance that it was pulled back
            //Debug.Log("Reach Velocity: " + PositionStates[2] / Time.deltaTime);
            //Push the collider out in from of the cannon to predict collisions before the happen, in proportion to the velocity

            if(PositionStates[2].y == 0)
            {
                Debug.Log("No current velocity");
            }
            else
            {
                Debug.Log(PositionStates[2].y);
            }
            CurrentColliderOffset.y = (PositionStates[2].y / Time.deltaTime / 100);

            boxCollider.center = InitialColliderCenter + CurrentColliderOffset;
        }
        else
        {
            boxCollider.center = InitialColliderCenter;
        }
        
    }

    void AdjustCannonCollisionPosition(Vector3 ballPosition)
    {
        float distance = 999;

        int maximumLoops = 20;

        while (Mathf.Abs(distance) > 0.1f)
        {
            //move the cannon position's y to between half of to previous posision [1] and ballposition.y
            //Need to work in global positions here because the ball is in global space
            Vector3 storedPosition = transform.position;

            storedPosition.x = (storedPosition.x + ballPosition.x) / 2;

            distance = storedPosition.x - ballPosition.x;

            this.transform.position = storedPosition;

            maximumLoops -= 1;
            if(maximumLoops <= 0)
            {
                Debug.Log("Couldnt get close enough, Breaking");
                break;
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        //When you are hit by the projectile
        if (other.tag == "Ball")
        {
            //Get the position of the ball 1 frame ago
            Vector3 previousBallPos = ball.transform.position - (ball.velocity * Time.smoothDeltaTime);

            //Debug.Log("Ball Position: " + ball.transform.position);
            //Debug.Log("Previous Ball Position Y: " + previousBallPos.y);

            //Debug.Log("Cannon Y Position: " + transform.position.y);
            //Debug.Log("Cannon Upper Boundary: " + (transform.position.y + transform.localScale.x / 2));



            if (previousBallPos.y > (transform.position.y + transform.localScale.x / 2) || previousBallPos.y < (transform.position.y - transform.localScale.x / 2))
            {
                Debug.Log("Side");
                ball.velocity.y *= -1;
            }
            else if(reachState == ReachState.Reaching)
            {
                //Reposition the cannon
                AdjustCannonCollisionPosition(other.transform.position);

                //If the speed of the hit exceeds the threshold, then start stallign on hit
                if (Mathf.Abs(ball.velocity.x) > stallMinimumVelocity)
                {
					if (StaticHitStall) {
						//Set a stall time
						StallTime = StaticStallTime;

						//Stall the ball too
						ball.StallTime = StaticStallTime;
					} else {
						//Set a stall time
						StallTime = Mathf.Abs (ball.velocity.x) * (hitStallMultiplier);

						//Stall the ball too
						ball.StallTime = Mathf.Abs (ball.velocity.x) * (hitStallMultiplier);
					}
                    
                }

                //Begin the retraction process of the cannon
                Retract(true);

                //Set the ball hit flag to true to queue up the next available launch ball function
                hitBall = true;

                Debug.Log("Reaching");
            }
            else if (reachState == ReachState.Reaching)
            {
                Debug.Log("Retracting");
                ball.velocity.x *= -1;
            }
            else if(reachState == ReachState.Static)
            {
                Debug.Log("Static");
                ball.velocity.x *= -1;
            }



        }

    }

    void LaunchBall()
    {
        BallCollision(ball);
        hitBall = false;
    }

    void Stalling()
    {
        if(Input.GetKey(fireKey))
        {

        }
    }

    private void OnDrawGizmos()
    {
    }
}
