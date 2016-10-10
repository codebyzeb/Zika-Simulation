using UnityEngine;
using System.Collections;

[System.Serializable]
public class Entity {
	public int ID { get; set; }
	public int age { get; private set;}
	public Transform transform;
	public Movement movement;
	public Infection infection;

	public void IncreaseAge()
	{

	}

	public void Reproduce()
	{

	}

	public void OnCollisionEnter (Collision col)
	{
		
	}

	public void OnCollisionExit (Collision col)
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

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
