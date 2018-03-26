using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ball : MonoBehaviour
{
    public Vector3 velocity;
    public SphereCollider sc;

    public float InitialSpeed;
    public float WallBounceSpeed;
    public float CannonBounceSpeed;

    public bool useStaticSpeeds;

    public float StallTime;

	// Use this for initialization
	void Start ()
    {
        sc = GetComponent<SphereCollider>();
	}
	
	// Update is called once per frame
	void Update ()
    {
        if(StallTime <= 0)
        {
            UpdateMovement();
        }
        else
        {
            StallTime -= Time.smoothDeltaTime;
        }
        

        if (Input.GetKeyDown(KeyCode.I))
        {
            velocity.y += 10;
        }

        if (Input.GetKeyDown(KeyCode.K))
        {
            velocity.y -= 10;
        }

        if (Input.GetKeyDown(KeyCode.J))
        {
            velocity.x -= 10;
        }

        if (Input.GetKeyDown(KeyCode.L))
        {
            velocity.x += 10;
        }
    }

    void UpdateMovement()
    {
        this.transform.position += velocity * Time.deltaTime;
    }

    private void OnTriggerEnter(Collider other)
    {
        //When you hit a wall
        if (other.tag == "VerticalWall")
        {
            //Flip your y velocity
            velocity.y *= -1;

            if(useStaticSpeeds)
            {
                velocity = velocity.normalized * WallBounceSpeed;
            }
            //Debug.Log("Vertical Wall");
        }
        else if (other.tag == "HorizontalWall")
        {
            //Flip your y velocity
            velocity.x *= -1;
            //Debug.Log("Horizontal Wall");
            velocity = Vector3.zero;

            if(other.transform.position.x < 0)
            {
                GameController.instance.EndGame(1);
            }

            if (other.transform.position.x > 0)
            {
                GameController.instance.EndGame(2);
            }
        }
    }
}
