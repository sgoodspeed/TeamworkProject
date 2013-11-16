using UnityEngine;
using System.Collections;

public class StartScreenBehavior : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	void OnMouseDown(){
		//Plane with an image attached is our background, if the player 
		//clicks anywhere on screen it will activate onmousedown
	Application.LoadLevel("GameplayScene");	
	}
}
