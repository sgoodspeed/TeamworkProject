using UnityEngine;
using System.Collections;

// Runs the logic for the whole game
public class GameController : MonoBehaviour {
	private GameCube[,] cubes;
	private int[] numWhiteCubes;
	public static int score;
	private Color[] cubeColors;
	private int[] colorValues;
	public int gridWidth, gridHeight;
	public GameObject aCube, unclickableCube;
	private GameCube nextCube;
	private int activeCubeX, activeCubeY;
	public float turnLength, gameLength;
	private float turnTimer, gameTimer;

	// Use this for initialization
	void Start () {
		gridWidth = 8;
		gridHeight = 5;
		
		// initial values
		turnLength = 3f;
		gameLength = 90f;
		turnTimer = 0f;
		gameTimer = gameLength;
		score = 0;
		
		// available colors
		cubeColors = new Color[] {Color.white, Color.gray, Color.black, Color.blue, Color.green, Color.red, Color.yellow};
		colorValues = new int[] {0, 1, 2, 4, 8, 16, 32};

		// track the active cube. Default to -1 at initialization
		activeCubeX = -1;
		activeCubeY = -1;

		
		cubes = new GameCube[gridWidth, gridHeight];
		numWhiteCubes = new int[gridHeight];
		// create the grid, row by row
		for (int y = 0; y < gridHeight; y++) {
			// track the number of white cubes
			numWhiteCubes[y] = gridWidth;
			
			// create each cube in the row
			for (int x = 0; x < gridWidth; x++) {
				cubes[x,y] = new GameCube(x, y, (GameObject) Instantiate(aCube, new Vector3(x*2 - 7, y*2 - 3, 2), Quaternion.identity));
			}
		}

		// create the next cube - it's a special prefab without a script on it to avoid being clickable
		nextCube = new GameCube((GameObject) Instantiate(unclickableCube, new Vector3(0, 7.5f, 2), Quaternion.identity));
		
		// start with a cube in the Next Cube area
		ShowNextCube();
		
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
		
		// if the cube is white AND we had an active cube already AND the cubes are adjacent
		else if (cubes[x,y].GetColorIndex() == 0 && activeCubeX > -1 && IsAdjacent(x,y,activeCubeX,activeCubeY) ) {
			
			// set the clicked cube to the active cube's color
			int activeCubeColorIndex = cubes[activeCubeX, activeCubeY].GetColorIndex();
			cubes[x,y].SetColor( activeCubeColorIndex, cubeColors[activeCubeColorIndex] );
			
			// set the previous cube to white, and deactivate it
			cubes[activeCubeX, activeCubeY].SetColor(0, cubeColors[0]);
			cubes[activeCubeX, activeCubeY].SetActive(false);
			
			// update the white cube count
			// new row has one fewer, old row has one less. (Note: If old row and new row are the same, then this cancels itself out)
			numWhiteCubes[y]--;
			numWhiteCubes[activeCubeY]++;
			
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
		int row = -1;
		
		// Only do something if the NextCube is not hidden
		if (!nextCube.IsHidden()) {
			
			// Did we detect a number key?
			// Note that using Input.inputString is tricky since the player could mash multiple buttons simultaneously,
			// so use GetKeyDown instead
			if (Input.GetKeyDown("1")) {
				row = 0;
			}
			// give preference to lower rows; if the player presses 1 and 2 at the same time, move to row 1
			else if (Input.GetKeyDown("2")) {
				row = 1;
			}
			else if (Input.GetKeyDown("3")) {
				row = 2;
			}
			else if (Input.GetKeyDown("4")) {
				row = 3;
			}
			else if (Input.GetKeyDown("5")) {
				row = 4;
			}
			
			// if we detected a valid key
			if (row >= 0) {
				// hide a random white cube in the appropriate row
				if (ColorRandomWhiteCube(row) == false) {
					// and if we can't, end the game
					Application.LoadLevel("GameSummaryScene");
				}
				
				// hide the next cube display
				nextCube.SetHidden(true);
			}
		}
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
						score += 10;
						CreateGrayPlus(x, y);
						break;
					
					// if we got a rainbow color valid score
					case ScoreResult.Rainbow:
						score += 5;
						CreateGrayPlus(x, y);
						break;

					// if we got a super rainbow score
					// currently not detected
					case ScoreResult.Rainbow9:
						score += 15;
						//CreateGraySuperPlus(x, y);
						break;

					// if we got a super same plus score
					// currently not detected
					case ScoreResult.Same9:
						score += 40;
						//CreateGraySuperPlus(x, y);
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
			// do we have a super plus with 9 cubes?
			if (CheckForSuperPlus(x, y, ScoreResult.Same)) {
				return ScoreResult.Same9;
			}
			else {
				return ScoreResult.Same;
			}
		}
		
		// Check for rainbow, which is only possible if the math below is true, based on the color values we picked as a power of 2
		else if (colorValues[c1] + colorValues[c2] + colorValues[c3] + colorValues[c4] + colorValues[c5] == 62) {
			// do we have a super plus with 9 cubes?
			if (CheckForSuperPlus(x, y, ScoreResult.Rainbow)) {
				return ScoreResult.Rainbow9;
			}
			else {
				return ScoreResult.Rainbow;
			}
		}
		
		// If we didn't get a same match or rainbow match, no score
		else {
			return ScoreResult.None;
		}
			
	}
	
