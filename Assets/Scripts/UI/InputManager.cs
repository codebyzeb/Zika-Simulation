using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

public static class SaveLoad {

	/*
	 * A static class used to save and load data between application runs.
	 * The static keyword allows it to be accessible from anywhere in the program, and
	 * it only exists once to prevent confusion (no objects can be initialised from it).
	 */ 

	//Dictionary to hold all presets
	public static Dictionary<string, Preset> presets = new Dictionary<string, Preset> ();

	public static void Save(string name, Preset save) {

		/*
		 * Function to save a preset given a name; first adds the preset to the dictionary
		 * then creates a binary formatter to save the data to the file "savedPresets.gd".
		 * The file is then closed.
		 */ 

		presets.Add (name, save);
		BinaryFormatter bf = new BinaryFormatter ();
		FileStream file = File.Create (Application.persistentDataPath + "/savedPresets.gd");
		bf.Serialize (file, SaveLoad.presets);
		file.Close ();
	}

	public static void Delete(string name) {

		/*
		 * Function to remove a preset given a name; first removes the preset from the dictionary
		 * then creates a binary formatter to save the data to the file "savedPresets.gd".
		 * The file is then closed.
		 */ 

		presets.Remove (name);
		BinaryFormatter bf = new BinaryFormatter ();
		FileStream file = File.Create (Application.persistentDataPath + "/savedPresets.gd");
		bf.Serialize (file, SaveLoad.presets);
		file.Close ();
	}

	public static void Load() {

		/*
		 * Function to load all presets. First checks to see that the file exists, then  proceeds
		 * to create a binary formatter to load the data by opening the file and setting the presets
		 * variable to the deserialised file. 
		 * The file is then closed
		 */ 

		if (File.Exists (Application.persistentDataPath + "/savedPresets.gd")) {
			BinaryFormatter bf = new BinaryFormatter ();
			FileStream file = File.Open (Application.persistentDataPath + "/savedPresets.gd", FileMode.Open);
			SaveLoad.presets = (Dictionary<string, Preset>)bf.Deserialize (file);
			file.Close ();
		}

	}

}

//This means the class can be serialised
[System.Serializable]
public class Preset {

	/*
	 * This class acts as an abstract data structure to store the variables
	 * that exist within each preset. This allows different presets to be easily
	 * loaded and saved.
	 */

	//A static preset accessible anywhere that refers to the currently loaded parameters
	public static Preset current;

	//A boolean value to prevent System-defined presets from being deletable.
	public bool isSystemPreset;

	//Two dictionaries to store mosquito and human data.
	public Dictionary<string, float> humanData = new Dictionary<string, float>();
	public Dictionary<string, float> mosquitoData = new Dictionary<string, float>();

	public Preset() {

		/*
		 * Function that initialises the preset, setting
		 * each value in the dictionary to 0.
		 */ 

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

		/*
		 * Function to change any value in this data structure, given
		 * whether or not the data being changed is for a human, the key in the dictionary
		 * and the new value to be used.
		 */

		if (human) {
			humanData [key] = value;
		} else {
			mosquitoData [key] = value;
		}
	}

