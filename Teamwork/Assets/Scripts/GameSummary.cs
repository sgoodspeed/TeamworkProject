using UnityEngine;
using System.Collections;

public class GameSummary : MonoBehaviour {
	// A GUIskin controls how all GUI elements look
	// make it public so we can modify it in the inspector and attach a new skin to it
	// In Unity, we clicked-and-dragged a GUIskin object onto this variable (like we would a public GameObject variable like the Cube)
	public GUISkin gameSummarySkin;
	
	// Use this for initialization
	void Start () {
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	void OnGUI (){
		// Tell Unity to use the skin we created (in the editor)
		GUI.skin = gameSummarySkin;

		if(GUI.Button (new Rect(300f,300f,200f,200f), "Play Again")){
			Application.LoadLevel("GameplayScene");	
		}
		
		GUI.Label (new Rect(300, 30, 200, 200), "Score\n"+ GameController.score);
		
		// TO DO:
		// display if we won or lost

	}
}
