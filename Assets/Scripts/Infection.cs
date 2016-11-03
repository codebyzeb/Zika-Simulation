using UnityEngine;
using System.Collections;

public class Infection : MonoBehaviour {

	public bool infected;
	float transmissionChance; //Chance that this entity gets infected during transfer
	float recoveryChance;
	float deathChance;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	public void Initialise (float tChance, float rChance, float dChance)
	{

		/*
		 * Initialises the infection script with parameters defined
		*/

		transmissionChance = tChance;
		recoveryChance = rChance;
		deathChance = dChance;
	}

	public void TransmitInfection(Entity other)
	{

		/*
		 * Transmits the infection between two entities, depending on which is infected.
		 * If the other entity is infected, this entity might get infected, otherwise
		 * the other entity might be infected. If both or neither are infected, nothing happens.
		*/

		if (other.infection.infected && !infected) {
			Infect ();
		}
		else if (!other.infection.infected && infected) {
			other.infection.Infect ();
		}
	}

	public void Infect()
	{

		/*
		 * If not infected, creates a random number between 0 and 1.
		 * If the number is less than transmissionChance, then the virus is transmitted
		 * and infected is set to true.
		*/

		if (!infected) {
			float randNum = Random.value;
			if (randNum <= transmissionChance) {
				infected = true;
			}
		}
	}
		
	public bool tryRecovery()
	{

		/*
		 * If infected, creates a random number between 0 and 1.
		 * If the number is less than recoveryChance, then the entity recovers
		 * and infected is set to false.
		*/

		if (infected) {
			float randNum = Random.value;
			if (randNum <= recoveryChance) {
				infected = false;
				return true;
			} else {
				return false;
			}
		}
		return false;
	}

	public bool getFatality()
	{

		/*
		 * Creates a random number between 0 and 1.
		 * If the number is less than deathChance, 
		 * then returns true. UNUSED SO FAR, MUST RESEARCH
		*/

		float randNum = Random.value;
		if (randNum <= deathChance) {
			return true;
		} else {
			return false;
		}
	}
}
