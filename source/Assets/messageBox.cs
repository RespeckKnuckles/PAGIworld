using System.Collections.Generic;
using UnityEngine;

public class messageBox : MonoBehaviour
{
	Vector2 scrollPosition;
	public bool show = false;
	bool collapse = true;
	int msgWidth = 200, msgHeight = 100;
	string displayMsg = "", displayTitle = "";
	
	enum msgType {msgBox, msgBoxOptions, textInput, fileLoad, fileSave};
	msgType currMsgType = msgType.msgBox;
	
	const int margin = 20;
	
	Rect windowRect;
	Rect titleBarRect = new Rect(0, 0, 100, 20);
	
	
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
	
	void Update ()
	{
	}
	
	public delegate void ReturnFromMsgBox(string s);
	ReturnFromMsgBox returnFromMsgBox = null;
	
	public void showMessage(string msg, ReturnFromMsgBox r=null, string title="Attention")
	{
		displayMsg = msg;
		displayTitle = title;
		returnFromMsgBox = r;
		currMsgType = msgType.msgBox;
		show = true;
	}
	
	public void showTextMessageBox(string msg, ReturnFromMsgBox r, string defaultTxt="enter text here", string title="Attention")
	{
		displayMsg = msg;
		displayTitle = title;
		returnFromMsgBox = r;
		currMsgType = msgType.textInput;
		stringToReturn = defaultTxt;
		show = true;
	}
	
	/// <summary>
	/// show a message box with multiple buttons as options
	/// </summary>
	/// <param name="msg">Message.</param>
	/// <param name="msgOptions">Message options.</param>
	public void showMessageOptions(string msg, List<string> msgOptions, ReturnFromMsgBox r, string title="Attention")
	{
		displayMsg = msg;
		this.msgOptions = msgOptions;
		returnFromMsgBox = r;
		displayTitle = title;
		currMsgType = msgType.msgBoxOptions;
		show = true;
	}
	List<string> msgOptions;
	
	/*public void showFileLoadBox(FileBrowser.FinishedCallback callback)
	{
	}*/
	
	void OnGUI ()
	{
		if (!show) {
			return;
		}
		
		windowRect = GUILayout.Window(1222, 
			new Rect((Screen.width-msgWidth)/2f, (Screen.height-msgHeight)/2f, msgWidth*1f, msgHeight*1f),
			ConsoleWindow, displayTitle);
	}
	
	
	string stringToReturn = "Enter msg here";
	/// <summary>
	/// A window that displayss the recorded logs.
	/// </summary>
	/// <param name="windowID">Window ID.</param>
	void ConsoleWindow (int windowID)
	{	
		GUILayout.BeginHorizontal();
		GUILayout.Label(displayMsg);
		GUILayout.EndHorizontal();
		
		if (currMsgType == msgType.msgBox)
		{
			GUILayout.BeginHorizontal();
			if (GUILayout.Button(new GUIContent("OK"))) {
				show = false;
				//Destroy(gameObject);
				if (returnFromMsgBox!=null)
					returnFromMsgBox("OK_pressed");
			}
			if (GUILayout.Button(new GUIContent("Cancel"))) {
				show = false;
				//Destroy(gameObject);
				if (returnFromMsgBox!=null)
					returnFromMsgBox("Cancel_pressed");
			}
			GUILayout.EndHorizontal();
		}
		else if (currMsgType == msgType.textInput)
		{
			GUILayout.BeginHorizontal();
			stringToReturn = GUILayout.TextField(stringToReturn);
			GUILayout.EndHorizontal();
		
			GUILayout.BeginHorizontal();
			if (GUILayout.Button(new GUIContent("OK"))) {
				show = false;
				//Destroy(gameObject);
				if (returnFromMsgBox!=null)
					returnFromMsgBox(stringToReturn);
			}
			if (GUILayout.Button(new GUIContent("Cancel"))) {
				show = false;
				//Destroy(gameObject);
				if (returnFromMsgBox!=null)
					returnFromMsgBox("Cancel_pressed");
			}
			GUILayout.EndHorizontal();
		}
		else if (currMsgType == msgType.msgBoxOptions)
		{
			GUILayout.BeginHorizontal();
			foreach (string s in msgOptions)
			{
				if (GUILayout.Button(new GUIContent(s))) {
					show = false;
					if (returnFromMsgBox!=null)
						returnFromMsgBox(s);
				}
			}
			GUILayout.EndHorizontal();
		}
		
		// Allow the window to be dragged by its title bar.
		GUI.DragWindow(titleBarRect);
		
	}
}