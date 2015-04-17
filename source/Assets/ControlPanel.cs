using System.Collections.Generic;
using UnityEngine;
using System.IO;

/// <summary>
/// A console to display Unity's debug logs in-game. Credit goes to: https://gist.github.com/mminer/975374
/// </summary>
public class ControlPanel : MonoBehaviour
{
	struct Log
	{
		public string message;
		public string stackTrace;
		public LogType type;
	}
	
	/// <summary>
	/// The hotkey to show and hide the console window.
	/// </summary>
	public KeyCode toggleKey = KeyCode.Tab;
	
	
	List<Log> logs = new List<Log>();
	Vector2 scrollPosition;
	bool show;
	bool collapse = true;
	
	// Visual elements:
	
	static readonly Dictionary<LogType, Color> logTypeColors = new Dictionary<LogType, Color>()
	{
		{ LogType.Assert, Color.white },
		{ LogType.Error, Color.red },
		{ LogType.Exception, Color.red },
		{ LogType.Log, Color.white },
		{ LogType.Warning, Color.yellow },
	};
	
	const int margin = 20;
	
	Rect windowRect = new Rect(margin, margin, Screen.width - (margin * 2), Screen.height - (margin * 2));
	Rect titleBarRect = new Rect(0, 0, 100, 20);
	Rect viewControlsWindowRect = new Rect(Screen.width - (margin + 200), margin,200,200);//Screen.height - (margin + 400), 200, 400);// margin, margin, Screen.width - (margin * 2), Screen.height - (margin * 2));
	Rect viewControlsTitleBarRect = new Rect(0, 0, 5, 5);
	//GUIContent clearLabel = new GUIContent("Clear", "Clear the contents of the console.");
	//GUIContent collapseLabel = new GUIContent("Collapse", "Hide repeated messages.");
	
	public static void loadFileDialog(string defaultDir = "")
	{
		GlobalVariables.messageDisplay.showTextMessageBox("Type the path to the .TSK file you want to load",
		                                                  new messageBox.ReturnFromMsgBox(loadFileDialog_callback), defaultDir, "Load File");
	}
	
	/// <summary>
	/// used by loadFileDialog
	/// </summary>
	/// <param name="dir">Dir.</param>
	static void loadFileDialog_callback(string dir)
	{
		Debug.Log("loaded file " + dir);
		if (dir=="Cancel_pressed")
			return;
		//remove all world objects currently in scene (except for body/hands)
		worldObject[] goArray = UnityEngine.MonoBehaviour.FindObjectsOfType(typeof(worldObject)) as worldObject[];
		List<string> doNotRemove = new List<string>() { "leftHand", "rightHand", "mainBody" };
		foreach (worldObject obj in goArray)
		{
			if (!doNotRemove.Contains(obj.objectName))
			{
				Destroy(obj.gameObject);
			}
		}
		FileSaving g = new FileSaving(dir); //loads the file
	}
	
	public static void saveFileDialog(string defaultDir = "")
	{
		GlobalVariables.messageDisplay.showTextMessageBox("Type the path to the .TSK file you want to save",
		                                                  new messageBox.ReturnFromMsgBox(saveFileDialog_callback), defaultDir, "Save Task");
	}
	
	/// <summary>
	/// used by saveFileDialog
	/// </summary>
	/// <param name="dir">Dir.</param>
	static void saveFileDialog_callback(string s)
	{
		if (s!="" && s!=null)
		{
			if (s=="Cancel_pressed")
				return;
			bool noErrors = true;
			try {
				FileSaving f = new FileSaving();
				f.taskDescriptor = "Default task descriptor";
				Debug.Log("Saving to file " + s);
				f.writeToFile(s);
			}
			catch(System.Exception e)
			{
				GlobalVariables.messageDisplay.showMessage("There was an error: " + e.ToString(), null, "Error");
				noErrors = false;
			}
			if (noErrors)
				GlobalVariables.messageDisplay.showMessage("File Saved Successfully!", null, "Awesome!");
		}
	}
		
		// Use this for initialization
	void Start () {
	}
	
	void OnEnable ()
	{
		//Application.RegisterLogCallback(HandleLog);
	}
	
	void OnDisable ()
	{
		//Application.RegisterLogCallback(null);
	}
	
	void testFn(string s)
	{
		//return "test";
		Debug.Log(s);
	}
	
