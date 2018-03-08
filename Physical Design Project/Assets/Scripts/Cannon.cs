using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cannon : MonoBehaviour
{
    //The key map to activate the cannon.
    public KeyCode fireKey;

    //The projectile the cannon shoots
    public GameObject prefab_Projectile;

    public Player player;

    public AnimationCurve ReachCurve;
    public AnimationCurve RetractCurve;

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

    //ZAC
    public float ChargeTime = 1f;
    public float ChargeReachY = 10f;
    public float ChargePullbackY = -0.5f;
    float curCharge = 0f;
    float chargeStartTime;
    bool charging = false;
    ParticleSystem particles;
    Rigidbody2D rb;

    // Use this for initialization
    void Start ()
    {
        StartY = transform.localPosition.y;

        //ZAX
        particles = GetComponentInChildren<ParticleSystem>();
        rb = GetComponent<Rigidbody2D>();
    }
	
	// Update is called once per frame
	void Update ()
    {
        UpdateInput();
        UpdateReach();
        UpdatePosition();
	}

    void UpdateInput()
    {
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
        if(Input.GetKeyUp(fireKey) && charging)
        {
            //If not on cooldown, start Reaching
            if(reachState == ReachState.Static)
            {
                Debug.Log("attempt Reach");
                charging = false;
                particles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                Reach();
            }
        } 
    }

    void Reach()
    {
        reachState = ReachState.Reaching;
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
            reachState = ReachState.Retracting;
        }
        //If yo uare retracting and have reaching minimum
        else if(reachState == ReachState.Retracting && CurrentReach <= 0)
        {
            //Swap to static
            CurrentReach = 0;
            reachState = ReachState.Static;

            //ZEK
            curCharge = 0;
        }
    }

    void UpdatePosition()
    {
        //Debug.Log(curCharge);
        Vector3 storedPosition = transform.localPosition;
        if (Input.GetKey(fireKey) && reachState == ReachState.Static)
        {
            storedPosition.y = Mathf.LerpUnclamped(StartY, ChargePullbackY, curCharge);
        }
        else
        {
            storedPosition.y = Mathf.LerpUnclamped(StartY, Mathf.Lerp(EndY, ChargeReachY, curCharge), EvaluatedReach);
        }
        transform.localPosition = storedPosition;

    }

    private void OnTriggerEnter(Collider other)
    {
        //When you are hit by the projectile
        if(other.tag == "Projectile")
        {
            Debug.Log("Hit by Projectile");
        }
    }
}
