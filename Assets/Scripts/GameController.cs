using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

[System.Serializable]
public class Entity {
	private static int tempID;

	public int ID;
	public int age;
	public Transform transform;
	public Movement movement;
	public Infection infection;
	SpriteRenderer sprite;

	public void Initialise()
	{
		ID = tempID;
		tempID++;
		age = 0;
		movement = transform.gameObject.GetComponent<Movement> ();
		movement.Initialise (this);
		infection = transform.gameObject.GetComponent<Infection> ();
		infection.Initialise (1, 0, 0);
		sprite = transform.gameObject.GetComponent<SpriteRenderer> ();
	}

	public void IncreaseAge()
	{
		age++;
	}

	public void Reproduce()
	{

	}

	public void Live(float gameSpeed) {
		movement.RandomMovement (gameSpeed);
		ManageGraphics ();
	}

	public void ManageGraphics() {
		sprite.color = (infection.infected) ? Color.red : Color.white;
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
	}
		
	new public void Live(float gameSpeed) {

		if (bitePhase == 1) {
			movement.MoveToClosestEntity ("Human", gameSpeed);
			if (movement.Attached ()) {
				bitePhase = 2;
				delayTimer = 0;
			}
		} else if (bitePhase == 2) {
			delayTimer += Time.deltaTime*gameSpeed;
			if (delayTimer > biteLength) {
				bitePhase = 0;
				infection.TransmitInfection (movement.getTouchingEntity ());
				movement.Detach ();
				delayTimer = 0;
			}
		} else {
			movement.RandomMovement (gameSpeed);
		}

		ManageGraphics ();

	}
}


public class GameController : MonoBehaviour {

	//Objects managed by the simulation:

	public List<Human> humans;
	public List<Mosquito> mosquitos;

	public List<Transform> humansTemp;
	public List<Transform> mosquitosTemp;

	//Variables declareed

	Camera camera;
	public float gameSpeed, savedGameSpeed;

	float globalTimer;
	float dayLength;

	bool paused;

	//UI elements

	public Button pauseButton, fasterButton, slowerButton, speedButton;
	public Text speedText, pauseText;

	// Initialisation of simulation
	void Start () {

		/*
		 * Timer reset and dayLength set to 1 second.
		 */ 

		camera = GetComponent<Camera> ();
		gameSpeed = 1;
		savedGameSpeed = 1;
		paused = false;

		globalTimer = 0;
		dayLength = 1;

		/*
		 * Each transform in humanTemp initialised as a Human object
		 * and added to the list humans, then
		 * each transform in mosquitoTemp intialised as a Mosquito object
		 * and added to the list mosquitos.
		 */

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

		/*
		 * UI Initialisation
		 */

		speedText = speedButton.GetComponentInChildren<Text> ();
		pauseText = pauseButton.GetComponentInChildren<Text> ();

	}
	
	// Update is called once per frame
	void Update () {
		Test2 ();
		ControlPanel ();
	}

	void ControlPanel() {

		//Sets text of centre two buttons

		speedText.text = gameSpeed.ToString() + "x";
		pauseText.text = (paused) ? "Play" : "Pause";
	}

	public void IncreaseGameSpeed (float amount) {

		//Increases gameSpeed by amount

		gameSpeed += amount;

		if (gameSpeed < 0) {
			gameSpeed = 0;
		}
	}

	public void TogglePause () {
		
		/*
		 * When pausing, saves current gameSpeed, sets gameSpeed to 0 and sets paused to true.
		 * When playing, sets current gameSpeed to savedGameSpeed and sets paused to true.
		 */ 

		if (paused) {
			gameSpeed = savedGameSpeed;
			paused = false;
		}
		else {
			savedGameSpeed = gameSpeed;
			gameSpeed = 0;
			paused = true;
		}
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
		globalTimer += Time.deltaTime*gameSpeed;
		if (globalTimer > dayLength) {
			foreach (Mosquito mosquito in mosquitos) {
				mosquito.decideBiting ();
			}
			globalTimer = 0;
		}

		foreach (Human human in humans) {
			human.Live (gameSpeed);
		}
		foreach (Mosquito mosquito in mosquitos) {
			mosquito.Live (gameSpeed);
		}

	}
		


}
