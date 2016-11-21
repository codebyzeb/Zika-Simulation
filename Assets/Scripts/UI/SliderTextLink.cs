using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class SliderTextLink : MonoBehaviour {

	public Text text;
	public string defaultText;
	Slider slider;

	// Use this for initialization
	void Start () {
		slider = GetComponent<Slider> ();
		slider.onValueChanged.AddListener (delegate {
			ValueChangeCheck ();
		});
	}

	public void ValueChangeCheck () {
		text.text = defaultText + ": " + Mathf.Round (slider.value * 100) / 100;;
	}
}

