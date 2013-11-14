using UnityEngine;
using System.Collections;

public class GameSummary : MonoBehaviour {
	public GUIStyle style;
	
	// Use this for initialization
	void Start () {
		style = new GUIStyle();
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	void OnGUI (){
		style.fontSize = 36;
		style.alignment = TextAnchor.MiddleCenter;

		if(GUI.Button (new Rect(200f,200f,200f,200f), "Play Again", style)){
			Application.LoadLevel("GameplayScene");	
		}
				
		GUI.Box (new Rect(800, 45, 100, 60), "Score\n"+ GameController.score, style);

	}
}
