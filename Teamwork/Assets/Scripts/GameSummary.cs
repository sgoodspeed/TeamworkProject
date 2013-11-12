using UnityEngine;
using System.Collections;

public class GameSummary : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	void OnGUI (){
		if(GUI.Button (new Rect(200f,200f,200f,200f), "Play Again")){
			Application.LoadLevel("GameplayScene");	
		}
	}
}
