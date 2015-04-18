using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Text.RegularExpressions;

public class GlobalVariables : MonoBehaviour {

	public static bool androidBuild = true;
	public static string versionNumber = "0.1.7";
	
	public static int portNumber = 42209;

	/// <summary>
	/// true if the next click the mouse makes will delete an object
	/// </summary>
	public static bool mouseDeleting = false;
	public Texture2D trashTexture;
	public Camera visionPreviewCam;
	//public bodyController mainBody;
	//public float timeScale = 1;
	
	public static ThreadSafeList<State> activeStates = new ThreadSafeList<State>();
	public static ThreadSafeList<Reflex> activeReflexes = new ThreadSafeList<Reflex>();
	
	public static ThreadSafeList<AIMessage> messageQueue = new ThreadSafeList<AIMessage>();
	public static ThreadSafeList<string> outgoingMessages = new ThreadSafeList<string>();
	
	public static bool spokenCommandFieldVisible = false; //whether or not the command box is visible
	public static bool sendNotificationOnReflexFirings = true; //when reflexes fire, should messages be sent?
	public static bool viewControlsVisible = false;
	public static bool centerCamera = false; //keeps camera centered on PAGI guy
	public static bool showDetailedVisionMarkers = false;	
	public static bool showPeripheralVisionMarkers = false;
	
	/// <summary>
	/// set to true if you don't want to display the intro dialog
	/// </summary>
	public bool debugMode = false;
	
	
	public static messageBox messageDisplay;
	
	/*public GameObject mainBody, rightHand, leftHand;
	public GameObject getMainBody() { return mainBody; }
	public GameObject getRightHand() { return rightHand; }
	public GameObject getLeftHand() { return leftHand; } this doesn't work */
	
	
	// Use this for initialization
	void Start () {
		messageDisplay = gameObject.AddComponent<messageBox>();
		
		
		
		if (debugMode)
			return;
		List<string> choices = new List<string>{"Start default task", "Load task from file", "See tutorial"};
		messageDisplay.showMessageOptions("Welcome to PAGI World! What would you like to do?",
                  choices, new messageBox.ReturnFromMsgBox(startPromptResponder), "Welcome!");
                  
		
	}
	
	void Awake() {
		QualitySettings.vSyncCount = 0;
		Application.targetFrameRate = 70;
	}
	
	/// <summary>
	/// callback function used for the initial prompt that comes up when you first open program
	/// </summary>
	/// <param name="response">Response.</param>
	void startPromptResponder(string response)
	{
		List<string> choices = new List<string>{"Start default task", "Load task from file", "See tutorial"};
		if (response==choices[1]) //load task from file
		{
			ControlPanel.loadFileDialog("/");
		}
		else if (response==choices[2]) //see tutorial
		{
			messageDisplay.showMessage("Go to http://rair.cogsci.rpi.edu for tutorial info");
			//UnityEditor.EditorUtility.DisplayDialog("No", "This isn't created yet", "Ok...:(");
		}//else do nothing
	}
	
	
	
	public void setTimeScale(float newVal)
	{
		Time.timeScale = newVal;
	}
	
	string spokenCommand = "send command here";
	void OnGUI()
	{
		if (spokenCommandFieldVisible)
		{
			spokenCommand = GUI.TextField(new Rect(0, 0, 200, 20), spokenCommand, 200);
			if (Event.current.Equals(Event.KeyboardEvent("None")))
			{
				//Debug.Log("Pressed Enter");
				//DoneTyping();
				outgoingMessages.Add("SC," + spokenCommand + "\n");
				spokenCommand = "";
			}
		}
	}
	
