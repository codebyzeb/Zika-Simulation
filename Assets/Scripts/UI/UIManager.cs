using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class UIManager : MonoBehaviour {

	//Camera tracking variables
	Camera cam = null;
	public static Vector2 screenSize;
	const float screenToWorldRatio = 2.237f;

	//Zooming variables
	float minZoom;
	float maxZoom;
	float defaultZoom;

	//Dynamic movement variables
	Vector3 mousePosOriginal;
	float cameraMoveSpeed;

	public void InitialiseCamera (float minZoomTemp = 100, float maxZoomTemp = 2000, float defaultZoomTemp = 400) {

		/*
		 * Initialises camera
		*/

		minZoom = minZoomTemp;
		maxZoom = maxZoomTemp;
		cam = GetComponent<Camera> ();
		defaultZoom = defaultZoomTemp;
		cam.orthographicSize = defaultZoomTemp;
	}

	public void UpdateCamera () {

		/*
		 * Updates screensize and operates dynamic controls.
		*/

		DynamicControls ();
		screenSize = new Vector2 (cam.pixelRect.width, cam.pixelRect.height)*screenToWorldRatio;
	}

	void DynamicControls() {

		/*
		 * Function that allows for dynamic use of the camera, such as moving the camera laterally and vertically.
		*/

		cameraMoveSpeed = 10 * cam.orthographicSize / 100;
		ZoomMouse ();
		MoveCamera ();

		//Series of selection statements to prevent camera exceding the boundary limit.
		if (transform.position.x > maxZoom - cam.orthographicSize) {
			transform.position = new Vector3 (maxZoom - cam.orthographicSize, transform.position.y, -15);
		}
		if (transform.position.x < -maxZoom + cam.orthographicSize) {
			transform.position = new Vector3 (-maxZoom + cam.orthographicSize, transform.position.y, -15);
		}

		if (transform.position.y > maxZoom - cam.orthographicSize) {
			transform.position = new Vector3 (transform.position.x, maxZoom - cam.orthographicSize, -15);
		}
		if (transform.position.y < -maxZoom + cam.orthographicSize) {
			transform.position = new Vector3 (transform.position.x, -maxZoom + cam.orthographicSize, -15);
		}
	}

	void ZoomOrthoCamera(Vector3 zoomTowards, bool zoomingIn)
	{

		/*
		 * Function that operates the zoom algorithm, at once moving the camera towards
		 * a point and increasing or decreasing the zoom.
		*/

		float amount = 0;

		//Direction of zoom set
		if (zoomingIn == true) {
			amount = cameraMoveSpeed;
		} else {
			amount = -cameraMoveSpeed;
		}	

		// Calculate how much we will have to move towards the zoomTowards position
		float multiplier = (1.0f / cam.orthographicSize * amount);

		// Move camera
		transform.position += (zoomTowards - transform.position) * multiplier; 

		// Zoom camera
		cam.orthographicSize -= amount;

		// Limit zoom using clamp function
		cam.orthographicSize = Mathf.Clamp(cam.orthographicSize, minZoom, maxZoom);

	}

	public void ZoomButton (bool zoomingIn) {

		/*
		 * Function called from button to zoom camera in or out
		*/

		ZoomOrthoCamera (transform.position, zoomingIn);
	}
		
	public void ResetCamera ()
	{
		/*
		 * Function called by a button to reset zoom and position of camera.
		*/

		transform.position = new Vector3 (0, 0, -15);
		cam.orthographicSize = defaultZoom;
	}

	void ZoomMouse ()
	{

		/*
		 * Function called by DynamicControls to allow zooming with the mouse wheel
		*/

		if (Input.GetAxis("Mouse ScrollWheel") > 0)
		{
			//Scrolling in
			ZoomOrthoCamera(Camera.main.ScreenToWorldPoint(Input.mousePosition), true);
		}
			
		if (Input.GetAxis("Mouse ScrollWheel") < 0)
		{
			// Scolling back
			ZoomOrthoCamera(Camera.main.ScreenToWorldPoint(Input.mousePosition), false);
		}
	}
		
	void MoveCamera () {

		/*
		 * Function called by DynamicControls to move the focus of the camera.
		*/

		//Moving the camera with the mouse
		if (Input.GetMouseButtonDown(1)) {
			//Origin point set when mouse clicked
			mousePosOriginal = Input.mousePosition;
		}
		if (Input.GetMouseButton (1)) {
			//Camera translated based on distance dragged away from origin point and dragSpeed
			Vector3 mousePos = Camera.main.ScreenToViewportPoint(Input.mousePosition-mousePosOriginal);
			Vector3 move = new Vector3(mousePos.x * cameraMoveSpeed, mousePos.y * cameraMoveSpeed, 0);

			transform.Translate(move, Space.World); 
		}

		//Moving the camera with the keyboard
		else {
			
			if (Input.GetKey (KeyCode.W) || Input.GetKey (KeyCode.UpArrow)) {
				transform.position += Vector3.up * cameraMoveSpeed / 10;
			}
			if (Input.GetKey (KeyCode.S) || Input.GetKey (KeyCode.DownArrow)) {
				transform.position -= Vector3.up * cameraMoveSpeed / 10;
			}
			if (Input.GetKey (KeyCode.A) || Input.GetKey (KeyCode.LeftArrow)) {
				transform.position += Vector3.left * cameraMoveSpeed / 10;
			}
			if (Input.GetKey (KeyCode.D) || Input.GetKey (KeyCode.RightArrow)) {
				transform.position -= Vector3.left * cameraMoveSpeed / 10;
			}

		}
	}
		
}
