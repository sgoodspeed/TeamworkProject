using UnityEngine;
using System.Collections;

public class CubeBehavior : MonoBehaviour {
	private GameController aGameController;
	public int x, y;
	
	// Use this for initialization
	void Start () {
		aGameController = GameObject.Find("GameControllerObject").GetComponent<GameController>();
		
	}
	
	// Update is called once per frame
	void Update () {
	  // Added some new code
	}
	
	void OnMouseDown () {
		aGameController.ProcessClickedCube(x, y);
	}
		
		
}
