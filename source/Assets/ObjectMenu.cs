using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A console to display Unity's debug logs in-game. Credit goes to: https://gist.github.com/mminer/975374
/// </summary>
public class ObjectMenu : MonoBehaviour
{
	
	/// <summary>
	/// The hotkey to show and hide the console window.
	/// </summary>
	public KeyCode toggleKey = KeyCode.M;
	
	//Objects that can be created, along with their prefab ID
	//public GameObject garf;
	
	public GameObject wallHorizontal; //7
	public GameObject wallVertical; //8
	public GameObject wall; //6
	public GameObject leftRamp; //5
	public GameObject rightRamp; //6
	
	public GameObject fWallHorizontal; //11
	public GameObject fWallVertical; //12
	public GameObject fWall; //13
	public GameObject fLeftRamp; //10
	public GameObject fRightRamp; //9
	public GameObject redDynamite; //16
	public GameObject blueDynamite; //14
	public GameObject greenDynamite; //15
	public GameObject redWall; //19
	public GameObject blueWall; //17
	public GameObject greenWall; //18
	
	
	public GameObject apple; //1
	public GameObject bacon; //2
	public GameObject steak; //20
	public GameObject pill; //21
	public GameObject redPill; //22
	public GameObject poison; //3
	public GameObject healthPack; //23
	public GameObject doubleHealthPack; //24
	public GameObject injuredMan; //25
	
	public GameObject triggerBox; //26
	public GameObject lavaBlock; //27
	public GameObject iceBlock; //28

	//These have prefab IDs but are not on the object menu
	public GameObject customItemBlock; //29
	public GameObject customItemEndorphinBlock; //30
	
	Vector2 scrollPosition;
	bool show;
	bool collapse = true;
	
	const int margin = 20;
	
	Rect windowRect = new Rect(margin, margin, Screen.width - (margin * 2), Screen.height/2 - (margin * 2));
	Rect titleBarRect = new Rect(0, 0, 100, 20);
	//GUIContent clearLabel = new GUIContent("Clear", "Clear the contents of the console.");
	//GUIContent collapseLabel = new GUIContent("Collapse", "Hide repeated messages.");
	
	void OnEnable ()
	{
		//gameObject.BroadcastMessage(
		//Application.RegisterLogCallback(HandleLog);
	}
	
	void OnDisable ()
	{
		//Application.RegisterLogCallback(null);
	}
	
	void Update ()
	{
		if (Input.GetKeyDown(toggleKey)) {
			windowRect = new Rect(margin, margin, Screen.width - (margin * 2), Screen.height - (margin * 2));
			show = !show;
		}
	}
	
	void OnGUI ()
	{
		if (!show) {
			return;
		}
		
		Event e = Event.current;
		if (e.keyCode == KeyCode.Escape) show = false;
		else
			windowRect = GUILayout.Window(1234511, windowRect, ConsoleWindow, "Objects: Click to create");
	}
	