	// Update is called once per frame
	void Update () {
		//check if any objects are off scren
		worldObject[] goArray = UnityEngine.MonoBehaviour.FindObjectsOfType(typeof(worldObject)) as worldObject[];
		List<string> doNotRemove = new List<string>() { "leftHand", "rightHand", "mainBody" };
		foreach (worldObject obj in goArray)
		{
			if (obj.transform.hasChanged)
			{
				//is it out of bounds?
				if (obj.transform.position.x < -195f || obj.transform.position.x > 194f || obj.transform.position.y < -122f)// || obj.transform.position.y > 126f)
				{
					if (!doNotRemove.Contains(obj.objectName))
					{
						Destroy(obj.gameObject);
					}
					else
					{
						obj.rigidbody2D.velocity = new Vector2(0,0);
					}
				}
				obj.transform.hasChanged = false;
			}
		}

		//check all activeStates to see if any need to expire
		List<State> toRemove = new List<State>();
		List<State> activeStates_copy = activeStates.getCopy();
		foreach (State s in activeStates_copy)
		{
			if (DateTime.Now - s.startTime >= s.lifeTime)
			{
				//Debug.Log("removing");
				toRemove.Add(s);
			}
			//Debug.Log("state " + s.stateName + ": " + (DateTime.Now - s.startTime).ToString() + ", " + s.lifeTime.Seconds.ToString());
		}
		foreach (State s in toRemove)
			activeStates.TryRemove(s);
		
		/*if (timeScale!=Time.timeScale)
		{
			setTimeScale(timeScale);
			timeScale = Time.timeScale; //in case it was out of range
		}*/
		if (Input.GetKeyDown(KeyCode.Delete))
		{
			if (mouseDeleting)
			{
				mouseDeleting = false;
				Cursor.SetCursor(null, Vector2.zero, CursorMode.ForceSoftware);
				Debug.Log("delete off");
			}
			else
			{
				mouseDeleting = true;
				Cursor.SetCursor(trashTexture, new Vector2(-40f, 10f), CursorMode.ForceSoftware);
				Debug.Log("delete");
			}
		}
	}
}


public sealed class ThreadSafeList<T> {
	private List<T> m_list = new List<T>();
	private object m_lock = new object();
	
	public void Add(T value) {
		lock (m_lock) {
			m_list.Add(value);
		}
	}
	public bool TryRemove(T value) {
		lock (m_lock) {
			return m_list.Remove(value);
		}
	}
	public bool TryRemoveAt(int index) {
		lock (m_lock) {
			m_list.RemoveAt(index);
			return true;
		}
	}
	public bool TryGet(int index, out T value) {
		lock ( m_lock ) {
			if( index < m_list.Count ) {
				value = m_list[index];
				return true;
			}
			value = default(T);
			return false;
		}
	}
	public int Count() {
		return m_list.Count;
	}

	/// <summary>
	/// Erases the list and replaces it with an empty one
	/// </summary>
	public void TryReset() {
		lock (m_lock) {
			m_list = new List<T>();
		}
	}
	
	public List<T> getCopy()
	{
		List<T> toReturn;
		lock(m_lock) {
			toReturn = m_list;
		}
		return toReturn;
	}
}

public class State
{
	public string stateName;
	public DateTime startTime;
	public TimeSpan lifeTime;
	
	public State(string name, TimeSpan life)
	{
		startTime = DateTime.Now;
		stateName = name;
		lifeTime = life;
	}
}

public class Reflex
{
	public abstract class condition
	{
	}
	public class stateCondition:condition
	{
		public string stateName;
		public bool isNegated = false;
	}
	public class sensoryCondition:condition
	{
		public string sensorName;
		public char operatorType;
		public float value;
	}
	public string reflexName;
	public List<condition> conditions;
	public List<AIMessage> actions;
	
	public void addCondition(string stateName, bool isNegated)
	{
		stateCondition s = new stateCondition();
		s.stateName = stateName;
		s.isNegated = isNegated;
		conditions.Add (s);
	}
	
	public void addCondition(string sensorName, char operatorType, float value)
	{
		sensoryCondition s = new sensoryCondition();
		s.sensorName = sensorName;
		s.operatorType = operatorType;
		s.value = value;
		conditions.Add(s);
	}
	
	public void addAction(AIMessage action)
	{
		actions.Add(action);
	}
	
	public Reflex(string name)
	{
		reflexName = name;
		conditions = new List<condition>();
		actions = new List<AIMessage>();
	}
	
	/// <summary>
	/// Checks the conditions to see if they're activated. If so, fire all actions, send message
	/// to user, and return true. Otherwise, false is returned.
	/// </summary>
	//public bool checkConditions();
}

/// <summary>
/// a node in a function tree
/// </summary>
public class fnNode
{
	public string val;
	public float fVal = 0.0f;
	public fnNode leftChild = null;
	public fnNode rightChild = null;
	public delegate float stringToVal_delegate(string s);
	public bool isEmpty = true; //true if this node has not yet been marked by the parsing function
	public bool isNegationSpecial = false;//used by parsing fn 
	
