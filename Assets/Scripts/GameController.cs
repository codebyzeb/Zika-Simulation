using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

[System.Serializable]
public class Entity {
	private static int tempID;

	public int ID;
	public Transform transform;
	public Movement movement;
	public Infection infection;
	SpriteRenderer sprite;

	protected bool startOfDay;
	protected float delayTimer;

	public void Initialise()
	{
		ID = tempID;
		tempID++;
		movement = transform.gameObject.GetComponent<Movement> ();
		movement.Initialise (this);
		infection = transform.gameObject.GetComponent<Infection> ();
		infection.Initialise (1, 0.05f);
		infection.ID = ID;
		sprite = transform.gameObject.GetComponent<SpriteRenderer> ();
	}
		
	public void Live(float gameSpeed) {

		if (startOfDay) {
			delayTimer += Time.deltaTime*gameSpeed;
			if (delayTimer >= 0) {
				doDailyActions ();
				startOfDay = false;
			}
		}

		ManageGraphics ();
	}

	public void startDailyActionsTimer(float dayLength) {
		startOfDay = true;
		delayTimer = -(Random.Range (0, 0.5f * dayLength));
	}

	protected void doDailyActions () {
		infection.tryRecovery ();
	}

	public void ManageGraphics() {
		sprite.color = (infection.infected) ? Color.red : Color.white;
	}

}

[System.Serializable]
public class Human : Entity {

	new public void Live(float gameSpeed) {
		base.Live (gameSpeed);
		movement.RandomMovement (gameSpeed);
	}

}

[System.Serializable]
public class Mosquito : Entity {

	public float biteRate;
	private int bitePhase;

	public float biteLength;

	new public void Initialise() {
		base.Initialise ();
		biteLength = 0.5f;
		biteRate = 0.6f;
	}

	public void decideBiting() {
		if (bitePhase == 0) {
			float randNum = Random.value;
			if (randNum <= biteRate) {
				bitePhase = 1;
				movement.Detach ();
			}
		}
	}
		
	new public void Live(float gameSpeed) {

		if (startOfDay) {
			delayTimer += Time.deltaTime*gameSpeed;
			if (delayTimer >= 0) {
				doDailyActions ();
				decideBiting ();
				startOfDay = false;
			}
		}

		if (bitePhase == 1) {
			movement.MoveToClosestEntity ("Human", gameSpeed);
			if (movement.Attached ()) {
				bitePhase = 2;
				delayTimer = 0;
				Debug.Log (ID + " now touching " + movement.getTouchingEntity ().ID);
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

	//Variables declared

	Camera camera;
	public float gameSpeed, savedGameSpeed;

	float globalTimer;
	float dayLength;

	bool paused;
	bool isActive;

	//UI elements

	public Button pauseButton, fasterButton, slowerButton, speedButton;
	public Text speedText, pauseText;
	public UIManager UI;

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
		dayLength = 5;

		//Initialising Simulation

		OldInitialisation ();

		/*
		 * UI Initialisation
		 */

		speedText = speedButton.GetComponentInChildren<Text> ();
		pauseText = pauseButton.GetComponentInChildren<Text> ();
		UI = GetComponent<UIManager> ();
		UI.InitialiseCamera ();

	}

	public void Initialise () {

	}

	void OldInitialisation () {

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

	}
	
	// Update is called once per frame
	void Update () {
		if (isActive) {
			MainSimulation ();
			ControlPanel ();
			UI.UpdateCamera ();
		}
	}

	void MainSimulation () {
		globalTimer += Time.deltaTime*gameSpeed;
		if (globalTimer > dayLength) {
			foreach (Mosquito mosquito in mosquitos) {
				mosquito.startDailyActionsTimer (dayLength);
			}
			foreach (Human human in humans) {
				human.startDailyActionsTimer (dayLength);
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

	public void SetActivity (bool active) {
		isActive = active;
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