	public void PrintAllValues () {

		/*
		 * Debugging funtion to print values for testing.
		 */

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

	/*
	 * Class used to control the Input Menu
	 */ 

	//Human UI Sliders and InputField
	Dictionary<string, Slider> humansInputSliders = new Dictionary<string, Slider>();
	public InputField humansNum;
	public Slider HPercentageInfected, HRecovery, HTransfer, HDensity;

	//Mosquito UI Sliders and InputField
	Dictionary<string, Slider> mosquitosInputSliders = new Dictionary<string, Slider>();
	public InputField mosquitoNum;
	public Slider MPercentageInfected, MRecovery, MTransfer, MBiteRate;

	//Button to start the simulation 
	public Button startButton;

	//Text and button used to open Save Preset menu
	public Text savePresetText;
	public Button savePreset;

	//Dropdown element, InputField, panel and button used in the Save Preset menu
	public Dropdown dropdown;
	public GameObject SavePanel;
	public InputField SaveField;
	public Button SavePresetName;

	//Whether or not the user is entering custom inputs (otherwise input fields are locked)
	bool customInputs;

	//Preset object that stores custom data
	Preset customData;

	void Start () {

		/*
		 * Initialisation of variables. First presets are loaded in SaveLoad,
		 * next variables are assigned and the dropdown object is
		 * filled with the names of the presets.
		 */ 

		SaveLoad.Load ();

		customInputs = true;
		customData = new Preset ();
		UpdateNumHuman ();
		UpdateNumMosquito ();
		Preset.current = customData;

		//Setting names of options in dropdown menu
		dropdown.ClearOptions ();
		dropdown.options.Add (new Dropdown.OptionData ("Custom"));
		dropdown.RefreshShownValue ();
		foreach (string key in SaveLoad.presets.Keys) {
			dropdown.options.Add (new Dropdown.OptionData (key));
		}
			
		//Sliders initialised
		InitialiseSliders ();

	}
		

	void Update () {

		/*
		 * Called once per frame.
		 */ 

		if (SavePanel.activeSelf) {
			
			/*
			 * If Save Preset menu is open, only allows the user to save the preset
			 * if the field is not empty and the name hasn't already been used.
			 */ 

			if (SaveField.text == "" || SaveLoad.presets.ContainsKey (SaveField.text)) {
				SavePresetName.interactable = false;
			} else {
				SavePresetName.interactable = true;
			}
		}

		else if (customInputs) {

			/*
			 * If entering custom inputs, adjusts data according to how the sliders are moved.
			 */ 

			foreach (string key in humansInputSliders.Keys) {
				customData.AdjustData (true, key, humansInputSliders [key].value);
			}
				
			foreach (string key in mosquitosInputSliders.Keys) {
				customData.AdjustData (false, key, mosquitosInputSliders [key].value);
			}

			//Current preset updated according to customData
			Preset.current = customData;

			//Data validated
			ValidateInput ();

		} 

	}

	void InitialiseSliders() {

		/*
		 * Initialises sliders by adding a listener to each input field and
		 * by adding each slider to a dictionary containing the same
		 * keys as the dictionaries in the Preset object.
		 */ 
		
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

		/*
		 * Sets the start button to non-interactable if certain conditions are not met.
		 * These conditions are:
		 * 	- humansNum text must not be blank or equal to 0
		 * 	- mosquitosNum text must not be blank or equal to 0
		 * All other validation is done in the unity editor by setting the limits of the sliders
		 * and only allowing the user to input numerical values.
		 */ 

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

		/*
		 * Function to either delete currently selected preset or save currently
		 * entered custom data.
		 */ 

		if (savePresetText.text == "Save Preset") {
			//Saving preset, sets buttons to be non-interactable and opens SavePanel
			SetAllInteractable (false);
			SavePanel.SetActive (true);
			dropdown.interactable = false;
			startButton.interactable = false;
			savePreset.interactable = false;

		} else {
			if (!Preset.current.isSystemPreset) {
				
				/*	
				 * Only deletes preset if it isn't a System Preset.
				 * Deletes by using SaveLoad and reseting the dropdown menu's value
				 * and by setting customInputs to true.
				 */

				SaveLoad.Delete (dropdown.options [dropdown.value].text);
				dropdown.options.Remove (dropdown.options [dropdown.value]);
				dropdown.value = 0;
				customData = Preset.current;
				customInputs = true;
			}
		}
	}

	public void SaveCurrentPreset(bool cancel) {

		/*
		 * Only saves preset if cancel is not true, so that
		 * the SavePanel can be closed without saving.
		 * Saves currently saved preset using SaveLoad.Save().
		 * New preset is automatically selected in the dropdown
		 * menu, the Save Preset menu is closed
		 * and the menu buttons are set to interactable again.
		 */ 

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

		/*
		 * When a new preset is selected in the dropdown menu, this function is called.
		 * If the selectedIndex is 0, then the data is custom so customInputs is set
		 * to True and the savePreset text is set to "Save Preset".
		 * Otherwise, the new preset is loaeded and the buttons are made non-interactable.
		 * The savePreset text is also set to "Delete Preset" but only if the preset is
		 * not a System Preset (prevents System Presets from being deleted).
		 */ 

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

		/*
		 * Loads a saved preset. Sets the value of the sliders and
		 * text accoridngly to match the newly loaded data.
		 */ 

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

		/*
		 * Sets all input sliders and buttons to interactable
		 * or non-interactable according to val.
		 */ 

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
		
		/*
		 * Called whenever the humansNum input field is updated.
		 */ 

		customData.AdjustData (true, "Population Size", int.Parse (humansNum.textComponent.text));
	}

	void UpdateNumMosquito () {

		/*
		 * Called whenever the mosquitosNum input field is updated.
		 */ 

		customData.AdjustData (false, "Population Size", int.Parse (mosquitoNum.textComponent.text));
	}

}
