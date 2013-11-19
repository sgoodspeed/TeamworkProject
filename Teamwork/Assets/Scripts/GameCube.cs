using UnityEngine;
using System.Collections;

// A class to track the cube objects in the game
public class GameCube {
	private int colorIndex;
	private int x, y;
	private GameObject cube;
	private bool active, hidden;
	private float scaleFactor = 0.6f;
	
	// Constructor. When we create the GameCube object, we'll set the values of various important items
	public GameCube (int startX, int startY, GameObject aCube) {
		colorIndex = 0;
		x = startX;
		y = startY;
		active = false;
		hidden = false;
		cube = aCube;
		// store the x and y variables on the cube behavior script as well, so the OnMouseDown method on that cube knows it's x and y position in the grid (regardless of xyz position in unity space)
		cube.GetComponent<CubeBehavior>().x = x;
		cube.GetComponent<CubeBehavior>().y = y;
	}
	
	// Alternate Constructor for the Next Cube, which doesn't go in the grid, and isn't clickable
	// When we call this contructor, we send it an unclickableCube prefab instead of the normal cube prefab
	// The unclickableCube prefab doesn't have a CubeBehavior script on it, so it never calls OnMouseDown
	public GameCube (GameObject aCube) {
		colorIndex = 0;
		active = false;
		hidden = false;
		cube = aCube;
	}
	
	public void SetColor (int index, Color newColor) {
		colorIndex = index;
		cube.renderer.material.color = newColor;
	}
	
	public int GetColorIndex () {
		// This will currently return 1 for grey even in the case of a hidden cube, but that's OK for game logic
		return colorIndex;
	}

	public bool IsActive () {
		return active;
	}

	public bool IsHidden () {
		return hidden;
	}

	public void SetActive (bool act) {
		// only if it's not hidden
		if (!hidden) {
			active = act;
		
			// make the cube bigger
			if (active) {
				// grow the cube
				cube.transform.localScale += new Vector3(scaleFactor, scaleFactor, scaleFactor);
			}
			// the cube just deactivated
			else {
				// reset to normal size
				cube.transform.localScale = new Vector3(1f, 1f, 1f);
			}
		}
		
	}

	public void SetHidden (bool hide) {
		
		if (hide) {
			// set the proper variable		
			hidden = true;
		
			// hide the cube so it's not clickable any more
			cube.transform.renderer.enabled = false;
		
			// deactivate it for safety
			active = false;
		
			// set the colorIndex to grey for safety, just in case we try to get it's color index later
			colorIndex = 1;
		}
		// if we want to unhide
		else {
			hidden = false;
			cube.transform.renderer.enabled = true;
		}
	}


}
