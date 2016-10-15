using UnityEngine;
using System.Collections;
using System.Collections.Generic;

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
	private Transform touchingEntity;

	private float updateClosestTimer;
	private Transform closestEntity;


	//initialisation function
	void Start () {
		updateClosestTimer = 0.2f;
		closestEntity = null;
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

	public void MoveToClosestEntity(string entityType) {

		/*
		 * Resets a timer every fixed time interval,
		 * then moves to closest entity.
		*/

		delayTimer += Time.deltaTime;
		if (delayTimer > updateClosestTimer) {
			delayTimer = 0;
			closestEntity = FindNearestEntity (entityType);
		}
		MoveToEntity (closestEntity);
	}

	public Transform FindNearestEntity(string entityType)
	{

		/*
		 * Function to find the nearest entity of type entityType.
		 * First checks if there are any actual entities of entityType,
		 * then operates main algorithm to find closest,
		 * then operates backup algorithm to find closest of those found.
		*/

		int totalOtherEntities = GameObject.FindGameObjectsWithTag (entityType).Length;
		if (totalOtherEntities == 0) {
			//returns null if there are no entities to find.
			return null;
		}

		List<Collider2D> nearbyColliders = new List<Collider2D>();
		//starts with a small radius for the search
		float radius = 2 * transform.localScale.x;
		while (nearbyColliders.Count == 0) {
			//Uses built-in unity function to find all colliders within a certain radius of the local position.
			Collider2D[] nearbyCollidersTemp = Physics2D.OverlapCircleAll (transform.position, radius);
			foreach (Collider2D collider in nearbyCollidersTemp) {
 				if (collider.tag == entityType) {
					//adds collider to the list if it is of type entityType
					nearbyColliders.Add (collider);
				}
			}
			if (radius > transform.localScale.x * 100) {
				//error check if none found within 100 diameters
				print ("Error: none found");
				break;
			}
			radius += 2 * transform.localScale.x;
		}

		if (nearbyColliders.Count == 1) {
			//If only one found, returns it.
			return nearbyColliders [0].transform;
		} else if (nearbyColliders.Count > 1) {
			//If many found, use secondary algorithm to find closest of those.
			return FindClosestOfList (nearbyColliders);
		} else {
			//If none found, return null (this shouldn't be triggered)
			return null;
		}
		
	}

	Transform FindClosestOfList(List<Collider2D> colliders)
	{

		/*
		 * Secondary algorith to find closest entity.
		 * Sets smallest distance to infinity and closestTransform to null.
		 * Then iterates through each collider in the list, finds the distance from this position
		 * and sets closestTransform to that collider's transform if it is the closest.
		*/

		float dist = Mathf.Infinity;
		Transform closestTransform = null;

		foreach (Collider2D collider in colliders) {
			float tempDist = Vector2.Distance (transform.position, collider.transform.position);
			if (tempDist < dist) {
				dist = tempDist;
				closestTransform = collider.transform;
			}
		}

		return closestTransform;
	}

	public void MoveToEntity(Transform target)
	{

		/*
		 * If not touching another entity, move towards the entity.
		 * Else, set the local transform's parent to be the object that 
		 * it is touching; this binds the location to the location of the other object.
		*/
		if (target != null) {
			if (!touchingOther) {
				MoveToPosition (target.transform.localPosition);
			} else if (transform.parent != touchingEntity) {
				transform.SetParent (touchingEntity);
			}
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
		
		/*
		 * This is a built in function called by Unity when another object
		 * enters this object's collider, which has been set to a circle around
		 * the object's sprite.
		 * I have set this function to set touchingOther to true and set the pointer
		 * touchingEntity to the transform of the other object.
		*/
		if (col.gameObject.tag != this.tag && !touchingOther) {
			touchingOther = true;
			touchingEntity = col.transform;
		}
	}


}
