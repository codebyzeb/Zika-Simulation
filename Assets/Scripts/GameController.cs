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
		age = 7;
		movement = transform.gameObject.GetComponent<Movement> ();
		infection = transform.gameObject.GetComponent<Infection> ();
	}

	public void IncreaseAge()
	{

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

}


public class GameController : MonoBehaviour {

	Camera camera;

	public Human human;
	public Mosquito mosquito;

	// Use this for initialization
	void Start () {
		camera = GetComponent<Camera> ();
		human.Initialise ();
		mosquito.Initialise ();
	}
	
	// Update is called once per frame
	void Update () {
		Vector3 mousePos = camera.ScreenToWorldPoint (Input.mousePosition);
		human.transform.position = new Vector2(mousePos.x, mousePos.y);
		mosquito.movement.MoveToEntity (human);
	}
}
