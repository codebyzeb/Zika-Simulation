using UnityEngine;
using System.Collections;

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
		infection = transform.gameObject.GetComponent<Infection> ();
	}

	public void IncreaseAge()
	{
		age++;
	}

	public void Reproduce()
	{

	}

}

[System.Serializable]
public class Human : Entity {

}

[System.Serializable]
public class Mosquito : Entity {
	void Bite() {

	}
}


public class GameController : MonoBehaviour {

	Camera camera;

	public Human[] humans;
	public Mosquito[] mosquitos;

	// Use this for initialization
	void Start () {
		camera = GetComponent<Camera> ();
		foreach (Human human in humans) {
			human.Initialise ();
		}
		foreach (Mosquito mosquito in mosquitos) {
			mosquito.Initialise ();
		}
	}
	
	// Update is called once per frame
	void Update () {
		Test1 ();
	}

	void Test1 () {
		foreach (Human human in humans) {
			human.movement.RandomMovement ();
		}
		foreach (Mosquito mosquito in mosquitos) {
			mosquito.movement.MoveToClosestEntity ("Human");
		}
	}
		


}
