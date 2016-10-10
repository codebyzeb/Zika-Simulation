using UnityEngine;
using System.Collections;

public class Movement : MonoBehaviour {

	float moveSpeed;
	Vector2 direction;


	//Variables used for RandomMovement()
	private float delayTimer;
	private float randomDelay;
	private int noiseAction;
	private Vector2 noisePos;

	//initialisation function
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	void RandomMovement()
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
			randomDelay = (noiseAction == 0) ? Random.Range (0.5f, 3) : Random.Range (0.5f, 5);
			noisePos = new Vector2 (Random.Range (transform.position.x + transform.localScale.x * 5, transform.position.x + transform.localScale.x * 5), Random.Range (transform.position.y + transform.localScale.y * 5, transform.position.y + transform.localScale.y * 5));
		}
		else if (noiseAction == 1) {
			MoveToPosition (noisePos);
		}
	}

	Entity FindNearestEntity()
	{
		return null;
	}

	void MoveToEntity(Entity target)
	{

	}

	void MoveToPosition(Vector2 target)
	{

		/*
		 * Direction calculated by normalising the difference between
		 * the two positions, local position and target position.
		 * Then local position is changed according to moveSpeed.
		*/

		Vector2 pos = transform.localPosition;
		Vector2 direction = (target - pos).normalized;
		transform.position = (pos + direction * transform.localScale.x*0.02f * moveSpeed);
	}

}
