using UnityEngine;
using System.Collections;

public class CubeBehavior : MonoBehaviour {
	private GameController aGameController;
	public int x, y;
	
	// Use this for initialization
	void Start () {
		// find the GameControllerObject, and then get the GameController script component that's on it
		aGameController = GameObject.Find("GameControllerObject").GetComponent<GameController>();
	}
	
	// Update is called once per frame
	void Update () {
	}
	
	void OnMouseDown () {
		// when we click the cube, call the appropriate method in our main game controller
		// send this cube's coordinates as arguments
		aGameController.ProcessClickedCube(x, y);
	}
		
		
}
