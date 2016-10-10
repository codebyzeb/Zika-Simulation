using UnityEngine;
using System.Collections;

public class Movement : MonoBehaviour {

	public float moveSpeed;
	Vector2 direction;


	//Variables used for RandomMovement()
	private float delayTimer;
	private float randomDelay;
	private int noiseAction;
	private Vector2 noisePos;

	//Variables used for moving to other entities
	private bool touchingOther;
	private Transform otherEntity;

	//initialisation function
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public void RandomMovement()
	{	

		/*
		 * When a time period determined by randomDelay has passed,
		 * make a new random decision by choosing a new target
		 * location then chosing a random choice.
		 * noiseAction == 0 means don't move
		 * noiseAction == 1 means move to noisePos
		*/

		//delayTimer increased while RandomMovement called
		delayTimer += Time.deltaTime;
		if (delayTimer > randomDelay) {
			//delayTimer reset
			delayTimer = 0;
			noiseAction = Random.Range (0,2);
			randomDelay = (noiseAction == 0) ? Random.Range (0.5f, 2) : Random.Range (0.5f, 3);
			noisePos = new Vector2 (Random.Range (transform.position.x - transform.localScale.x * moveSpeed, transform.position.x + transform.localScale.x * moveSpeed), Random.Range (transform.position.y - transform.localScale.y * 5, transform.position.y + transform.localScale.y * 5));
		}
		else if (noiseAction == 1) {
			MoveToPosition (noisePos);
		}
	}

	Entity FindNearestEntity(string entityType)
	{
		return null;
	}

	public void MoveToEntity(Entity target)
	{
		if (!touchingOther) {
			MoveToPosition (target.transform.localPosition);
		} else if (transform.parent != otherEntity); {
			transform.SetParent (otherEntity);
		}
	}

	void MoveToPosition(Vector2 target)
	{

		/*
		 * Direction calculated by normalising the difference between
		 * the two positions, local position and target position.
		 * Then local position is changed according to moveSpeed.
		*/

		Vector2 pos = transform.position;
		Vector2 direction = (target - pos).normalized;
		if (Vector2.Distance (pos, target) > transform.localScale.x / 2) {
			//Will stop moving towards position if position is within one radius of the entity's position
			transform.position = (pos + direction * transform.localScale.x*0.02f * moveSpeed);
		}
	}

	void OnTriggerEnter2D(Collider2D col) {
		if (col.gameObject.tag != this.tag) {
			touchingOther = true;
			otherEntity = col.transform;
		}
	}


}
