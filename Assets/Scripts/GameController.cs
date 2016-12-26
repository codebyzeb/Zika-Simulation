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

	//Simulation variables

	Camera camera;
	public float gameSpeed, savedGameSpeed, maxGameSpeed;

	float globalTimer;
	float dayLength;

	bool paused;
	bool isActive;

	public GameObject humanPrefab, mosquitoPrefab;

	//Simulation data

	float hNum, hTransferChance, hRecoveryRate;
	float hMoveSpeed = 5;
	float hDensity;
	float mNum, mTransferChance, mRecoveryRate, mBiteRate;
	float mMoveSpeed = 20;

	//Equation data

	float SIM_hInfected;
	float SIM_mInfected;
	float SIR_hInfected;
	float SIR_mInfected;
	static bool firstRun = true;

	//UI elements

	public Text speedText, pauseText;
	UIManager UI;
	public GameObject equationPanel, menuPanel;
	public Text equationsPanelButtonText, equationText, simulationText, errorText;

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

		/*
		 * UI Initialisation
		 */

		UI = GetComponent<UIManager> ();

	}
	
	// Update is called once per frame
	void Update () {

		/*
		 * If the simulation is active, the MainSimulation() function is called,
		 * running the simulation. The Control Panel is also updated, as is the 
		 * Camera to allow zooming and panning.
		 */

		if (isActive) {
			MainSimulation ();
			ControlPanel ();
			UI.UpdateCamera ();
		}
	}

	void MainSimulation () {

		/*
		 * The function loops through every human and mosquito to call the Live function
		 * attached to each Entity.
		 * There is also a group of periodic "Daily Actions" that occur according to
		 * a globalTimer variable keeps track of the time. Once it passes a certain
		 * threshold (dayLength) it operates the daily actions.
		 * 
		 * Daily Actions:
		 * 	- Loop through all mosquitos and humans to start their personal daily actions
		 * 	- Count all of the infected mosquitos and humans to calculate the percentage infected for each
		 * 	- Reset the global timer
		 *	
		 */ 

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

			globalTimer = 0;
		}

		foreach (Human human in humans) {
			human.Live (gameSpeed);
		}
		foreach (Mosquito mosquito in mosquitos) {
			mosquito.Live (gameSpeed);
		}
	}

	void UpdateEquations () {

		/*
		 * Updates the SIR data, using the equations detailed in the Analysis 1.1.5 and Design 2.2.3
		 * Only updates after the first run to ensure the equations are kept inline with the simulation.
		 */

		if (!firstRun) {
			SIR_hInfected += (mNum / hNum) * mBiteRate * hTransferChance * SIR_mInfected * (1 - SIR_hInfected) - hRecoveryRate * SIR_hInfected;
			SIR_mInfected += mBiteRate * mTransferChance * SIR_hInfected * (1 - SIR_mInfected) - mRecoveryRate * SIR_mInfected;
		} else {
			firstRun = false;
		}
	}

	void UpdateEquationsText () {

		/*
		 * Part of the main simultion interface, updates the text in the equations comparison panel to
		 * display the percentage infected mosquitos and humans as calculated by the equations and the 
		 * percentage difference between the two.
		 */

		equationText.text = "Percentage Infected Humans: " + (Mathf.Round (SIR_hInfected * 1000) / 10).ToString () + "%\nPercentage Infected Mosquitos: " + (Mathf.Round (SIR_mInfected * 1000) / 10).ToString () + "%";
		simulationText.text = "Percentage Infected Humans: " + (Mathf.Round (SIM_hInfected * 1000) / 10).ToString () + "%\nPercentage Infected Mosquitos: " + (Mathf.Round (SIM_mInfected * 1000) / 10).ToString () + "%";
		errorText.text = "Percentage Difference Humans: " + Mathf.Abs (Mathf.Round ((SIM_hInfected - SIR_hInfected) * 1000) / 10).ToString () + "%\nPercentage Difference Mosquitos: " + Mathf.Abs (Mathf.Round ((SIM_mInfected - SIR_mInfected) * 1000) / 10).ToString () + "%";
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

		//Sets the activity of the simulation to True or False

		isActive = active;
	}

	public void StartSimulation () {

		/*
		 * Function that initialises and begins the simulation.
		 * Processes:
		 * 	1) Clears all entities currently in operation.
		 * 	2) Sets the parameters of the simulation according to the parameteres
		 * 		chosen by the user.
		 * 	3) Initialises the SIR equation parameters
		 * 	4) Camera is initialised using a calculated spawnRadius parameter based
		 * 		on the number of mosquitos and humans and the density parameter.
		 * 	5) Loops through hNum to create each human and place them in the 2D world space.
		 * 	6) Loops through mNum to create each mosquito and place them in the 2D world space.
		 * 	7) The simulation is set to active.
		 */


		//1
		ClearEntities ();

		//2
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

		//3
		SIR_hInfected = hInfectedStart;
		SIR_mInfected = mInfectedStart;

		//4
		float spawnRadius = humanPrefab.transform.localScale.x * 4 * Mathf.Sqrt (hNum+mNum) / hDensity;
		UI.InitialiseCamera (50, (int)spawnRadius * 2, (int)spawnRadius);

		//5
		for (int i = 0; i < hNum; i++) {
			bool infected = (float)(i+1)/hNum <= hInfectedStart ? true : false;

			GameObject temp = (GameObject)Instantiate (humanPrefab);
			Vector2 circle = Random.insideUnitCircle * spawnRadius;
			temp.transform.localPosition = new Vector3 (circle.x, circle.y, 0);

			Human human = new Human (temp.transform, infected, hRecoveryRate, hTransferChance, hMoveSpeed);

			humans.Add (human);
		}

		//6
		for (int i = 0; i < mNum; i++) {
			bool infected = (float)(i+1)/mNum <= mInfectedStart ? true : false;

			GameObject temp = (GameObject)Instantiate (mosquitoPrefab);
			Vector2 circle = Random.insideUnitCircle * spawnRadius;
			temp.transform.localPosition = new Vector3 (circle.x, circle.y, 0);

			Mosquito mosquito = new Mosquito (temp.transform, infected, mRecoveryRate, mTransferChance, mMoveSpeed, mBiteRate);

			mosquitos.Add (mosquito);
		}

		//7
		SetActivity (true);

	}

	void ClearEntities () {

		/*
		 * Deletes all entities currently in use.
		 */

		foreach (Human human in humans) {
			Destroy (human.transform.gameObject);
		}
		humans.Clear();
		foreach (Mosquito mosquito in mosquitos) {
			Destroy (mosquito.transform.gameObject);
		}
		mosquitos.Clear ();
	}
		
	public void ToggleEquationsPanel () {

		/*
		 * Toggles the equations panel by setting it to active
		 * or inactive and changing the text on the button.
		 */

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

		/*
		 * Changes the text of the center two control panel buttons.
		 * Also toggles the menu panel if the escape key is pressed.
		 */

		speedText.text = gameSpeed.ToString() + "x";
		pauseText.text = (paused) ? "Play" : "Pause";
		if (Input.GetKeyDown ("escape")) {
			ToggleMenuPanel ();
		}
	}

	public void ToggleMenuPanel () {

		//Toggles the activity of the pause menu panel

		menuPanel.SetActive (!menuPanel.activeSelf);
	}

	public void Quit() {

		//Quits the application

		Application.Quit ();
	}

}
