using UnityEngine;
using System.Collections;

// Runs the logic for the whole game
public class GameController : MonoBehaviour {
	// to track score across scenes, make it public static
	public static int score;
	
	// these are public so they are exposed in the editor
	// the game designer can change these values as appropriate while the game is running
	public int gridWidth, gridHeight;
	public float turnLength, gameLength;
	public int samePlusPoints, rainbowPlusPoints, sameDoublePlusPoints, rainbowDoublePlusPoints, mixedDoublePlusPoints;

	// these are public so we can assign the prefabs in unity
	public GameObject aCube, unclickableCube;

	// these are private because we should only be changing them via the code here
	private GameCube[,] cubes;
	private int[] numWhiteCubes;
	private Color[] cubeColors;
	private int[] colorValues;
	private GameCube nextCube;
	private int activeCubeX, activeCubeY;
	private float turnTimer, gameTimer;
	
	// Use this for initialization
	void Start () {
		gridWidth = 8;
		gridHeight = 5;
		
		// initial values
		turnLength = 2f;
		gameLength = 120f;
		turnTimer = 0f;
		gameTimer = gameLength;
		score = 0;
		
		// scoring values
		samePlusPoints = 10;
		rainbowPlusPoints = 5;
		sameDoublePlusPoints = 40;
		rainbowDoublePlusPoints = 20;
		mixedDoublePlusPoints = 30;

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
		// default invalid value that we may overwrite later
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
				// attempt to hide a random white cube in the appropriate row
				if (ColorRandomWhiteCube(row) == false) {
					// and if we can't, end the game
					Application.LoadLevel("GameSummaryScene");
				}
				
				// hide the next cube display
				nextCube.SetHidden(true);
				
				// Since we just placed a colored cube, check to see if we scored
				CheckForScore();
			}
		}
	}
		

	// check the whole grid to see if there is a valid score
	private void CheckForScore() {
		// a helper variable to track score results
		ScoreResult[,] scoreResults = new ScoreResult[gridWidth, gridHeight];
		
		// avoid checking the top or bottom rows, or left or right edges
		for (int x = 1; x < gridWidth - 1; x++) {
			for (int y = 1; y < gridHeight - 1; y++) {
				// check for a plus once, store the result, and then do game logic later.
				// This way, we avoid checking and rechecking the same cubes over and over.
				scoreResults[x,y] = CheckForPlus (x, y);
			}
		}
		
		// look at the results and see if we find any double pluses or regular pluses
		for (int x = 1; x < gridWidth - 1; x++) {
			for (int y = 1; y < gridHeight - 1; y++) {

				// if we don't have a score
				if (scoreResults[x,y] == ScoreResult.None) {
					continue; // this continues to the next iteration of the for() loop
				}
				
				// Otherwise, we have a valid plus of some sort,
				// so we turn this group of cubes gray
				CreateGrayPlus(x, y);
			
				// do we have a horizontal double plus, within the bounds of the grid?
				// NOTE: if the first comparison is false (x < gridWidth-3), the second comparison won't ever be checked.
				// Therefore, the order of these comparisons is important here, to avoid going outside the grid and getting an ArrayOutOfBounds exception
				if (x < gridWidth - 3 && scoreResults[x+2, y] != ScoreResult.None) {
					CreateGrayPlus(x+2, y);
					ScoreDoublePlus(scoreResults[x,y], scoreResults[x+2, y]);
					// change the double plus score result to none to avoid double counting it
					scoreResults[x+2, y] = ScoreResult.None;
				}
			
				// do we have a vertical double plus, within the bounds of the grid?
				// NOTE: if the first comparison is false (y < gridHeight-3), the second comparison won't ever be checked.
				// Therefore, the order of these comparisons is important here, to avoid going outside the grid and getting an ArrayOutOfBounds exception
				else if (y < gridHeight - 3 && scoreResults[x, y+2] != ScoreResult.None) {
					CreateGrayPlus(x, y+2);
					ScoreDoublePlus(scoreResults[x,y], scoreResults[x, y+2]);
					// change the double plus score result to none to avoid double counting it
					scoreResults[x, y+2] = ScoreResult.None;
				}
			
				// no double plus, just a normal plus result
				else {
					if (scoreResults[x,y] == ScoreResult.Same) {
						score += samePlusPoints;
					}
					else if (scoreResults[x,y] == ScoreResult.Rainbow) {
						score += rainbowPlusPoints;
					}
				}
			}
		}
	}
		
	// only called when we know for sure we have a double plus
	private void ScoreDoublePlus (ScoreResult originalPlus, ScoreResult doublePlus) {
		// error checking, just to be sure:
		if (originalPlus == ScoreResult.None || doublePlus == ScoreResult.None) {
			// we should never get here
			return;
		}

		// double same
		if (originalPlus == ScoreResult.Same && doublePlus == ScoreResult.Same) {
			score += sameDoublePlusPoints;		
		}
		// double rainbow https://www.youtube.com/watch?v=OQSNhk5ICTI
		else if (originalPlus == ScoreResult.Rainbow && doublePlus == ScoreResult.Rainbow) {
			score += rainbowDoublePlusPoints;
		}
		// double mixed
		else {
			score += mixedDoublePlusPoints;
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
		// We could also do this using GUISkin. See the Game Summary for an example of GUISkin.
		GUIStyle style = new GUIStyle();
		style.fontSize = 36;
		style.alignment = TextAnchor.MiddleCenter;
		
		GUI.Box (new Rect(370, 45, 100, 50), "Next\nCube", style);
		
		// the \n after Score means "new line"
		GUI.Box (new Rect(800, 45, 100, 60), "Score\n"+score, style);
		
	}
	
}
