using UnityEngine;
using System.Collections;

// A class to track the cube objects in the game
public class GameCube {
	private int colorIndex;
	private int x, y;
	private GameObject cube;
	private bool active;
	
	// Constructor. When we create the GameCube object, we'll set the values of various important items
	public GameCube (int startX, int startY, GameObject aCube) {
		colorIndex = 0;
		x = startX;
		y = startY;
		active = false;
		cube = aCube;
		cube.GetComponent<CubeBehavior>().x = x;
		cube.GetComponent<CubeBehavior>().y = y;
	
	}
	
	public void SetColor (int index, Color newColor) {
		colorIndex = index;
		cube.renderer.material.color = newColor;
	}
	
	public int GetColorIndex () {
		return colorIndex;
	}

	public bool IsActive () {
		return active;
	}

	public void SetActive (bool act) {
		active = act;
		
		// turn on or off a visual effect based on whether or not it's active
		
		
	}

}
