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
	private int activeCubeX, activeCubeY;
	public float turnLength, gameLength;
	private float turnTimer, gameTimer;

	// Use this for initialization
	void Start () {
		gridWidth = 8;
		gridHeight = 5;
		
		// initial values
		turnLength = 2f;
		gameLength = 60f;
		turnTimer = 0f;
		gameTimer = gameLength;
		
		// track the active cube. Default to -1 at initialization
		activeCubeX = -1;
		activeCubeY = -1;
		
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
		// if the cube is colored
		if (cubes[x,y].GetColorIndex() > 1) {
			
			// if it's not active already, make it active
			if (!cubes[x,y].IsActive()) {
				cubes[x,y].SetActive(true);
				
				// if there was previously an active cube, make it inactive
				if (activeCubeX > -1) {
					cubes[activeCubeX, activeCubeY].SetActive(false);
				}
				
				// update the tracking of the active cube
				activeCubeX = x;
				activeCubeY = y;
			}
			// the colored cube we just clicked was active
			else {
				// deactivate it
				cubes[x,y].SetActive(false);
				// and update our tracking to match
				activeCubeX = -1;
				activeCubeY = -1;
			}
		}
		
		// if the cube is white AND we had an active cube already && the cubes are adjacent
		else if (cubes[x,y].GetColorIndex() == 0 && activeCubeX > -1 && IsAdjacent(x,y,activeCubeX,activeCubeY) ) {
			
			// set the clicked cube to the active cube's color
			int activeCubeColorIndex = cubes[activeCubeX, activeCubeY].GetColorIndex();
			cubes[x,y].SetColor( activeCubeColorIndex, cubeColors[activeCubeColorIndex] );
			
			// set the previous cube to white, and deactivate it
			cubes[activeCubeX, activeCubeY].SetColor(0, cubeColors[0]);
			cubes[activeCubeX, activeCubeY].SetActive(false);
			
			// set the new cube to be active and update the tracking
			cubes[x,y].SetActive(true);
			activeCubeX = x;
			activeCubeY = y;
			
			// since something moved, now check to see if we should score
			CheckForScore();
		}
		
	}
	
	private bool IsAdjacent(int x1, int y1, int x2, int y2) {
		int xDistance, yDistance;
		
		// if we just clicked on the same spot, return false since the cube itself isn't adjacent to itself
		if (x1 == x2 && y1 == y2) {
			return false;
		}

		// otherwise, calculate distance for x and y
		xDistance = Mathf.Abs(x1 - x2);
		yDistance = Mathf.Abs(y1 - y2);
		
		// it's adjacent as long as both values are within 1
		return xDistance < 2 && yDistance < 2;
		
	}
	
	private void ProcessKeyboardInput () {
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
		// 1 is gray
		cubes[x, y].SetColor(1, cubeColors[1]);
		cubes[x+1, y].SetColor(1, cubeColors[1]);
		cubes[x-1, y].SetColor(1, cubeColors[1]);
		cubes[x, y+1].SetColor(1, cubeColors[1]);
		cubes[x, y-1].SetColor(1, cubeColors[1]);
	}
	
	// Update is called once per frame
	void Update () {
		turnTimer += Time.deltaTime;
		gameTimer -= Time.deltaTime;
		
		// if it's time to take a turn
		if (turnTimer >= turnLength) {
			// reset the turn timer
			turnTimer = 0;
		
			// DEBUG ONLY
			// Make a random cube colored
			// 0 is white and 1 is grey, so use 2-7
			int randColor = Random.Range(2, 7);
			int randX = Random.Range(0, gridWidth);
			int randY = Random.Range(0, gridHeight);
			cubes[randX,randY].SetColor(randColor, cubeColors[randColor]);

			
			// Check for keyboard input
			// ProcessKeyboardInput();
		}
		
		// end the game (after taking our turn above)
		if (gameTimer <= 0) {
			Application.LoadLevel("GameSummaryScene");
		}
		
	}
}
