using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class Entity {
	private static int tempID;

	public int ID;
	public int age;
	public Transform transform;
	public Movement movement;
	public Infection infection;

	public void Initialise()
	{
		ID = tempID;
		tempID++;
		age = 0;
		movement = transform.gameObject.GetComponent<Movement> ();
		movement.Initialise (this);
		infection = transform.gameObject.GetComponent<Infection> ();
	}

	public void IncreaseAge()
	{
		age++;
	}

	public void Reproduce()
	{

	}

	public void Live() {
		movement.RandomMovement ();
	}

}

[System.Serializable]
public class Human : Entity {

}

[System.Serializable]
public class Mosquito : Entity {

	public float biteRate;
	private int bitePhase;

	public float biteLength;
	private float delayTimer;

	new public void Initialise() {
		base.Initialise ();
		biteLength = 0.5f;
		biteRate = 0.1f;
	}

	public void decideBiting() {
		if (bitePhase == 0) {
			float randNum = Random.value;
			if (randNum <= biteRate) {
				bitePhase = 1;
			}
		}
		Debug.Log (bitePhase);
	}
		
	new public void Live() {

		if (bitePhase == 1) {
			movement.MoveToClosestEntity ("Human");
			if (movement.Attached ()) {
				bitePhase = 2;
				delayTimer = 0;
			}
		} else if (bitePhase == 2) {
			delayTimer += Time.deltaTime;
			if (delayTimer > biteLength) {
				bitePhase = 0;
				infection.TransmitInfection (movement.getTouchingEntity ());
				movement.Detach ();
				delayTimer = 0;
			}
		} else {
			movement.RandomMovement ();
		}
	}
}


public class GameController : MonoBehaviour {

	Camera camera;

	public List<Human> humans;
	public List<Mosquito> mosquitos;

	public List<Transform> humansTemp;
	public List<Transform> mosquitosTemp;

	float globalTimer;
	float dayLength;

	// Initialisation of simulation
	void Start () {
		globalTimer = 0;
		dayLength = 1;

		camera = GetComponent<Camera> ();
		foreach (Transform humanTemp in humansTemp) {
			Human human = new Human ();
			human.transform = humanTemp;
			human.Initialise ();
			humans.Add (human);
		}
		foreach (Transform mosquitoTemp in mosquitosTemp) {
			Mosquito mosquito = new Mosquito ();
			mosquito.transform = mosquitoTemp;
			mosquito.Initialise ();
			mosquitos.Add (mosquito);
		}
	}
	
	// Update is called once per frame
	void Update () {
		Test2 ();
	}

	void Test1 () {
		foreach (Human human in humans) {
			human.movement.RandomMovement ();
		}
		foreach (Mosquito mosquito in mosquitos) {
			mosquito.movement.MoveToClosestEntity ("Human");
		}
	}

	void Test2 () {
		globalTimer += Time.deltaTime;
		if (globalTimer > dayLength) {
			foreach (Mosquito mosquito in mosquitos) {
				mosquito.decideBiting ();
			}
			globalTimer = 0;
		}

		foreach (Human human in humans) {
			human.Live ();
		}
		foreach (Mosquito mosquito in mosquitos) {
			mosquito.Live ();
		}

	}
		


}
