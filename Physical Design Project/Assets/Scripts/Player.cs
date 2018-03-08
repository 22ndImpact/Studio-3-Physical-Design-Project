using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    //Objects
    public HeatBar heatBar;
    public Cannon[] cannons;

    //Variables
    public int Health;

    

    // Use this for initialization
    void Start ()
    {
		
	}
	
	// Update is called once per frame
	void Update ()
    {
        
		//When you let go of the fire key
            //If Charging
                //Fire Shot
            //If not charging
                //Do Nothing
            //Reset ChargeAmount to 0

        //When you hold down the fire key
            //Increase ChargeAmount
	}

    void UpdateShot()
    {
        
    }



    void UpdateHealth(int _Adjustment)
    {
        Health += _Adjustment;

        switch(Health)
        {
            case 3:
                break;
            case 2:
                break;
            case 1:
                break;
        }
    }
}
