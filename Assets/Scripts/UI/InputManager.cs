using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

public static class SaveLoad {

	public static Dictionary<string, Preset> presets = new Dictionary<string, Preset> ();

	public static void Save(string name, Preset save) {
		presets.Add (name, save);
		BinaryFormatter bf = new BinaryFormatter ();
		FileStream file = File.Create (Application.persistentDataPath + "/savedPresets.gd");
		bf.Serialize (file, SaveLoad.presets);
		file.Close ();
	}

	public static void Delete(string name) {
		presets.Remove (name);
		BinaryFormatter bf = new BinaryFormatter ();
		FileStream file = File.Create (Application.persistentDataPath + "/savedPresets.gd");
		bf.Serialize (file, SaveLoad.presets);
		file.Close ();
	}

	public static void Load() {
		if (File.Exists (Application.persistentDataPath + "/savedPresets.gd")) {
			BinaryFormatter bf = new BinaryFormatter ();
			FileStream file = File.Open (Application.persistentDataPath + "/savedPresets.gd", FileMode.Open);
			SaveLoad.presets = (Dictionary<string, Preset>)bf.Deserialize (file);
			file.Close ();
		}

	}

}


[System.Serializable]
public class Preset {

	public static Preset current;
	public bool isSystemPreset;
	public Dictionary<string, float> humanData = new Dictionary<string, float>();
	public Dictionary<string, float> mosquitoData = new Dictionary<string, float>();

	public Preset() {

		humanData.Add ("Population Size", 0);
		humanData.Add ("Percentage Infected", 0);
		humanData.Add ("Percentage Recovery", 0);
		humanData.Add ("Transfer Chance", 0);
		humanData.Add ("Density", 0);

		mosquitoData.Add ("Population Size", 0);
		mosquitoData.Add ("Percentage Infected", 0);
		mosquitoData.Add ("Percentage Recovery", 0);
		mosquitoData.Add ("Transfer Chance", 0);
		mosquitoData.Add ("Bite Rate", 0);
	}

	public void AdjustData(bool human, string key, float value) {
		if (human) {
			humanData [key] = value;
		} else {
			mosquitoData [key] = value;
		}
	}

	public void PrintAllValues () {
		Debug.Log ("\nHumans: ");
		foreach (string key in humanData.Keys) {
			Debug.Log (key + ": " + humanData[key]);
		}
		Debug.Log ("\nMosquitos: ");
		foreach (string key in mosquitoData.Keys) {
			Debug.Log (key + ": " + mosquitoData[key]);
		}
	}

}

public class InputManager : MonoBehaviour {

	Dictionary<string, Slider> humansInputSliders = new Dictionary<string, Slider>();
	public InputField humansNum;
	public Slider HPercentageInfected, HRecovery, HTransfer, HDensity;

	Dictionary<string, Slider> mosquitosInputSliders = new Dictionary<string, Slider>();
	public InputField mosquitoNum;
	public Slider MPercentageInfected, MRecovery, MTransfer, MBiteRate;

	public Button startButton;
	public Text savePresetText;
	public Button savePreset;

	public Dropdown dropdown;
	public GameObject SavePanel;
	public InputField SaveField;
	public Button SavePresetName;

	bool customInputs;

	Preset customData;

	// Use this for initialization
	void Start () {
		SaveLoad.Load ();

		customInputs = true;
		customData = new Preset ();
		UpdateNumHuman ();
		UpdateNumMosquito ();
		Preset.current = customData;

		dropdown.ClearOptions ();
		dropdown.options.Add (new Dropdown.OptionData ("Custom"));
		dropdown.RefreshShownValue ();
		foreach (string key in SaveLoad.presets.Keys) {
			dropdown.options.Add (new Dropdown.OptionData (key));
		}
			
		InitialiseSliders ();

	}

	// Update is called once per frame
	void Update () {


		if (SavePanel.activeSelf) {
			if (SaveField.text == "" || SaveLoad.presets.ContainsKey (SaveField.text)) {
				SavePresetName.interactable = false;
			} else {
				SavePresetName.interactable = true;
			}
		}

		else if (customInputs) {
			foreach (string key in humansInputSliders.Keys) {
				customData.AdjustData (true, key, humansInputSliders [key].value);
			}
				
			foreach (string key in mosquitosInputSliders.Keys) {
				customData.AdjustData (false, key, mosquitosInputSliders [key].value);
			}

			Preset.current = customData;

			ValidateInput ();

		} 

	}

