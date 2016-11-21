using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class InputManager : MonoBehaviour {

	public Dictionary<string, float> humansInput = new Dictionary<string, float>();
	Dictionary<string, Slider> humansInputSliders = new Dictionary<string, Slider>();
	public InputField humansNum;
	public Slider HPercentageInfected, HRecovery, HTransfer;

	public Dictionary<string, float> mosquitosInput = new Dictionary<string, float>();
	Dictionary<string, Slider> mosquitosInputSliders = new Dictionary<string, Slider>();
	public InputField mosquitoNum;
	public Slider MPercentageInfected, MRecovery, MTransfer, MBiteRate;

	// Use this for initialization
	void Start () {

		humansInput.Add ("Population Size", 0);
		humansNum.onEndEdit.AddListener (delegate {
			UpdateNumHuman ();
		});
		humansInputSliders.Add ("Percentage Infected", HPercentageInfected);
		humansInputSliders.Add ("Percentage Recovery", HRecovery);
		humansInputSliders.Add ("Transfer Chance", HTransfer);
		foreach (string key in humansInputSliders.Keys) {
			humansInput.Add (key, 0);
		}

		mosquitosInput.Add ("Population Size", 0);
		mosquitoNum.onEndEdit.AddListener (delegate {
			UpdateNumMosquito ();
		});
		mosquitosInputSliders.Add ("Percentage Infected", MPercentageInfected);
		mosquitosInputSliders.Add ("Percentage Recovery", MRecovery);
		mosquitosInputSliders.Add ("Transfer Chance", MTransfer);
		mosquitosInputSliders.Add ("Bite Rate", MBiteRate);
		foreach (string key in mosquitosInputSliders.Keys) {
			mosquitosInput.Add (key, 0);
		}

			
	}
	
	// Update is called once per frame
	void Update () {
		foreach (string key in humansInputSliders.Keys) {
			humansInput[key] = humansInputSliders [key].value;
		}
			
		foreach (string key in mosquitosInputSliders.Keys) {
			mosquitosInput[key] = mosquitosInputSliders [key].value;
		}
	}

	void UpdateNumHuman () {
		humansInput ["Population Size"] = int.Parse(humansNum.textComponent.text);
	}

	void UpdateNumMosquito () {
		mosquitosInput ["Population Size"] = int.Parse(mosquitoNum.textComponent.text);
	}

	public void PrintAllValues () {
		print ("\nHumans: ");
		foreach (string key in humansInput.Keys) {
			print(key + ": " + humansInput[key]);
		}
		print ("\nMosquitos: ");
		foreach (string key in mosquitosInput.Keys) {
			print(key + ": " + mosquitosInput[key]);
		}
	}
}
