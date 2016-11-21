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

	public Entity(Transform transformTemp, bool infected, float recoveryRate, float transferChance, float moveSpeed)
	{
		transform = transformTemp;

		//Creates an ID for the entity
		ID = tempID;
		tempID++;

		//Sets up movement composite object
		movement = transform.gameObject.GetComponent<Movement> ();
		movement.Initialise (this, moveSpeed);

		//Sets up infection composite object
		infection = transform.gameObject.GetComponent<Infection> ();
		infection.Initialise (transferChance, recoveryRate, infected);
		infection.ID = ID;

		sprite = transform.gameObject.GetComponent<SpriteRenderer> ();
	}

	public Entity (Transform transformTemp) {
		
		transform = transformTemp;

		//Creates an ID for the entity
		ID = tempID;
		tempID++;

		//Sets up movement composite object
		movement = transform.gameObject.GetComponent<Movement> ();
		movement.Initialise (this, 0);

		//Sets up infection composite object
		infection = transform.gameObject.GetComponent<Infection> ();
		infection.Initialise (1, 0, false);
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

	public Human (Transform transformTemp, bool infected, float recoveryRate, float transferChance, float moveSpeed) : base (transformTemp, infected, recoveryRate, transferChance, moveSpeed) { 
		
	}

	public Human (Transform transformTemp) : base (transformTemp) {
		
	}

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

	public Mosquito (Transform transformTemp, bool infected, float recoveryRate, float transferChance, float moveSpeed, float biteRateTemp) : base(transformTemp, infected, recoveryRate, transferChance, moveSpeed) {
		biteLength = 0.5f;
		biteRate = biteRateTemp;
	}

	public Mosquito (Transform transformTemp) : base (transformTemp) {
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

	//Simulation variables

	Camera camera;
	public float gameSpeed, savedGameSpeed;

	float globalTimer;
	float dayLength;

	bool paused;
	bool isActive;

	public GameObject humanPrefab, mosquitoPrefab;

	//Simulation data

	int hNum;
	float hTransferChance, hInfected, hRecoveryRate;
	float hMoveSpeed = 5;
	int mNum;
	float mTransferChance, mInfected, mRecoveryRate, mBiteRate;
	float mMoveSpeed = 20;

	//UI elements

	public Button pauseButton, fasterButton, slowerButton, speedButton;
	public Text speedText, pauseText;
	UIManager UI;
	public InputManager inputMenu;

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

		OldInitialisation ();

		/*
		 * UI Initialisation
		 */

		speedText = speedButton.GetComponentInChildren<Text> ();
		pauseText = pauseButton.GetComponentInChildren<Text> ();
		UI = GetComponent<UIManager> ();

	}

	void OldInitialisation () {

		/*
		 * Each transform in humanTemp initialised as a Human object
		 * and added to the list humans, then
		 * each transform in mosquitoTemp intialised as a Mosquito object
		 * and added to the list mosquitos.
		 */
		
		foreach (Transform humanTemp in humansTemp) {
			Human human = new Human (humanTemp);
			humans.Add (human);
		}
		foreach (Transform mosquitoTemp in mosquitosTemp) {
			Mosquito mosquito = new Mosquito (mosquitoTemp);
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

	public void StartSimulation () {
		ClearEntities ();

		hNum = (int)inputMenu.humansInput ["Population Size"];
		float hInfectedStart = inputMenu.humansInput ["Percentage Infected"];
		hRecoveryRate = inputMenu.humansInput ["Percentage Recovery"];
		hTransferChance = inputMenu.humansInput ["Transfer Chance"];

		mNum = (int)inputMenu.mosquitosInput ["Population Size"];
		float mInfectedStart = inputMenu.mosquitosInput ["Percentage Infected"];
		mRecoveryRate = inputMenu.mosquitosInput ["Percentage Recovery"];
		mTransferChance = inputMenu.mosquitosInput ["Transfer Chance"];
		mBiteRate = inputMenu.mosquitosInput ["Bite Rate"];

		float density = 1;
		print (humanPrefab.transform.localScale.x);
		float spawnSquare = humanPrefab.transform.localScale.x * 4 * Mathf.Sqrt (mNum) / density;
		UI.InitialiseCamera (50, spawnSquare + 50, spawnSquare/2 + 50);

		for (int i = 0; i < hNum; i++) {
			bool infected = (float)i/hNum <= hInfectedStart ? true : false;

			GameObject temp = (GameObject)Instantiate (humanPrefab);
			Vector2 circle = Random.insideUnitCircle * spawnSquare;
			temp.transform.localPosition = new Vector3 (circle.x, circle.y, 0);

			Human human = new Human (temp.transform, infected, hRecoveryRate, hTransferChance, hMoveSpeed);

			humans.Add (human);
		}

		for (int i = 0; i < mNum; i++) {
			bool infected = (float)i/mNum < mInfectedStart ? true : false;

			GameObject temp = (GameObject)Instantiate (mosquitoPrefab);
			Vector2 circle = Random.insideUnitCircle * spawnSquare;
			temp.transform.localPosition = new Vector3 (circle.x, circle.y, 0);

			Mosquito mosquito = new Mosquito (temp.transform, infected, mRecoveryRate, mTransferChance, mMoveSpeed, mBiteRate);

			mosquitos.Add (mosquito);
		}

		isActive = true;

	}

	void ClearEntities () {
		foreach (Human human in humans) {
			Destroy (human.transform.gameObject);
		}
		humans.Clear();
		foreach (Mosquito mosquito in mosquitos) {
			Destroy (mosquito.transform.gameObject);
		}
		mosquitos.Clear ();
	}

}
