using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Text;
using System;

/// <summary>
/// A console to display Unity's debug logs in-game. Credit goes to: https://gist.github.com/mminer/975374
/// </summary>
public class Console : MonoBehaviour
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
	public KeyCode toggleKey = KeyCode.BackQuote;
	
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
	//Rect titleBarRect = new Rect(0,0,1,1);//margin, margin, Screen.width - (margin * 2), 200);
	GUIContent clearLabel = new GUIContent("Clear", "Clear the contents of the console.");
	GUIContent exportLog = new GUIContent("Export Log", "Exports log file");
	GUIContent collapseLabel = new GUIContent("Collapse", "Hide repeated messages.");
	string logFileName = "log.txt";
	
	void OnEnable ()
	{
		Application.RegisterLogCallback(HandleLog);
		//Debug.Log("registered");
	}
	
	void OnDisable ()
	{
		//Debug.Log("disabled");
		Application.RegisterLogCallback(null);
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
		
		//titleBarRect = new Rect(windowRect.x, windowRect.y, windowRect.width, windowRect.height);
		
		Event e = Event.current;
		if (e.keyCode == KeyCode.Escape) show = false;
		else
			windowRect = GUILayout.Window(123456, windowRect, ConsoleWindow, "Console");
		
	}

	
	/// <summary>
	/// A window that displayss the recorded logs.
	/// </summary>
	/// <param name="windowID">Window ID.</param>
	void ConsoleWindow (int windowID)
	{
		scrollPosition = GUILayout.BeginScrollView(scrollPosition);
		
		StringBuilder sb = new StringBuilder();
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
			//GUILayout.Label(log.message);
			//GUILayout.TextArea(log.message, "Label");
			sb.AppendLine(log.message);
		}
		GUILayout.TextArea(sb.ToString(), "Label");
		
		GUILayout.EndScrollView();
		
		GUI.contentColor = Color.white;
		
		GUILayout.BeginHorizontal();
		
		if (GUILayout.Button(clearLabel)) {
			logs.Clear();
		}
		
		if (GUILayout.Button(exportLog)) {
			bool writeOk = true;
			try {
				StreamWriter sw = new StreamWriter(logFileName);
				foreach (Log l in logs)
					sw.WriteLine(l.message);
				sw.Close();
			}
			catch(IOException I) {
				GlobalVariables.messageDisplay.showMessage("Error exporting log: " + I.ToString());
				writeOk = false;
			}
			
			if (writeOk)
				GlobalVariables.messageDisplay.showMessage("Logs exported to " + logFileName);	
		}
		
		collapse = GUILayout.Toggle(collapse, collapseLabel, GUILayout.ExpandWidth(false));
		
		GUILayout.EndHorizontal();
		
		// Allow the window to be dragged by its title bar.
		GUI.DragWindow(windowRect);//titleBarRect);
		//GUI.DragWindow(titleBarRect);
	}
	
	/// <summary>
	/// Records a log from the log callback.
	/// </summary>
	/// <param name="message">Message.</param>
	/// <param name="stackTrace">Trace of where the message came from.</param>
	/// <param name="type">Type of message (error, exception, warning, assert).</param>
	void HandleLog (string message, string stackTrace, LogType type)
	{
		//Debug.Log("received log msg");
		logs.Insert(0, new Log() {
			message = System.DateTime.Now.ToLongTimeString() + "  " + message,
			stackTrace = stackTrace,
			type = type,
		});
	}
}