	private bool CheckForSuperPlus (int x, int y, ScoreResult r) {
		// do some logic here!
		return false;
	}
	
	private void CreateGrayPlus (int x, int y) {
		// 1 is gray
		cubes[x, y].SetColor(1, cubeColors[1]);
		cubes[x+1, y].SetColor(1, cubeColors[1]);
		cubes[x-1, y].SetColor(1, cubeColors[1]);
		cubes[x, y+1].SetColor(1, cubeColors[1]);
		cubes[x, y-1].SetColor(1, cubeColors[1]);
		
		// if any of them are the active cube, deactivate it
		if ( x == activeCubeX && y == activeCubeY ||
			 x + 1 == activeCubeX && y == activeCubeY ||
			 x - 1 == activeCubeX && y == activeCubeY ||
			 x == activeCubeX && y + 1 == activeCubeY ||
			 x == activeCubeX && y - 1 == activeCubeY) {
			// deactivate the active cube
			cubes[activeCubeX, activeCubeY].SetActive(false);
			activeCubeX = -1;
			activeCubeY = -1;
		}
			
	}
	
	private void ShowNextCube () {
		// Make a random cube colored
		// 0 is white and 1 is grey, so use 2 through the length of the cubeColors array
		// Range using ints is inclusive for the first argument, and exclusive for the second argument
		int randColor = Random.Range(2, cubeColors.Length);
		nextCube.SetColor(randColor, cubeColors[randColor]);
		
		// make the cube visible in case it was hidden before
		nextCube.SetHidden(false);
	}
	
	private bool HideRandomWhiteCube() {
		
		// calculate the total number of white cubes
		int totalWhiteCubes = 0;
		for (int i = 0; i < numWhiteCubes.Length; i++) {
			totalWhiteCubes += numWhiteCubes[i];
		}
		
		// if there are no white cubes left, return false
		if (totalWhiteCubes == 0) {
			return false;	
		}
		
		// set starting values
		int randX = -1;
		int randY = -1;
		
		// we know there's at least one valid location, so this won't be an infinite loop
		// while we haven't found a white cube, keep trying
		while (randX < 0) {
			// try a random cube
			randX = Random.Range(0, gridWidth);
			randY = Random.Range(0, gridHeight);
			
			// if the random cube isn't white (0 is white)
			if (cubes[randX, randY].GetColorIndex() > 0) {
				// reset the values and try again
				randX = -1;
				randY = -1;
			}
		}
		
		// hide the chosen cube
		cubes[randX,randY].SetHidden(true);
		
		// update the white cube count
		numWhiteCubes[randY]--;
		
		return true;
	}

	private bool ColorRandomWhiteCube(int row) {
		
		// If the row is already full, return false
		if (numWhiteCubes[row] == 0) {
			return false;
		}
		
		int col = -1;
		// we know there's at least one valid location
		while (col < 0) {
			// try a random column
			col = Random.Range(0, gridWidth);
			
			// if it's not white (0 is white)
			if (cubes[col, row].GetColorIndex() > 0) {
				// then reset column to -1 and keep trying
				col = -1;
			}
		}
		
		// set the chosen cube to the color of the next cube
		cubes[col,row].SetColor(nextCube.GetColorIndex(), cubeColors[nextCube.GetColorIndex()]);	
		
		// update the white cube count
		numWhiteCubes[row]--;
			
		return true;
	}

	// Update is called once per frame
	void Update () {
		// Check for keyboard input every frame
		ProcessKeyboardInput();
		
		// update the timers
		turnTimer += Time.deltaTime;
		gameTimer -= Time.deltaTime;

		// if it's time to take a turn
		if (turnTimer >= turnLength) {
			// reset the turn timer
			turnTimer = 0;
			
			// if the player failed to move the cube from the Next Cube area
			if (!nextCube.IsHidden()) {
				// decrease score, but not below zero
				score = Mathf.Max(score - 1, 0);
				
				// try to hide random white cube
				if (HideRandomWhiteCube() == false) {
					// if we couldn't, end the game
					Application.LoadLevel("GameSummaryScene");
				}
			}
			
			// Show the next cube
			ShowNextCube();
		
		}
		
		// end the game (after taking our turn above)
		if (gameTimer <= 0) {
			Application.LoadLevel("GameSummaryScene");
		}	
	}
	
	void OnGUI () {
		GUIStyle style = new GUIStyle();
		style.fontSize = 36;
		style.alignment = TextAnchor.MiddleCenter;
		
		GUI.Box (new Rect(370, 45, 100, 50), "Next\nCube", style);
		
		GUI.Box (new Rect(800, 45, 100, 60), "Score\n"+score, style);
		
	}
	
}
