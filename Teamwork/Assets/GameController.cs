using UnityEngine;
using System.Collections;

// Runs the logic for the whole game
public class GameController : MonoBehaviour {
	private GameCube[,] cubes;
	private int score;
	private Color[] cubeColors;
	private int[] colorValues;
	public int gridWidth, gridHeight;
	public GameObject aCube;

	// Use this for initialization
	void Start () {
		gridWidth = 8;
		gridHeight = 5;
		
		cubes = new GameCube[gridWidth, gridHeight];
		cubeColors = new Color[] {Color.white, Color.gray, Color.black, Color.blue, Color.green, Color.red, Color.yellow};
		colorValues = new int[] {0, 1, 2, 4, 8, 16, 32};
		
		for (int x = 0; x < gridWidth; x++) {
			for (int y = 0; y < gridHeight; y++) {
				cubes[x,y] = new GameCube(x, y, (GameObject) Instantiate(aCube, new Vector3(x*2 - 7, y*2 - 3, 2), Quaternion.identity));
			}
		}
	}
	
	// When a cube is clicked
	public void ProcessClickedCube (int x, int y) {
		// for now, when we click a cube, turn it a random color
		// 0 is white and 1 is grey, so use 2-7
		int randColor = Random.Range(2, 7);
		cubes[x,y].SetColor(randColor, cubeColors[randColor]);
		
		// now check to see if we should score
		CheckForScore();
	}
	
	// check the whole grid to see if there is a valid score
	private void CheckForScore() {

		// avoid checking the top or bottom rows, or left or right edges
		for (int x = 1; x < gridWidth - 1; x++) {
			for (int y = 1; y < gridHeight - 1; y++) {
				
				// check for the plus defined by the center cube in the potential plus
				switch (CheckForPlus(x, y)) {
					
					// if we didn't get a valid score, move on
					case ScoreResult.None:
						break;
					
					// if we got a same color valid score
					case ScoreResult.Same:
						score += 5;
						CreateGrayPlus(x, y);
						break;
					
					// if we got a rainbow color valid score
					case ScoreResult.Rainbow:
						score += 5;
						CreateGrayPlus(x, y);
						break;
				}
			}
		}
		
	}
	
	private ScoreResult CheckForPlus (int x, int y) {
		int c1 = cubes[x,y].GetColorIndex();
		int c2 = cubes[x+1,y].GetColorIndex();
		int c3 = cubes[x-1,y].GetColorIndex();
		int c4 = cubes[x,y+1].GetColorIndex();
		int c5 = cubes[x,y-1].GetColorIndex();
		
		// all the same, and not white and not gray
		if (c1 != 0 && c1 != 1 && c1 == c2 && c1 == c3 && c1 == c4 && c1 == c5) {
			return ScoreResult.Same;
		}
		
		// Check for rainbow, which is only possible if the math below is true, based on the color values we picked as a power of 2
		else if (colorValues[c1] + colorValues[c2] + colorValues[c3] + colorValues[c4] + colorValues[c5] == 62) {
			return ScoreResult.Rainbow;
		}
		
		// If we didn't get a same match or rainbow match, no score
		else {
			return ScoreResult.None;
		}
			
	}
	
	private void CreateGrayPlus (int x, int y) {
		// 1 is grey
		cubes[x, y].SetColor(1, cubeColors[1]);
		cubes[x+1, y].SetColor(1, cubeColors[1]);
		cubes[x-1, y].SetColor(1, cubeColors[1]);
		cubes[x, y+1].SetColor(1, cubeColors[1]);
		cubes[x, y-1].SetColor(1, cubeColors[1]);
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