	/// <summary>
	/// A window that displayss the recorded logs.
	/// </summary>
	/// <param name="windowID">Window ID.</param>
	void ConsoleWindow (int windowID)
	{
		/*scrollPosition = GUILayout.BeginScrollView(scrollPosition);
		
		// Iterate through the recorded logs.
		for (int i = 0; i < logs.Count; i++) {
			var log = logs[i];
			
			// Combine identical messages if collapse option is chosen.
			if (collapse) {
				var messageSameAsPrevious = i > 0 && log.message == logs[i - 1].message;
				
				if (messageSameAsPrevious) {
					continue;
				}
			}
			
			GUI.contentColor = logTypeColors[log.type];
			GUILayout.Label(log.message);
		}
		
		GUILayout.EndScrollView();*/
		
		GUI.contentColor = Color.white;
		
		if (GUI.Button(new Rect(0, 0, 18, 18), new GUIContent("x")))
			show = false;
		
		
		GUILayout.BeginHorizontal();
			GUILayout.Label("Walls / Ramps (non-floating)");
		GUILayout.EndHorizontal();
		
		GUILayout.BeginHorizontal();
		if (GUILayout.Button(new GUIContent("Wall Piece", "Creates a normal wall block."))) {
			Instantiate(wall, new Vector2(0, 20f), new Quaternion());
		}
		if (GUILayout.Button(new GUIContent("Horizontal Wall", "Creates a horizontal wall."))) {
			Instantiate(wallHorizontal, new Vector2(0, 20f), new Quaternion());
		}
		if (GUILayout.Button(new GUIContent("Vertical Wall", "Creates a vertical wall."))) {
			Instantiate(wallVertical, new Vector2(0, 20f), new Quaternion());
		}
		if (GUILayout.Button(new GUIContent("Left Ramp", "Creates a left ramp."))) {
			Instantiate(leftRamp, new Vector2(0, 20f), new Quaternion());
		}
		if (GUILayout.Button(new GUIContent("Right Ramp", "Creates a right ramp."))) {
			Instantiate(rightRamp, new Vector2(0, 20f), new Quaternion());
		}
		if (GUILayout.Button(new GUIContent("Lava Block", "Creates a very hot block."))) {
			//logs.Clear();
			Instantiate(lavaBlock, new Vector2(0, 20f), new Quaternion());
		}
		if (GUILayout.Button(new GUIContent("Ice Block", "Creates a very cold block."))) {
			//logs.Clear();
			Instantiate(iceBlock, new Vector2(0, 20f), new Quaternion());
		}
		GUILayout.EndHorizontal();
		
		
		
		
		GUILayout.BeginHorizontal();
		GUILayout.Label("Walls / Ramps (floating)");
		GUILayout.EndHorizontal();
		
		GUILayout.BeginHorizontal();
		if (GUILayout.Button(new GUIContent("Wall Piece", "Creates a floating wall."))) {
			Instantiate(fWall, new Vector2(-10f, 20f), new Quaternion());
		}
		if (GUILayout.Button(new GUIContent("Horizontal Wall", "Creates a horizontal wall."))) {
			Instantiate(fWallHorizontal, new Vector2(-5f, 20f), new Quaternion());
		}
		if (GUILayout.Button(new GUIContent("Vertical Wall", "Creates a vertical wall."))) {
			Instantiate(fWallVertical, new Vector2(0, 20f), new Quaternion());
		}
		if (GUILayout.Button(new GUIContent("Left Ramp", "Creates a left ramp."))) {
			Instantiate(fLeftRamp, new Vector2(5f, 20f), new Quaternion());
		}
		if (GUILayout.Button(new GUIContent("Right Ramp", "Creates a right ramp."))) {
			Instantiate(fRightRamp, new Vector2(10f, 20f), new Quaternion());
		}
		GUILayout.EndHorizontal();
		
		
		
		
		GUILayout.BeginHorizontal();
		GUILayout.Label("Explosives");
		GUILayout.EndHorizontal();
		
		GUILayout.BeginHorizontal();
		
		if (GUILayout.Button(new GUIContent("Red Wall", "Creates a red wall block."))) {
			Instantiate(redWall, new Vector3(-5,15), new Quaternion());
		}
		if (GUILayout.Button(new GUIContent("Red Bomb", "Creates a red dynamite stick."))) {
			Instantiate(redDynamite, new Vector3(5,10), new Quaternion());
		}
		if (GUILayout.Button(new GUIContent("Green Wall", "Creates a green wall block."))) {
			Instantiate(greenWall, new Vector3(0,15), new Quaternion());
		}
		if (GUILayout.Button(new GUIContent("Green Bomb", "Creates a green dynamite stick."))) {
			Instantiate(greenDynamite, new Vector3(5,10), new Quaternion());
		}
		if (GUILayout.Button(new GUIContent("Blue Wall", "Creates a blue wall block."))) {
			Instantiate(blueWall, new Vector3(5,15), new Quaternion());
		}
		if (GUILayout.Button(new GUIContent("Blue Bomb", "Creates a blue dynamite stick."))) {
			Instantiate(blueDynamite, new Vector3(5,10), new Quaternion());
		}

		GUILayout.EndHorizontal();
		
		
		
		GUILayout.BeginHorizontal();
			GUILayout.Label("Various Objects");	
		GUILayout.EndHorizontal();
		
		GUILayout.BeginHorizontal();
		/*if (GUILayout.Button(new GUIContent("Garfield", "Creates a Garfield block."))) {
			//Debug.Log("resetting");
			//gameObject.SendMessage("OnRestartServer");	
			//logs.Clear();
			Instantiate(garf);
			garf.rigidbody2D.position = new Vector2(0, 20f);
		}*/
		if (GUILayout.Button(new GUIContent("Trigger Box", "Creates a trigger box."))) {
			Instantiate(triggerBox, new Vector2(10f, 10f), new Quaternion());
		}
		if (GUILayout.Button(new GUIContent("Health Pack", "Creates a health pack."))) {
			Instantiate(healthPack, new Vector2(-10f, 5f), new Quaternion());
		}
		/*if (GUILayout.Button(new GUIContent("Double health pack", "Creates a double health pack."))) {
			GameObject t = Instantiate(doubleHealthPack) as GameObject;
			t.rigidbody2D.position = new Vector2(0f, 0f);
		}*/
		if (GUILayout.Button(new GUIContent("Injured man", "Creates an injured man."))) {
			Instantiate(injuredMan, new Vector2(15f, 5f), new Quaternion());
		}
		GUILayout.EndHorizontal();

		
		
		
		GUILayout.BeginHorizontal();
		GUILayout.Label("Rewards / Punishments");	
		GUILayout.EndHorizontal();
		
		GUILayout.BeginHorizontal();
		if (GUILayout.Button(new GUIContent("Apple", "Creates an apple."))) {
			Instantiate(apple, new Vector2(5, 15f), new Quaternion());
		}
		if (GUILayout.Button(new GUIContent("Steak", "Creates a steak."))) {
			Instantiate(steak, new Vector3(0,10), new Quaternion());
		}
		if (GUILayout.Button(new GUIContent("Bacon", "Creates bacon."))) {
			Instantiate(bacon, new Vector2(5, 15f), new Quaternion());
		}
		if (GUILayout.Button(new GUIContent("Pill", "Creates a good pill."))) {
			Instantiate(pill, new Vector3(0,10), new Quaternion());
		}
		if (GUILayout.Button(new GUIContent("Red Pill", "Creates a bad pill."))) {
			Instantiate(redPill, new Vector3(0,10), new Quaternion());
		}
		if (GUILayout.Button(new GUIContent("Poison Bottle", "Creates a poisonous bottle."))) {
			Instantiate(poison, new Vector2(5, 15f), new Quaternion());
		}
		GUILayout.EndHorizontal();
		
						
		// Allow the window to be dragged by its title bar.
		GUI.DragWindow(windowRect);
	}
	
	/*
	/// <summary>
	/// Records a log from the log callback.
	/// </summary>
	/// <param name="message">Message.</param>
	/// <param name="stackTrace">Trace of where the message came from.</param>
	/// <param name="type">Type of message (error, exception, warning, assert).</param>
	void HandleLog (string message, string stackTrace, LogType type)
	{
		logs.Insert(0, new Log() {
			message = message,
			stackTrace = stackTrace,
			type = type,
		});
	}*/
}