	void Update ()
	{
		if (Input.GetKeyDown(toggleKey)) {
			windowRect = new Rect(margin, margin, Screen.width - (margin * 2), Screen.height - (margin * 2));
			show = !show;
		}
		if (Input.GetKeyDown(KeyCode.Escape))
			Screen.fullScreen = false;
			
		/*if (Input.GetKeyDown(KeyCode.Minus))
		{
			//GlobalVariables.messageDisplay.showTextMessageBox("hi", new messageBox.ReturnFromMsgBox(testFn), "t");
			List<string> ls = new List<string>{"option1", "option2", "option3"};
			GlobalVariables.messageDisplay.showMessageOptions("pick this", ls, testFn, "Look!");
		}*/
		
		bool shiftdown = (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift));
		bool ctrldown = (Input.GetKey (KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl));
		if (shiftdown && ctrldown && Input.GetKeyDown(KeyCode.R))
		{
			Debug.Log("Received keyboard input Shift+Ctrl+R; restarting tcp/ip connection...");
			gameObject.SendMessage("OnRestartServer");
		}
	}
	
	void OnGUI ()
	{
		if (GlobalVariables.viewControlsVisible)
			viewControlsWindowRect = GUILayout.Window(12112, viewControlsWindowRect, ViewPortWindow, "View Port");

		if (!show) {
			return;
		}
		
		Event e = Event.current;
		if (e.keyCode == KeyCode.Escape) show = false;
		else
			windowRect = GUILayout.Window(12345, windowRect, ConsoleWindow, "Controls");
	}

	void ViewPortWindow(int windowID)
	{
		GUI.contentColor = Color.white;

		//GUILayout.BeginHorizontal();
		if (GUI.Button(new Rect(0, 0, 18, 18), new GUIContent("x")))
		//if (GUILayout.Button(new GUIContent("X"))) {
			//Debug.Log("resetting");
			GlobalVariables.viewControlsVisible = false;
		//}
		//GUILayout.EndHorizontal();

		GUILayout.BeginHorizontal();
		if (GUILayout.Button(new GUIContent("Zoom -")))
			camera.orthographicSize = (camera.orthographicSize >= 89.3f)?125f:camera.orthographicSize + camera.orthographicSize*0.4f;
		if (GUILayout.Button(new GUIContent("Zoom +")))
			camera.orthographicSize = (camera.orthographicSize <= 41.7f)?20f:camera.orthographicSize - camera.orthographicSize*0.4f;
		GUILayout.EndHorizontal();
		
		float scrollFactor = 5f; //larger means it scrolls less

		GUILayout.BeginHorizontal();
		if (GUILayout.Button(new GUIContent("/\\")))
			camera.transform.Translate(0, camera.orthographicSize/scrollFactor, 0);
		GUILayout.EndHorizontal();

		GUILayout.BeginHorizontal();
		if (GUILayout.Button(new GUIContent("<")))
			camera.transform.Translate(-(camera.orthographicSize/scrollFactor), 0, 0);
		if (GUILayout.Button(new GUIContent(">")))
			camera.transform.Translate(camera.orthographicSize/scrollFactor, 0, 0);
		GUILayout.EndHorizontal();
		
		GUILayout.BeginHorizontal();
		if (GUILayout.Button(new GUIContent("\\/")))
			camera.transform.Translate(0, -camera.orthographicSize/scrollFactor, 0);
		GUILayout.EndHorizontal();

		GUILayout.BeginHorizontal();
		GlobalVariables.centerCamera = GUILayout.Toggle(GlobalVariables.centerCamera, "Keep PAGI Guy Centered");
		GUILayout.EndHorizontal();

		GUILayout.BeginHorizontal();
		if (GUILayout.Button(new GUIContent("test")))
		{
			worldObject w = Instantiate(emptyblock, new Vector3(10f,10f), new Quaternion()) as worldObject;
			Texture2D tex = null;
			byte[] fileData;
			string fileName = "bill.jpeg";
			if (File.Exists(fileName))     {
				fileData = File.ReadAllBytes(fileName);
				tex = new Texture2D(5, 5);
				tex.LoadImage(fileData); //..this will auto-resize the texture dimensions.
				
				Sprite newSprite = new Sprite();
				newSprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0, 0), 50f);
				w.GetComponent<SpriteRenderer>().sprite = newSprite;
				BoxCollider2D b = w.GetComponent<BoxCollider2D>();
				b.center = newSprite.bounds.center;
				b.size = newSprite.bounds.size;
				//Debug.Log("w " + newSprite.bounds + " h " + b.bounds);
            }
			else
				Debug.Log("file " + fileName + " not found");
		}
        GUILayout.EndHorizontal();

		GUI.DragWindow(windowRect);
	}
	public worldObject emptyblock;
	
	string newPortStr = GlobalVariables.portNumber.ToString();
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
		
		
		if (GUILayout.Button(new GUIContent("Reset Socket Connection (you can also press Shift+Ctrl+R)", "Reset the socket connection and accept new connections."))) {
			//Debug.Log("resetting");
			gameObject.SendMessage("OnRestartServer");	
			//logs.Clear();
		}
		
		/*GUILayout.Label("Port number:");
		newPortStr = GUILayout.TextField(newPortStr);
		int tmpPort = 42209;
		if (int.TryParse(newPortStr, out tmpPort))
			GlobalVariables.portNumber = tmpPort;
		else
			newPortStr = GlobalVariables.portNumber.ToString();*/
		GUILayout.EndHorizontal();
		
		/*GUILayout.BeginHorizontal();
		if (GUILayout.Button(new GUIContent("Return to main screen"))) {
			//Debug.Log("resetting");
			//gameObject.SendMessage("OnRestartServer");
			gameObject.SendMessage("Disconnect");
			Application.LoadLevel("MainScreen");
			//logs.Clear();
		}
		GUILayout.Label("\t\tExits this scene and returns to the main screen.");
		GUILayout.EndHorizontal();*/
		
		//GUILayout.BeginHorizontal();
		//GUILayout.EndHorizontal();
		
		GUILayout.BeginHorizontal();
		if (GUILayout.Button(new GUIContent("Save this task to file"))) {
			saveFileDialog("myTask.tsk");
		}
		if (GUILayout.Button(new GUIContent("Load task from file"))) {
			loadFileDialog("myTask.tsk");
		}
		GUILayout.EndHorizontal();
		
		
		GUILayout.BeginHorizontal();
		GUILayout.Label("\nActive\nStates:");
		//construct string of all states
		string st = "";
		foreach (State s in GlobalVariables.activeStates.getCopy())
			st += s.stateName + ", ";
		GUI.TextArea(new Rect(85,85,800,100), st, 200);
		GUILayout.EndHorizontal();
		
		GUILayout.Space(45);
		
		GUILayout.BeginHorizontal();
		GUILayout.Label("\nActive\nReflexes:");
		//construct string of all reflexes
		st = "";
		foreach (Reflex s in GlobalVariables.activeReflexes.getCopy())
			st += s.reflexName + ", ";
		GUI.TextArea(new Rect(85,185,800,100), st, 200);
		GUILayout.EndHorizontal();
		
		GUILayout.Space(65);

		GUILayout.BeginHorizontal();
		if (GUILayout.Button(new GUIContent("Reset States")))
		{
			GlobalVariables.activeStates.TryReset();
		}
		if (GUILayout.Button(new GUIContent("Reset Reflexes")))
		{
			GlobalVariables.activeReflexes.TryReset();
		}
		/*if (GUILayout.Button(new GUIContent("test")))
		{
			GlobalVariables.messageQueue.Add(AIMessage.fromString("setReflex,r1,s1\n"));
			GlobalVariables.messageQueue.Add(AIMessage.fromString("setReflex,r2,s1\n"));
			GlobalVariables.messageQueue.Add(AIMessage.fromString("setReflex,r3,s1\n"));
			GlobalVariables.messageQueue.Add(AIMessage.fromString("setReflex,r4,s1\n"));
		}*/
		GUILayout.EndHorizontal();
		
		GUILayout.BeginHorizontal();
		GlobalVariables.sendNotificationOnReflexFirings = GUILayout.Toggle(GlobalVariables.sendNotificationOnReflexFirings, "Send notifications when reflexes fire?");
		
		GlobalVariables.spokenCommandFieldVisible = GUILayout.Toggle(GlobalVariables.spokenCommandFieldVisible, "Show command textbox");
		
		GlobalVariables.viewControlsVisible = GUILayout.Toggle(GlobalVariables.viewControlsVisible, "Show view controls");
		GUILayout.EndHorizontal();

		GUILayout.BeginHorizontal();
		GlobalVariables.showDetailedVisionMarkers = GUILayout.Toggle(GlobalVariables.showDetailedVisionMarkers, "Show markers ('*') for detailed vision sensors?");
		//GUILayout.EndHorizontal();

		//GUILayout.BeginHorizontal();
		GlobalVariables.showPeripheralVisionMarkers = GUILayout.Toggle(GlobalVariables.showPeripheralVisionMarkers, "Show markers ('o') for peripheral vision sensors?");
		GUILayout.EndHorizontal();
		
		GUILayout.BeginHorizontal();
		//get the main guy
		bodyController[] mainGuy = UnityEngine.MonoBehaviour.FindObjectsOfType(typeof(bodyController)) as bodyController[];
		GUILayout.Label("Current coordinates of PAGI guy: " + mainGuy[0].rigidbody2D.position.ToString() + "\t\t");
		if (GUILayout.Button(new GUIContent("Reset PAGI guy to (0,0)"))) {
			mainGuy[0].rigidbody2D.position = new Vector2(0,0);
			leftHandController[] leftHand = UnityEngine.MonoBehaviour.FindObjectsOfType(typeof(leftHandController)) as leftHandController[];
			rightHandController[] rightHand = UnityEngine.MonoBehaviour.FindObjectsOfType(typeof(rightHandController)) as rightHandController[];
			leftHand[0].rigidbody2D.position = new Vector2(-1f, 1f);
			rightHand[0].rigidbody2D.position = new Vector2(1f, 1f);	
			mainGuy[0].rigidbody2D.rotation = 0;
			leftHand[0].rigidbody2D.rotation = 0;
			rightHand[0].rigidbody2D.rotation = 0;
			mainGuy[0].rigidbody2D.velocity = Vector2.zero;
			leftHand[0].rigidbody2D.velocity = Vector2.zero;
			rightHand[0].rigidbody2D.velocity = Vector2.zero;
			mainGuy[0].rigidbody2D.AddForce(Vector2.zero); //forces update of rotation
		}
		GUILayout.EndHorizontal();
		
		GUILayout.BeginHorizontal();
		GUILayout.Label("Speed (0=paused, 1=normal speed):");	
		//GUILayout.EndHorizontal();
		//GUILayout.BeginHorizontal();
		if (GUILayout.Button(new GUIContent("<", "Slows down time"))) {
			Time.timeScale = Mathf.Round(10f*Mathf.Max(0f, Time.timeScale-0.1f))/10f;
		}
		GUILayout.Label("  "+Time.timeScale.ToString());
		if (GUILayout.Button(new GUIContent(">", "Speeds up time"))) {
			Time.timeScale = Mathf.Round(10f*Mathf.Min(10f, Time.timeScale+0.1f))/10f;
		}
		//GUILayout.EndHorizontal();
		
		//GUILayout.BeginHorizontal();
		GUILayout.Label("\t\tGravity:");	
		if (GUILayout.Button(new GUIContent("<", "Slows down time"))) {
			Physics2D.gravity = new Vector2(0, Physics2D.gravity.y-1);
		}
		GUILayout.Label("  "+Physics2D.gravity.y);
		if (GUILayout.Button(new GUIContent(">", "Speeds up time"))) {
			Physics2D.gravity = new Vector2(0, Physics2D.gravity.y+1);
		}
		if (GUILayout.Button(new GUIContent("zero", "Sets gravity to zero"))) {
			Physics2D.gravity = new Vector2(0, 0);
		}
		if (GUILayout.Button(new GUIContent("normal", "Sets gravity to its normal level"))) {
			Physics2D.gravity = new Vector2(0, -9.81f);
		}
		GUILayout.EndHorizontal();
		
		/*GUILayout.BeginHorizontal();
		PhysicsMaterial2D pm2d = mainGuy[0].rigidbody2D.collider2D.sharedMaterial;
		GUILayout.Label("World friction (is normal):");	
		//GUILayout.EndHorizontal();
		//GUILayout.BeginHorizontal();
		if (GUILayout.Button(new GUIContent("<", "Slows down time"))) {
			pm2d.friction -= 0.1f;
		}
		GUILayout.Label("  " + pm2d.friction.ToString());
		if (GUILayout.Button(new GUIContent(">", "Speeds up time"))) {
			pm2d.friction += 0.1f;
		}
		GUILayout.EndHorizontal();*/
		
		
		// Allow the window to be dragged by its title bar.
		GUI.DragWindow(windowRect);
		
	}
	public worldObject garf;
	
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
	}
}