	void InitialiseSliders() {
		
		humansNum.onEndEdit.AddListener (delegate {
			UpdateNumHuman ();
		});
		humansInputSliders.Add ("Percentage Infected", HPercentageInfected);
		humansInputSliders.Add ("Percentage Recovery", HRecovery);
		humansInputSliders.Add ("Transfer Chance", HTransfer);
		humansInputSliders.Add ("Density", HDensity);

		mosquitoNum.onEndEdit.AddListener (delegate {
			UpdateNumMosquito ();
		});
		mosquitosInputSliders.Add ("Percentage Infected", MPercentageInfected);
		mosquitosInputSliders.Add ("Percentage Recovery", MRecovery);
		mosquitosInputSliders.Add ("Transfer Chance", MTransfer);
		mosquitosInputSliders.Add ("Bite Rate", MBiteRate);

	}
		
		
	void ValidateInput () {
		startButton.interactable = true;

		if (humansNum.text == "" || int.Parse(humansNum.text)==0) {
			startButton.interactable = false;
			humansNum.textComponent.color = Color.red;
		} else {
			humansNum.textComponent.color = Color.black;
		}

		if (mosquitoNum.text == "" || int.Parse(mosquitoNum.text)==0) {
			startButton.interactable = false;
			mosquitoNum.textComponent.color = Color.red;
		} else {
			mosquitoNum.textComponent.color = Color.black;
		}

	}

	public void DeleteSave() {
		if (savePresetText.text == "Save Preset") {
			SetAllInteractable (false);
			SavePanel.SetActive (true);
			dropdown.interactable = false;
			startButton.interactable = false;
			savePreset.interactable = false;

		} else {
			if (!Preset.current.isSystemPreset) {
				SaveLoad.Delete (dropdown.options [dropdown.value].text);
				dropdown.options.Remove (dropdown.options [dropdown.value]);
				dropdown.value = 0;
				customData = Preset.current;
				customInputs = true;
			}
		}
	}

	public void SaveCurrentPreset(bool cancel) {
		if (!cancel) {
			string presetName = SaveField.text;
			SaveLoad.Save (presetName, Preset.current);
			dropdown.options.Add (new Dropdown.OptionData (presetName));
			dropdown.value = dropdown.options.Count - 1;
			dropdown.RefreshShownValue ();
		}

		SetAllInteractable (true);
		SavePanel.SetActive (false);
		dropdown.interactable = true;
		startButton.interactable = true;
		savePreset.interactable = true;
	}

	public void ChangedPreset() {
		int selectedIndex = dropdown.value;
		if (selectedIndex == 0) {
			SetAllInteractable (true);
			customData = Preset.current;
			customInputs = true;
			savePreset.interactable = true;
			savePresetText.text = "Save Preset";
		} else {
			if (Preset.current.isSystemPreset) {
				savePreset.interactable = false;
			}
			else {
				savePreset.interactable = true;
			}
			SetAllInteractable (false);
			customInputs = false;
			string presetName = dropdown.options [selectedIndex].text;
			LoadPreset (presetName);
			Preset.current = SaveLoad.presets [presetName];
			savePresetText.text = "Delete Preset";
		}

	}

	void LoadPreset(string name) {
		Preset load = SaveLoad.presets [name];

		foreach (string key in load.humanData.Keys) {
			if (key != "Population Size") {
				humansInputSliders [key].value = load.humanData [key];
			}
		}

		foreach (string key in load.mosquitoData.Keys) {
			if (key != "Population Size") {
				mosquitosInputSliders [key].value = load.mosquitoData [key];
			}
		}

		humansNum.text = load.humanData ["Population Size"].ToString();
		mosquitoNum.text = load.mosquitoData ["Population Size"].ToString();
	}

	public void SetAllInteractable (bool val) {
		foreach (Slider slider in humansInputSliders.Values) {
			slider.interactable = val;
		}
		foreach (Slider slider in mosquitosInputSliders.Values) {
			slider.interactable = val;
		}
		humansNum.interactable = val;
		mosquitoNum.interactable = val;
	}

	void UpdateNumHuman () {
		customData.AdjustData (true, "Population Size", int.Parse (humansNum.textComponent.text));
	}

	void UpdateNumMosquito () {
		customData.AdjustData (false, "Population Size", int.Parse (mosquitoNum.textComponent.text));
	}

}