	public fnNode(string val)
	{
		this.val = val;
	}
	
	public fnNode(float fVal)
	{
		this.val = "";
		this.fVal = fVal;
	}
	
	public string toString()
	{
		string s = "";
		if (leftChild!=null)
			s += "(" + leftChild.toString();
		if (val=="")
			s += fVal.ToString();
		else
			s += val;
		if (rightChild!=null)
			s += rightChild.toString() + ")";
		return s;
	}
	
	public float evaluate(stringToVal_delegate f)
	{
		//is this a leaf node?
		if (leftChild == null && rightChild == null)
		{
			if (val == "")
				return fVal;
			else
				return f(val);
		}
		else //not a child
		{
			if (val == "+")
				return leftChild.evaluate(f) + rightChild.evaluate(f);
			else if (val == "-")
				return leftChild.evaluate(f) - rightChild.evaluate(f);
			else if (val == "/")
				return leftChild.evaluate(f) / rightChild.evaluate(f);
			else if (val == "*")
				return leftChild.evaluate(f) * rightChild.evaluate(f);
			else if (val == "%")
				return leftChild.evaluate(f) % rightChild.evaluate(f);
			else if (val == "^")
				return (float)Math.Pow((double)leftChild.evaluate(f), (double)rightChild.evaluate(f));
			else
				throw new Exception("I don't recognize this operator type: " + val);
		}
	}
	
