using UnityEngine;
using System.Collections;

// A class to track the cube objects in the game
public class GameCube {
	private int colorIndex;
	private int x, y;
	private GameObject cube;
	
	
	// Constructor. When we create the GameCube object, we'll set the values of various important items
	public GameCube (int startX, int startY, GameObject aCube) {
		colorIndex = 0;
		x = startX;
		y = startY;
		cube = aCube;
		aCube.GetComponent<CubeBehavior>().x = x;
		aCube.GetComponent<CubeBehavior>().y = y;
	
	}
	
	public void SetColor (int index, Color newColor) {
		colorIndex = index;
		cube.renderer.material.color = newColor;
	}
	
	public int GetColorIndex () {
		return colorIndex;
	}

}
