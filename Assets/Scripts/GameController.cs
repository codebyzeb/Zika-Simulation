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
	public float gameSpeed, savedGameSpeed, maxGameSpeed;

	float globalTimer;
	float dayLength;

	bool paused;
	bool isActive;

	public GameObject humanPrefab, mosquitoPrefab;

	//Simulation data

	int hNum;
	float hTransferChance, hRecoveryRate;
	float hMoveSpeed = 5;
	float hDensity;
	int mNum;
	float mTransferChance, mRecoveryRate, mBiteRate;
	float mMoveSpeed = 20;

	//Equation data

	float SIM_hInfected;
	float SIM_mInfected;
	float SIR_hInfected;
	float SIR_mInfected;

	//UI elements

	public Button pauseButton, fasterButton, slowerButton, speedButton;
	public Text speedText, pauseText;
	UIManager UI;
	public InputManager inputMenu;
	public GameObject equationPanel;
	public Text equationsPanelButtonText;
	public Text equationText;
	public Text simulationText;
	public Text errorText;

	// Initialisation of simulation
	void Start () {

		/*
		 * Timer reset and dayLength set to 1 second.
		 */ 

		camera = GetComponent<Camera> ();
		gameSpeed = 1;
		savedGameSpeed = 1;
		maxGameSpeed = 5;
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
			UpdateEquations ();

			float numInfectedMosquitos = 0;
			float numInfectedHumans = 0;
			foreach (Human human in humans) {
				human.startDailyActionsTimer (dayLength);
				if (human.infection.infected) {
					numInfectedHumans++;
				}
			}
			foreach (Mosquito mosquito in mosquitos) {
				mosquito.startDailyActionsTimer (dayLength);
				if (mosquito.infection.infected) {
					numInfectedMosquitos++;
				}
			}
			SIM_hInfected = numInfectedHumans / hNum;
			SIM_mInfected = numInfectedMosquitos / mNum;

			equationText.text = "Percentage Infected Humans: " + (Mathf.Round (SIR_hInfected * 1000) / 10).ToString () + "%\nPercentage Infected Mosquitos: " + (Mathf.Round (SIR_mInfected * 1000) / 10).ToString () + "%";
			simulationText.text = "Percentage Infected Humans: " + (Mathf.Round (SIM_hInfected * 1000) / 10).ToString () + "%\nPercentage Infected Mosquitos: " + (Mathf.Round (SIM_mInfected * 1000) / 10).ToString () + "%";
			errorText.text = "Percentage Difference Humans: " + Mathf.Abs (Mathf.Round ((SIM_hInfected - SIR_hInfected) * 1000) / 10).ToString () + "%\nPercentage Difference Mosquitos: " + Mathf.Abs (Mathf.Round ((SIM_mInfected - SIR_mInfected) * 1000) / 10).ToString () + "%";

			globalTimer = 0;
		}

		foreach (Human human in humans) {
			human.Live (gameSpeed);
		}
		foreach (Mosquito mosquito in mosquitos) {
			mosquito.Live (gameSpeed);
		}
	}

	static bool firstRun = true;
	void UpdateEquations () {
		if (!firstRun) {
			SIR_hInfected += (mNum / hNum) * mBiteRate * hTransferChance * SIR_mInfected * (1 - SIR_hInfected) - hRecoveryRate * SIR_hInfected;
			SIR_mInfected += mBiteRate * mTransferChance * SIR_hInfected * (1 - SIR_mInfected) - mRecoveryRate * SIR_mInfected;
		} else {
			firstRun = false;
		}
	}

	public void ToggleEquationsPanel () {
		if (equationPanel.activeSelf) {
			equationPanel.SetActive (false);
			equationsPanelButtonText.text = "Show Comparison"; 
		}
		else {
			equationPanel.SetActive (true);
			equationsPanelButtonText.text = "Hide Comparison"; 
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

		//Clamp gameSpeed to be between 0 and maxGameSpeed
		if (gameSpeed < 0) {
			gameSpeed = 0;
		} else if (gameSpeed > maxGameSpeed) {
			gameSpeed = maxGameSpeed;
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

		hNum = (int)Preset.current.humanData ["Population Size"];
		float hInfectedStart = Preset.current.humanData ["Percentage Infected"];
		hRecoveryRate = Preset.current.humanData ["Percentage Recovery"];
		hTransferChance = Preset.current.humanData ["Transfer Chance"];
		hDensity = Preset.current.humanData ["Density"];

		mNum = (int)Preset.current.mosquitoData ["Population Size"];
		float mInfectedStart = Preset.current.mosquitoData ["Percentage Infected"];
		mRecoveryRate = Preset.current.mosquitoData ["Percentage Recovery"];
		mTransferChance = Preset.current.mosquitoData ["Transfer Chance"];
		mBiteRate = Preset.current.mosquitoData ["Bite Rate"];

		SIR_hInfected = hInfectedStart;
		SIR_mInfected = mInfectedStart;

		float spawnRadius = humanPrefab.transform.localScale.x * 4 * Mathf.Sqrt (hNum+mNum) / hDensity;
		UI.InitialiseCamera (50, (int)spawnRadius * 2, (int)spawnRadius);

		for (int i = 0; i < hNum; i++) {
			bool infected = (float)(i+1)/hNum <= hInfectedStart ? true : false;

			GameObject temp = (GameObject)Instantiate (humanPrefab);
			Vector2 circle = Random.insideUnitCircle * spawnRadius;
			temp.transform.localPosition = new Vector3 (circle.x, circle.y, 0);

			Human human = new Human (temp.transform, infected, hRecoveryRate, hTransferChance, hMoveSpeed);

			humans.Add (human);
		}

		for (int i = 0; i < mNum; i++) {
			bool infected = (float)(i+1)/mNum <= mInfectedStart ? true : false;

			GameObject temp = (GameObject)Instantiate (mosquitoPrefab);
			Vector2 circle = Random.insideUnitCircle * spawnRadius;
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