	/// <summary>
	/// Evaluates an evaluation tree that is either a float, or
	/// a function wrapped in square brackets.
	/// </summary>
	/// <returns>The float.</returns>
	/// <param name="s">S.</param>
	public static fnNode parseFloat(string s)
	{
		s = s.Trim();
		if (s[0]=='[' && s[s.Length-1]==']')
		{//treat it as a function, parse it
			//parse string, return as fnNode tree
			string[] tokens = Regex.Split(s, @"(?=[-+*/()^%\[\]])|(?<=[-+*/()^%\[\]])");
			List<fnNode> depthTrace = new List<fnNode>(); //traces the descendants of the current node
			fnNode currNode = new fnNode("");
			fnNode rootNode = currNode;
			depthTrace.Add(currNode);
			string operators = "-+/*^%";
			for (int i=2; i<tokens.Length-2; i++)
			{
				string dt = "";
				foreach (fnNode f in depthTrace)
				{
					if (f.val=="")
						dt += ", " + f.fVal.ToString();
					else
						dt += ", " + f.val;
					//if (f == currNode)
					//	dt += "(curr)";
				}
				Debug.Log("depth trace: " + dt);
				Debug.Log("root: " + depthTrace[0].toString());
				Debug.Log("on token " + tokens[i]);
				
				//depthTrace.Add(new fnNode("test"));
				if (tokens[i].Trim()=="")//ignore whitespace
					continue;
				//is this a parenthesis?
				else if (tokens[i].Trim()=="(")
				{
					//push curr to be a left child of a new node, point to its right child
					//fnNode newNode = new fnNode("");
					currNode.leftChild = new fnNode("");
					currNode.rightChild = new fnNode("");
					currNode = currNode.leftChild;
					//depthTrace[depthTrace.Count-1] = newNode;
					depthTrace.Add(currNode);
					//Debug.Log("ignoring");
				}
				else if (tokens[i].Trim()==")")
				{
					//point to parent. If the parent is empty, remove it.
					if (depthTrace.Count < 2)
						throw new Exception("too many closing parentheses in " + s);
					depthTrace.RemoveAt(depthTrace.Count-1);
					currNode = depthTrace[depthTrace.Count-1];
					if (currNode.isEmpty) //remove it
					{
						Debug.Log("is empty node");
						if (currNode.rightChild != null && !currNode.rightChild.isEmpty)
							throw new Exception("closing parenthesis out of place in " + s);
						//otherwise assume the left child is the only one we need to keep
						//find the parent of currNode, if any
						if (depthTrace.Count > 1)
						{
							fnNode parent = depthTrace[depthTrace.Count-2];
							//is currNode the right or left child?
							if (parent.leftChild == currNode)
							{
								parent.leftChild = currNode.leftChild;
								depthTrace[depthTrace.Count-1] = currNode.leftChild;
								currNode = currNode.leftChild;
							}
							else if (parent.rightChild == currNode)
							{
								parent.rightChild = currNode.leftChild;
								depthTrace[depthTrace.Count-1] = currNode.leftChild;
								currNode = currNode.leftChild;
							}
							else
								throw new Exception("error parsing: depthTrace was incorrect!");
						}
						
					}
				}
				else if (operators.Contains(tokens[i].Trim()))
				{
					//if curr is nonempty and has a parent, then fill parent with this operator and move to its right child
					//if curr is nonempty and there is no parent, create one, then fill it with this operator and move to its right child
					//if curr is empty, then this must be the negation operator (error otherwise)
					if (currNode.isEmpty)
					{
						if (tokens[i].Trim() != "-")
							throw new Exception("error parsing: misplaced operator " + tokens[i]);
						currNode.val = "*";
						currNode.isNegationSpecial = true;
						currNode.isEmpty = false;
						currNode.leftChild = new fnNode(-1f);
						currNode.leftChild.isEmpty = false;
						currNode.rightChild = new fnNode("");
						currNode = currNode.rightChild;
						depthTrace.Add(currNode);
					}
					else
					{
						fnNode parent;
						if (depthTrace.Count < 2) //there is no parent
						{
							parent = new fnNode(tokens[i].Trim());
							parent.isEmpty = false;
							parent.leftChild = currNode;
							parent.rightChild = new fnNode("");
							currNode = parent.rightChild;
							depthTrace[depthTrace.Count-1] = parent;
							depthTrace.Add(currNode);
						}
						else
						{//there is a parent
							parent = depthTrace[depthTrace.Count-2];
							parent.isEmpty = false;
							parent.val = tokens[i].Trim();
							parent.rightChild = new fnNode("");
							currNode = parent.rightChild;
							depthTrace[depthTrace.Count-1] = currNode;
						}
					}
				}
				else 	//it's a word or number
				{
					currNode.isEmpty = false;
					if (float.TryParse(tokens[i], out currNode.fVal))
					{//it's a number
						Debug.Log(tokens[i] + " is a num");
						//currNode.fVal = float.Parse(tokens[i]);
					}
					else
					{//it's not a number
						Debug.Log(tokens[i] + " is not a num");
						currNode.val = tokens[i].Trim();
					}
					//is the parent a negation multiplier? If so move up. This is necessary because
					//putting a '-' in front of numbers to make them negative turns it into a shorthand
					//for multiplying by -1, but there is no end parentheses to correct the pointer later
					while (depthTrace.Count > 1)
					{
						//what is the parent of curr?
						fnNode parent = depthTrace[depthTrace.Count-2];
						//if parent is a negation multiplier, then move curr to it.
						if (parent.isNegationSpecial)
						{
							currNode = parent;
							depthTrace.RemoveAt(depthTrace.Count-1);
						}
						//otherwise, exit this loop
						else
							break;
					}
				}
			}
			//foreach (fnNode f in depthTrace)
			//	Debug.Log("had: " + f.toString());
			return depthTrace[0];
		}
		else //assume it's a float already
			return new fnNode(float.Parse(s));
	}
}

/// <summary>
/// describes the message sent from the external AI controller to control the agent
/// </summary>
public class AIMessage
{
	public enum AIMessageType { sensorRequest, addForce, dropItem, print,
		setState, setReflex, removeReflex, getStates, getReflexes, findObj,
		createItem, addForceToItem, getInfoAboutItem, destroyItem,
		establishConnection, removeConnection, loadTask, other }
	public AIMessageType messageType;
	/// <summary>
	/// For different values of messageType:
	/// sensorRequest, triggerAdd, triggerRemove, addForce - this is the identifier of the sensor or force output to use
	/// establishConnection, removeConnection - this is ignored
	/// </summary>
	public string stringContent;
	/// <summary>
	/// Depending on value of messageType:
	/// triggerAdd: If the sensor specified by stringContent changes by at least floatContent, a notification is sent.
	/// for all else, this is unused
	/// </summary>
	public float floatContent; //optional, only used when a value needs to be specified
	public Vector2 vectorContent;
	public System.Object detail;
	public DateTime timeCreated;
	public fnNode function1=null,function2=null;
	
