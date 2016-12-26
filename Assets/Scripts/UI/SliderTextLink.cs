using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class SliderTextLink : MonoBehaviour {

	/*
	 * This is a very simple script used in the Input Menu to change the value
	 * of the text box next to each slider according to the value selected.
	 */ 

	public Text text;
	public string defaultText;
	Slider slider;

	void Start () {

		/*
		 * Initialises the variables for this script
		 */ 

		slider = GetComponent<Slider> ();
		slider.onValueChanged.AddListener (delegate {
			ValueChangeCheck ();
		});
		ValueChangeCheck ();
	}

	public void ValueChangeCheck () {

		/*
		 * Function called by each slider when the value is
		 * changed to update the text next to it.
		 */ 

		text.text = defaultText + ": " + Mathf.Round (slider.value * 100) / 100;
	}
}