	public AIMessage() //used for testing purposes only
	{
		messageType = AIMessageType.other;
		stringContent = "";
		floatContent = 0;
		//vectorContent = Vector2.zero;
		detail = null;
		timeCreated = DateTime.Now;
	}
	
	
	public static AIMessage fromString(string s)
	{
		if (s.Trim()=="")
			throw new Exception("ERR: Received string that was nothing but whitespace!");

		string[] clientArgs = s.Split(',');
		AIMessage a = new AIMessage(AIMessage.AIMessageType.other, "ERR: Unrecognized Command. Received string:\"" + s + "\"\n", 100f, "");
		clientArgs[0] = clientArgs[0].Trim();
		if (clientArgs[0]== "sensorRequest")
		{
			a.messageType = AIMessage.AIMessageType.sensorRequest;
			if (clientArgs.Length==2)
				a.stringContent = clientArgs[1];
			else
				throw new Exception("Incorrect # of arguments given in client message: " + s);
		}
		else if (clientArgs[0]=="print")
		{
			a.messageType = AIMessageType.print;
			//the content should be everything following the print, command"
			a.stringContent = s.Substring(s.IndexOf(',')+1);
		}
		else if (clientArgs[0]=="loadTask")
		{
			//FileSaving f = new FileSaving(clientArgs[1]);
			a.messageType = AIMessage.AIMessageType.loadTask;
			if (clientArgs.Length == 2)
				a.stringContent = clientArgs[1];
			else
				throw new Exception("Incorrect # of arguments given in client message: " + s);
		}
		else if (clientArgs[0]=="findObj")
		{
			//FileSaving f = new FileSaving(clientArgs[1]);
			a.messageType = AIMessage.AIMessageType.findObj;
			if (clientArgs.Length == 3)
			{
				a.stringContent = clientArgs[1];
				a.detail = clientArgs[2];
			}
			else
				throw new Exception("Incorrect # of arguments given in client message: " + s);
		}
		else if (clientArgs[0] == "addForce")
		{
			/*addForce,e,v
					e - The code of the effector to send force to.
					v - The amount of force to add to the effector.*/
			a.messageType = AIMessage.AIMessageType.addForce;
			if (clientArgs.Length==3)
			{
				a.stringContent = clientArgs[1];
				//the arguments at indices 2 and 3 can be either float values, or 
				//if they are in square brackets, a function that evaluates to one
				a.function1 = fnNode.parseFloat(clientArgs[2]);
			}
			else if (clientArgs.Length==4)
			{
				a.stringContent = clientArgs[1];
				a.function1 = fnNode.parseFloat(clientArgs[2]);
				a.function2 = fnNode.parseFloat(clientArgs[3]);
				//a.vectorContent = new Vector2(evaluateFloat(clientArgs[2]), evaluateFloat(clientArgs[3]));
			}
			else
				throw new Exception("Incorrect # of arguments given in client message: " + s);
		}
		else if (clientArgs[0] == "dropItem")
		{
			a.messageType = AIMessage.AIMessageType.dropItem;
			if (clientArgs.Length == 4 || clientArgs.Length == 5)
			{
				a.stringContent = clientArgs[1];
				a.vectorContent = new Vector2(float.Parse(clientArgs[2]), float.Parse(clientArgs[3]));
				if (clientArgs.Length==5)
					a.detail = clientArgs[4];
			}
			else
				throw new Exception("Incorrect # of arguments given in client message: " + s);
		}
		else if (clientArgs[0] == "setState")
		{
			a.messageType = AIMessageType.setState;
			if (clientArgs.Length==3)
			{
				a.stringContent = clientArgs[1];
				a.floatContent = float.Parse(clientArgs[2]);
				int stateLife = int.Parse(clientArgs[2]);
				State st;
				if (stateLife == 0) //this is actually a command to kill the state
					st = new State(clientArgs[1], TimeSpan.Zero);
				else if (stateLife < 0)
					st = new State(clientArgs[1], new TimeSpan(Int64.MaxValue));
				else
					st = new State(clientArgs[1], new TimeSpan(0,0,0,0,stateLife));
				a.detail = st;
			}
			else
				throw new Exception("Incorrect # of arguments given in client message: " + s);
		}
		else if (clientArgs[0] == "setReflex")
		{
			a.messageType = AIMessageType.setReflex;
			if (clientArgs.Length==3 || clientArgs.Length==4)
			{
				a.stringContent = clientArgs[1];
				//clientArgs[2] is a list of conditions
				//clientArgs[3] (optional) is a list of actions to execute, each of which should be AIMessages.
				//create reflex object
				Reflex r = new Reflex(clientArgs[1]);
				//parse conditions
				foreach (string strCon in clientArgs[2].Split(new char[]{';'}))
				{
					//each condition is either sensory (sensorAspectCode|operator|value) or state ([-]s)
					if (strCon.Contains("|"))
					{ //treat it as sensory condition
						string[] args = strCon.Split(new char[]{'|'});
						if (args.Length != 3)
							throw new Exception("Incorrect # of arguments in sensory condition " + strCon);
						r.addCondition(args[0], args[1][0], float.Parse(args[2])); 
					}
					else
					{//treat it as state condition
						if (strCon.StartsWith("-"))
							r.addCondition(strCon.Substring(1), true);
						else
							r.addCondition(strCon, false);
					}
				}
				//parse actions
				if (clientArgs.Length > 3)
				{
					foreach (string strAct in clientArgs[3].Split(new char[]{';'}))
					{
						//each is a standard AIMessage except for setReflex.
						string aiMsgString = strAct.Replace('|', ',');
						r.addAction(AIMessage.fromString(aiMsgString));
					}
				}
				a.detail = r;
			}
			else
				throw new Exception("Incorrect # of arguments given in client message: " + s);
			
		}
		else if (clientArgs[0] == "removeReflex")
		{
			a.messageType = AIMessageType.removeReflex;
			if (clientArgs.Length==2)
			{
				a.stringContent = clientArgs[1];
			}
			else
				throw new Exception("Incorrect # of arguments given in client message: " + s);
		}
		else if (clientArgs[0] == "getActiveStates")
		{
			a.messageType = AIMessageType.getStates;
			a.detail = "";
		}
		else if (clientArgs[0] == "getActiveReflexes")
			a.messageType = AIMessageType.getReflexes;
		else if (clientArgs[0] == "createItem")
		{
			//createItem,name,filePath,x,y,mass,friction,rotation,endorphins,disappear,kinematic
			a.messageType = AIMessageType.createItem;
			Dictionary<string,System.Object> args = new Dictionary<string,System.Object>();
			if (clientArgs.Length==11)
			{
				try
				{
					args.Add("name", clientArgs[1]);
					args.Add("filePath", clientArgs[2]);
					args.Add("x", float.Parse(clientArgs[3]));
					args.Add("y", float.Parse(clientArgs[4]));
					args.Add("mass", float.Parse(clientArgs[5]));
					args.Add("friction", int.Parse(clientArgs[6]));
					if (!(new List<String>{"0","1","2","3","4","5"}).Contains(clientArgs[6].Trim()))
						throw new Exception("Friction must be either -1, 0, or 1!");
					args.Add("rotation", float.Parse(clientArgs[7]));
					args.Add("endorphins", float.Parse(clientArgs[8]));
					args.Add("disappear", clientArgs[9].Trim()=="1");
					args.Add("kinematic", int.Parse(clientArgs[10]));
					if (!(new List<String>{"0", "1", "2", "3", "4", "5"}).Contains(clientArgs[10].Trim()))
						throw new Exception("Friction must be an integer from 0 to 5");
				}
				catch(Exception e)
				{
					a.messageType = AIMessageType.other;
					a.stringContent = "ERROR: Error parsing one or more values in command: \""
						+ s + "\"\n";
					//throw e;
					return a;
				}
			}
			else
				throw new Exception("Incorrect # of arguments given in client message: " + s);
			
			a.detail = args;
		}
		else if (clientArgs[0] == "addForceToItem")
		{
		}
		else if (clientArgs[0] == "getInfoAboutItem")
		{
		}
		else if (clientArgs[0] == "destroyItem")
		{
		}
		//else if (clientArgs[0] == "establishConnection")
		//{}
		//else if (clientArgs[0] == "removeConnection")
		//{}
		
		return a;
	}
	
	public AIMessage(AIMessageType mtype, string sContent, float fContent=0, string details="")
	{
		messageType = mtype;
		stringContent = sContent;
		floatContent = fContent;
		//vectorContent = vContent;
		detail = details;
		timeCreated = DateTime.Now;
	}
	
	public override string ToString ()
	{
		return "{" + messageType.ToString() + ", " + stringContent + ", " + floatContent.ToString() + "}";
	}
}
