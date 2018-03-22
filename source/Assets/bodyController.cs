using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Text;
using System;
using System.IO;

public abstract class sensor
{
	[NonSerialized]
	public Rigidbody2D anchorBody = new Rigidbody2D();
	[NonSerialized]
	public Vector2 relativePosition = new Vector2();
	public abstract void updateSensor();
	
	protected worldObject getObjectAt(Vector2 pos) 
	{ 
		//close hand, grip whatever
		//find all objects it might possibly grip
		
		//int layerNum = 8; //Normal objects layer
		List<int> validLayers = new List<int>();
		validLayers.Add (LayerMask.NameToLayer("Default"));
		validLayers.Add (LayerMask.NameToLayer("Normal Objects"));
		validLayers.Add (LayerMask.NameToLayer("VisibleButNonreactive"));
		//int[] validLayers = new int[]{0,8};
		worldObject[] goArray = UnityEngine.MonoBehaviour.FindObjectsOfType(typeof(worldObject)) as worldObject[];
		List<System.Object> goList = new List<System.Object>();
		for (int i = 0; i < goArray.Length; i++) {
			if (validLayers.Contains(goArray[i].gameObject.layer)) {
				//Debug.Log("found: " + goArray[i]);
				goList.Add(goArray[i]);
				//does it have a collider?
				if (goArray[i].GetComponent<Collider2D>() != null)
				{
					if (goArray[i].GetComponent<Collider2D>().OverlapPoint(pos))
					{//connect it at that point
						worldObject obj = goArray[i];
						//Debug.Log("connecting hand to " + obj + " at " + rightHandRigidBody.position);
						//assuming the object is a rigidbody, you can get the position of pos relative to obj with obj.rigidbody2D.GetVector(pos)
						if (obj.objectName!="rightHand" && obj.objectName!="leftHand")
							return obj;
					}
				}
			}
		}
		//create background object to return
		worldObject BG = new worldObject();
		BG.objectName = "Background";
		return BG;
	}

	/// <summary>
	/// returns the absolute coordinates of this sensor
	/// </summary>
	/// <returns>The position.</returns>
	public Vector2 getPosition()
	{
		return anchorBody.GetRelativePoint(relativePosition);
	}
}

[System.Serializable]
public class touchSensor : sensor
{
	public string sensorCode;
	public int p;
	public float temp = 0f;
	public float[] texture = new float[3];
	[NonSerialized]
	public worldObject objectTouched;// = new worldObject();
	public float endorphins = 0f;
	
	public touchSensor(Rigidbody2D anchrBdy, Vector2 relPos)
	{
		anchorBody = anchrBdy;
		relativePosition = relPos;
	}
	
	public override void updateSensor()
	{
		Vector2 pos = getPosition();
		objectTouched = getObjectAt(pos);
		temp = objectTouched.temperature;
		if (objectTouched.GetType()==typeof(rewardOrPunishmentController))
			endorphins = ((rewardOrPunishmentController)objectTouched).endorphins;
		else
			endorphins = 0f;
		texture = new float[objectTouched.texture.Length];
		for (int i=0; i<texture.Length; i++)
			texture[i] = objectTouched.texture[i]; 
		if (objectTouched.objectName == "Background")
		{
			p = 0;
		} else {
			p = 1;
		}
	}
	
	/// <summary>
	/// Returns a string with all of the values to be returned (see documentation).
	/// Make sure to update the sensor before using this.
	/// </summary>
	/// <returns>The report.</returns>
	public string getReport()
	{
		if (objectTouched.objectName == "Background")
		{
			return "0,0,0,0,0,0,0,Background\n";
		}
		else {
			string toReturn = "1,";
			toReturn += temp.ToString();
			for (int i=0; i<texture.Length; i++)
				toReturn += "," + texture[i].ToString();
			//Debug.Log(texture.Length + " of " + sensor.objectTouched.name);
			toReturn += "," + endorphins.ToString();
			toReturn += "," + objectTouched.objectName;
			return toReturn;
		}
	}
}

[System.Serializable]
public class visualSensor : sensor
{
	public string sensorCode;
	public float[] vq = new float[4];
	/// <summary>
	/// the type/category of the object this sensor is sensing, or background
	/// </summary>
	public string type = "";
	/// <summary>
	/// the name/unique id of the object this sensor is sensing, or background
	/// </summary>
	public string name = "";
	[NonSerialized]
	public int indexX;
	[NonSerialized]
	public int indexY;
	
	public visualSensor(Rigidbody2D anchrBdy, Vector2 relPos, int indexX, int indexY)
	{
		anchorBody=anchrBdy;
		relativePosition=relPos;
		this.indexX = indexX;
		this.indexY = indexY;
	}
	
	public override void updateSensor()
	{
		Vector2 pos = getPosition();
		worldObject obj = getObjectAt(pos);
		//Debug.Log(obj.objectName);
		type = obj.objectType;
		name = obj.objectName;
		for (int i=0; i<vq.Length; i++)
		{
			if (i>=obj.visualFeatures.Length)
				vq[i]=0;
			else
				vq[i] = obj.visualFeatures[i];
		}
	}
}

public class bodyController : worldObject {

	public ThreadSafeList<AIMessage> messageQueue = GlobalVariables.messageQueue;// new ThreadSafeList<AIMessage>();
	public ThreadSafeList<string> outgoingMessages = GlobalVariables.outgoingMessages;//new ThreadSafeList<string>();
	
	//sensors
	public touchSensor[] leftHandSensor = new touchSensor[5];
	public touchSensor[] rightHandSensor = new touchSensor[5];
	public touchSensor[] bodySensor = new touchSensor[8];
	public Vector2[] proprioceptionSensor = new Vector2[2];
	static int numVisualSensorsX = 31, numVisualSensorsY = 21;
	public visualSensor[,] visualSensors = new visualSensor[numVisualSensorsX, numVisualSensorsY];
	static int numPeripheralSensorsX = 16, numPeripheralSensorsY = 11;
	public visualSensor[,] peripheralSensors = new visualSensor[numPeripheralSensorsX, numPeripheralSensorsY];
	//Dictionary<string,sensor> sensorNameLookup = new Dictionary<string, sensor>();
	
	public GameObject[] leftArm;
	public GameObject[] rightArm;
	public worldObject leftHand;
	public worldObject rightHand;
	Rigidbody2D leftHandRigidBody;
	Rigidbody2D rightHandRigidBody;
	
	public ObjectMenu objectMenu; //used for creating objects
	
	//temp: remove this later
	//public Rigidbody2D garf;
	
	//stuff for picking up things with hands
	//public bool leftIsClosed = false, rightIsClosed = false;
	public bool[] handIsClosed = new bool[]{false, false}; 
	//DistanceJoint2D leftJoint = null, rightJoint = null; 
	DistanceJoint2D[] handJoint = new DistanceJoint2D[]{null, null}; //this will connect the right hand to whatever it picked up
	public bool powerMode = false; //true if the hands move with 10x normal force
	
	// Use this for initialization
	void Start () {
		Application.runInBackground = true;
		//Debug.Log(System.IO.Directory.GetCurrentDirectory());
		
		leftHandRigidBody = leftHand.GetComponent<Rigidbody2D>();
		rightHandRigidBody = rightHand.GetComponent<Rigidbody2D>();
		leftHand.objectName = "leftHand";
		rightHand.objectName = "rightHand";
		
		//initialize sensors
		leftHandSensor[0] = new touchSensor(leftHandRigidBody, new Vector2(0, .65f));
		leftHandSensor[1] = new touchSensor(leftHandRigidBody, new Vector2(0.6f, 0));
		leftHandSensor[2] = new touchSensor(leftHandRigidBody, new Vector2(0, -.65f));
		leftHandSensor[3] = new touchSensor(leftHandRigidBody, new Vector2(-0.6f, 0));
		leftHandSensor[4] = new touchSensor(leftHandRigidBody, new Vector2(0, 0));
		rightHandSensor[0] = new touchSensor(rightHandRigidBody, new Vector2(0, .65f));
		rightHandSensor[1] = new touchSensor(rightHandRigidBody, new Vector2(0.6f, 0));
		rightHandSensor[2] = new touchSensor(rightHandRigidBody, new Vector2(0, -.65f));
		rightHandSensor[3] = new touchSensor(rightHandRigidBody, new Vector2(-0.6f, 0));
		rightHandSensor[4] = new touchSensor(rightHandRigidBody, new Vector2(0, 0));
		bodySensor[0] = new touchSensor(GetComponent<Rigidbody2D>(), new Vector2(0, 1.2f));
		bodySensor[1] = new touchSensor(GetComponent<Rigidbody2D>(), new Vector2(0.9f, 0.9f));
		bodySensor[2] = new touchSensor(GetComponent<Rigidbody2D>(), new Vector2(1.2f, 0));
		bodySensor[3] = new touchSensor(GetComponent<Rigidbody2D>(), new Vector2(0.9f, -0.9f));
		bodySensor[4] = new touchSensor(GetComponent<Rigidbody2D>(), new Vector2(0, -1.2f));
		bodySensor[5] = new touchSensor(GetComponent<Rigidbody2D>(), new Vector2(-0.9f, -0.9f));
		bodySensor[6] = new touchSensor(GetComponent<Rigidbody2D>(), new Vector2(-1.2f, 0));
		bodySensor[7] = new touchSensor(GetComponent<Rigidbody2D>(), new Vector2(-0.9f, 0.9f));
		Vector2 blCorner = new Vector2(-2.25f,1.65f); //the distance the bottom left corner of the visual field is from the body's center
		for (int x=0; x<numVisualSensorsX; x++)
			for (int y=0; y<numVisualSensorsY; y++)
				visualSensors[x,y] = new visualSensor(GetComponent<Rigidbody2D>(), blCorner + new Vector2(x*0.148f, y*0.1475f), x, y);
		blCorner = new Vector2(-14.5f,0.5f); //the distance the bottom left corner of the peripheral field is from the body's center
		for (int x=0; x<numPeripheralSensorsX; x++)
			for (int y=0; y<numPeripheralSensorsY; y++)
				peripheralSensors[x,y] = new visualSensor(GetComponent<Rigidbody2D>(), blCorner + new Vector2(x*2f, y*2f), x, y);
	
		/*//connect each sensor to a lookup key
		for (int i=0; i<5; i++)
		{
			sensorNameLookup.Add("leftHand" + i.ToString(), leftHandSensor[i]);
			sensorNameLookup.Add("rightHand" + i.ToString(), rightHandSensor[i]);
		}
		for (int i=0; i<8; i++)
			sensorNameLookup.Add("body" + i.ToString(), bodySensor[i]);
		for (int x=0; x<numVisualSensorsX; x++)
			for (int y=0; y<numVisualSensorsY; y++)
				sensorNameLookup.Add("dVisual" + x.ToString() + "." + y.ToString(), visualSensors[x,y]);
		blCorner = new Vector2(-11.25f,1.65f); //the distance the bottom left corner of the peripheral field is from the body's center
		for (int x=0; x<numPeripheralSensorsX; x++)
			for (int y=0; y<numPeripheralSensorsY; y++)
				sensorNameLookup.Add("pVisual" + x.ToString() + "." + y.ToString(), peripheralSensors[x,y]);*/
	
		/*//test:
		Reflex r = new Reflex("r");
		r.addCondition("BPx", '<', 0.5f);
		
		GlobalVariables.activeReflexes.Add(r);*/ 
		//texture = new Texture2D(1, 1);
		//texture.SetPixel(0,0, Color.blue);
		//texture.Apply();
		string[] s = System.Environment.GetCommandLineArgs();
		foreach (string S in s)
			Debug.Log("cmd: " + S);
	}

	//Texture2D texture;
	void OnGUI()
	{
		/*for (int x=0; x<numPeripheralSensorsX; x++)
		{
			for (int y=0; y<numPeripheralSensorsY; y++)
			{
				Vector2 v = peripheralSensors[x,y].getPosition();
				GUI.Box(new Rect(v.x-0.1f, v.y-0.1f, 0.2f, 0.2f), "P");
			}
		}
*/
		
		//GUI.skin.box.normal.background = texture;
		if (GlobalVariables.showPeripheralVisionMarkers)
		{
			Color oldColor = GUI.color;
			GUI.color = Color.red;
			for (int x=0; x<numPeripheralSensorsX; x++)
			{
				for (int y=0; y<numPeripheralSensorsY; y++)
				{
					Vector2 v = peripheralSensors[x,y].getPosition();
					//Debug.Log(Screen.width); 
					Vector3 v3 = Camera.main.WorldToScreenPoint(new Vector3(v.x, v.y, 0));
					//GUI.Box(new Rect(v3.x-1,(Screen.height - v3.y)-1, 2, 2), GUIContent.none); //why is this so slow
					GUI.Label(new Rect(v3.x-3, (Screen.height - v3.y)-5, 10, 10), "*");
					//GUI.Box(new Rect(10, 10, 0.5f, 0.5f), GUIContent.none); 
				}
			}
			GUI.color = oldColor;
		}
		if (GlobalVariables.showDetailedVisionMarkers)
		{
			for (int x=0; x<numVisualSensorsX; x++)
			{
				for (int y=0; y<numVisualSensorsY; y++)
				{
					Vector2 v = visualSensors[x,y].getPosition();
					//Debug.Log(Screen.width); 
					Vector3 v3 = Camera.main.WorldToScreenPoint(new Vector3(v.x, v.y, 0));
					//GUI.Box(new Rect(v3.x-1,(Screen.height - v3.y)-1, 2, 2), GUIContent.none); //why is this so slow
					GUI.Label(new Rect(v3.x-3, (Screen.height - v3.y)-5, 10, 10), "*");
					//GUI.Box(new Rect(10, 10, 0.5f, 0.5f), GUIContent.none); 
				}
			}
		}
	}

	/// <summary>>
	/// formats the detailed visual sensor map into a json object for sending back to the connected agent
	/// </summary>
	/// <returns> A string containing the JSON obj</returns>
	public string MDNtoJSON(){
		StringBuilder sb = new StringBuilder("");//("{ \"sensorType\":\"MDN\", \"sensorMap\":");
		for (int y=0; y<numVisualSensorsY; y++)
		{
			for (int x=0; x<numVisualSensorsX; x++)
			{
				visualSensor s = visualSensors[x,y];
				s.updateSensor();
				//string sensorString = JsonUtility.ToJson (s);
				//Debug.Log (sensorString);

				string sName = s.name;
				if (sName=="Background")
					sName = "";
				sb.Append(sName + ",");
			}
		}
		//sb.Append (" }");
		//sb[sb.Length-1] = '\n';
		//Debug.Log("msg is " + sb.ToString());
		//outgoingMessages.Add(sb.ToString());
		MsgToAgent newMsg = new MsgToAgent("MDN",sb.ToString());
		string finalMsg = JsonUtility.ToJson(newMsg);
		Debug.Log (finalMsg);
		return finalMsg;
	}

	/// <summary>>
	/// formats the detailed visual sensor map into a json object for sending back to the connected agent
	/// </summary>
	/// <returns> A string containing the JSON obj</returns>
	public string MPNtoJSON(){
		StringBuilder sb = new StringBuilder(""); //("{ \"type\":\"MDN\", \"sensorMap\":");
		for (int y=0; y<numPeripheralSensorsY; y++)
		{
			for (int x=0; x<numPeripheralSensorsX; x++)
			{
				visualSensor s = peripheralSensors[x,y];
				s.updateSensor();
				string sName = s.name;
				if (sName=="Background")
					sName = "";
				sb.Append(sName + ",");
			}
		}
		//sb.Append (" }");
		//sb[sb.Length-1] = '\n';
		//Debug.Log("msg is " + sb.ToString());
		//outgoingMessages.Add(sb.ToString());

		MsgToAgent newMsg = new MsgToAgent("MPN",sb.ToString());
		string finalMsg = JsonUtility.ToJson(newMsg);
		Debug.Log (finalMsg);
		return finalMsg;
	}

	public string ErrorToJSON(string msg)
	{
		MsgToAgent newMsg = new MsgToAgent ("errorMsg", msg);
		string jsonMsg = JsonUtility.ToJson(newMsg);
		return jsonMsg;
	}

	public string UpdateToJSON(string msg)
	{
		MsgToAgent newMsg = new MsgToAgent ("update", msg);
		string jsonMsg = JsonUtility.ToJson (newMsg);
		return jsonMsg;
	}
	
	/// <summary>
	/// interprets the sensor aspect code and checks the current value of the sensor
	/// referred to by it.
	/// </summary>
	/// <returns>The sensor aspect value code.</returns>
	/// <param name="sensorAspectCode">Sensor aspect code.</param>
	float getSensorAspectValue(string sensorAspectCode)
	{
		float sensorVal=0.0f;
		bool isStandardSensor = false;
		switch (sensorAspectCode)
		{
			case "Sx":
			sensorVal = GetComponent<Rigidbody2D>().velocity.x;
			break;
			case "Sy":
			sensorVal = GetComponent<Rigidbody2D>().velocity.y;
			break;
			case "BPx":
			sensorVal = GetComponent<Rigidbody2D>().position.x;
			break;
			case "BPy":
			sensorVal = GetComponent<Rigidbody2D>().position.y;
			break;
			case "LPx":
			leftHandSensor[4].updateSensor(); //recall sensor 4 is right in the middle of the hand
			Vector2 relativePoint = GetComponent<Rigidbody2D>().GetPoint(leftHandSensor[4].getPosition());
			sensorVal = relativePoint.x;
			break;
			case "LPy":
			leftHandSensor[4].updateSensor(); //recall sensor 4 is right in the middle of the hand
			relativePoint = GetComponent<Rigidbody2D>().GetPoint(leftHandSensor[4].getPosition());
			sensorVal = relativePoint.y;
			break;
			case "RPx":
			rightHandSensor[4].updateSensor(); //recall sensor 4 is right in the middle of the hand
			relativePoint = GetComponent<Rigidbody2D>().GetPoint(rightHandSensor[4].getPosition());
			sensorVal = relativePoint.x;
			break;
			case "RPy":
			rightHandSensor[4].updateSensor(); //recall sensor 4 is right in the middle of the hand
			relativePoint = GetComponent<Rigidbody2D>().GetPoint(rightHandSensor[4].getPosition());
			sensorVal = relativePoint.y;
			break;
			case "A":
			sensorVal = GetComponent<Rigidbody2D>().rotation;
			break;
			default:
			isStandardSensor = true;
			break;
		}
		
		if (isStandardSensor)
		{//find out which sensor it's polling: left/right hand, body, or visual?
			string[] ss = sensorAspectCode.Split(new char[]{'_'});
			touchSensor ts = null;
			bool isTactileSensor = false;
			if (ss[0].StartsWith("L"))
			{//tactile sensor for left hand
				isTactileSensor = true;
				ts = leftHandSensor[int.Parse(ss[0].Substring(1))];
			}
			if (ss[0].StartsWith("R"))
			{//tactile sensor for right hand
				isTactileSensor = true;
				ts = rightHandSensor[int.Parse(ss[0].Substring(1))];
			}
			if (ss[0].StartsWith("B"))
			{//tactile sensor for body
				isTactileSensor = true;
				ts = bodySensor[int.Parse(ss[0].Substring(1))];
			}
			if (isTactileSensor)
			{
				switch(ss[1])
				{
				case "tmp":
					ts.updateSensor();
					sensorVal = ts.temp;
					break;
				case "tx1":
					ts.updateSensor();
					sensorVal = ts.texture[0];
					break;
				case "tx2":
					ts.updateSensor();
					sensorVal = ts.texture[1];
					break;
				case "tx3":
					ts.updateSensor();
					sensorVal = ts.texture[2];
					break;
				case "tx4":
					ts.updateSensor();
					sensorVal = ts.texture[3];
					break;
				case "e":
					ts.updateSensor();
					sensorVal = ts.endorphins;
					break;
				}
			}
			else
			{//it's a detailed visual or peripheral sensor
				//ss[0] is e.g. V0.0 and ss[1] is e.g. vq2
				string[] crds = ss[0].Substring(1).Split(new char[]{'.'});
				//Debug.Log(sensorAspectCode);
				//Debug.Log(ss[0]);
				//Debug.Log(ss[1]);
				int x=int.Parse(crds[0]),y=int.Parse(crds[1]);
				visualSensor vs = null;
				if (ss[0][0] == 'V')
					vs = visualSensors[x,y];
				else if (ss[0][0]=='P')
					vs = peripheralSensors[x,y];
				else
					throw new Exception("I don't know this sensor aspect code: " + sensorAspectCode);
				vs.updateSensor();
				switch(ss[1])
				{
				case "vq1":
					sensorVal = vs.vq[0];
					break;
				case "vq2":
					sensorVal = vs.vq[1];
					break;
				case "vq3":
					sensorVal = vs.vq[2];
					break;
				case "vq4":
					sensorVal = vs.vq[3];
					break;
				}
			}
		}
		return sensorVal;
	}
	
	/// <summary>
	/// checks all active reflexes to see if any should be fired
	/// </summary>
	void checkReflexes()
	{	
		List<Reflex> activeReflexes_copy = GlobalVariables.activeReflexes.getCopy();
		List<State> activeStates_copy = GlobalVariables.activeStates.getCopy();
		foreach (Reflex r in activeReflexes_copy)
		{
			//are the conditions of r all satisfied?
			bool allSatisfied = true;
			foreach (Reflex.condition c in r.conditions)
			{
				//is c satisfied?
				if (c is Reflex.stateCondition)
				{
					//Debug.Log("checking state");
					Reflex.stateCondition C = (Reflex.stateCondition)c;
					//check if C.stateName is an active state TODO: make this faster
					bool foundState = false;
					foreach (State st in GlobalVariables.activeStates.getCopy())
					{
						if (st.stateName == C.stateName)
						{
							//Debug.Log("found");
							foundState = true;
							break;
						}
					}
					if (foundState == C.isNegated)
					{
						allSatisfied = false;
						break;
					}
				}
				else if (c is Reflex.sensoryCondition)
				{
					float tolerance = 0.01f;
					Reflex.sensoryCondition C = (Reflex.sensoryCondition)c;
					//which sensor does it correspond to?
					float sensorVal = getSensorAspectValue(C.sensorName);
					if (C.operatorType == '<')
						allSatisfied = (sensorVal < C.value);
					else if (C.operatorType == '>')
						allSatisfied = (sensorVal > C.value);
					else if (C.operatorType == '=')
					{
						allSatisfied = (Math.Abs(sensorVal - C.value) <= tolerance);
					}
					else if (C.operatorType == '}')
						allSatisfied = (sensorVal >= C.value);
					else if (C.operatorType == '{')
						allSatisfied = (sensorVal <= C.value);
					else if (C.operatorType == '!')
						allSatisfied = (Math.Abs(sensorVal - C.value) > tolerance);
					else {
						throw new Exception("Unrecognized operatorType " + C.operatorType);
					}
					//Debug.Log("sensorVal and actual: " + sensorVal.ToString() + ", " + C.value + ", " + allSatisfied);
						
					if (!allSatisfied)
					{
						//Debug.Log("condition not met!");
						break;
					}
					//else
					//	Debug.Log("condition met!");
				}
			}
			
			//if all conditions are satisfied, trigger this reflex and send msg to the user
			if (allSatisfied)
			{
				//Debug.Log("all good");
				if (GlobalVariables.sendNotificationOnReflexFirings) {
					string s = UpdateToJSON ("reflexFired," + r.reflexName);
					outgoingMessages.Add (s);
				}
				foreach (AIMessage a in r.actions)
					messageQueue.Add (a);
			}
		}
	}

	/// <summary>
	/// Called after first loading a file. Goes through all custom items and adds them to the list.
	/// </summary>
	public void indexCustomItems()
	{
		//for every custom item on the map
		customItemController[] goArray = UnityEngine.MonoBehaviour.FindObjectsOfType(typeof(customItemController)) as customItemController[];
		Debug.Log(goArray.Length);
		foreach (customItemController c in goArray)
		{
			string wName = c.objectName;
			/*while (customItems.ContainsKey(wName))
			{
				if (customItems[wName] != null)
				{
					Destroy(customItems[wName].gameObject);
				}
				customItems.Remove(wName);
				Debug.Log("removing " + wName);
			}*/
			if (customItems.ContainsKey(wName))
			{
				//Debug.Log("seeing key " + wName + " to " + customItems[wName]);
			}
			else
			{
				//Debug.Log("adding key " + wName + " (not in dictionary)");
				customItems.Add(wName,c);
			}
		}
	}

	/// <summary>
	/// iterate through the customItems, remove any that no longer exist
	/// </summary>
	void updateCustomItems()
	{
		List<string> toRemove = new List<string> ();
		foreach (string ky in customItems.Keys)
			if (customItems [ky] == null)
				toRemove.Add (ky);
		foreach (string ky in toRemove)
			customItems.Remove (ky);
	}

	public Camera mainCamera;
	public customItemController emptyblock; //used to instantiate custom objects
	public speechBubbleController emptyBubble; //used to instantiate speech bubbles
	public Dictionary<string,worldObject> customItems = new Dictionary<string, worldObject>();
	// Update is called once per frame
	void Update () {
	
		if (GlobalVariables.centerCamera) //scroll camera
		{
			//Vector2 newLeft = 
			if (mainCamera == null)
				Debug.Log("no camera");
			else
				mainCamera.transform.Translate((transform.position - mainCamera.transform.position) - new Vector3(0,0,10));
		}


		//Debug.Log(rigidbody2D.transform.rotation*new Vector2(0,100));
		//Debug.Log (rigidbody2D.position);
		//visualSensors[15,1].updateSensor();
		//Debug.Log(visualSensors[15,1].name);	
		checkReflexes();
	
		//check for messages in the message queue (which stores all messages sent by TCP clients)
		while (messageQueue.Count() > 0)
		{
			AIMessage firstMsg;
			string strToReturn;
			while (!messageQueue.TryGet(0, out firstMsg)) 
				Thread.Sleep(100);
			/*Debug.Log (messageQueue.Count() + " MESSAGES IN QUEUE: " + firstMsg.ToString());
			for (int i=0; i<messageQueue.Count(); i++)
			{
				AIMessage a;
				while (!messageQueue.TryGet(i, out a)) 
					Thread.Sleep(100);
				Debug.Log("message " + i.ToString() + ": " + a.ToString());
			}*/
			while (!messageQueue.TryRemoveAt(0))
				Thread.Sleep(100);
			//process firstMsg
			firstMsg.stringContent = firstMsg.stringContent.Trim();
			try
			{
			//switch (firstMsg.messageType)
			//{
				if( firstMsg.messageType.CompareTo("other") == 0 ){
					//AIMessage.AIMessageType.other:
					Debug.Log("received unrecognized command from client connection " + firstMsg.stringContent);
					strToReturn = ErrorToJSON("UnrecognizedCommandError" + firstMsg.stringContent.Trim());
					outgoingMessages.Add(strToReturn);
				}
				else if( firstMsg.messageType.CompareTo("createItem") == 0){
					Debug.Log("Received command to create item");

					Dictionary<string,System.Object> dd = (Dictionary<string,System.Object>)firstMsg.detail;
					string wName = (string)dd["name"]; //firstMsg.otherStrings[0]; 
					customItemController cic = Instantiate(emptyblock, new Vector3(), new Quaternion()) as customItemController;

					//Debug.Log("MADE IT to bodyController func");
					//if (!cic.initialize(firstMsg.stringContent, firstMsg.otherStrings[0], firstMsg.vectorContent, float.Parse(firstMsg.otherStrings[2]), 
					//	float.Parse(firstMsg.otherStrings[3]), firstMsg.floatContent,
					//	int.Parse(firstMsg.otherStrings[1]), int.Parse(firstMsg.otherStrings[4]) ))
					if (!cic.initialize((string)dd["filePath"], wName, new Vector2((float)dd["x"], (float)dd["y"]),
					               (float)dd["rotation"], (float)dd["endorphins"], (float)dd["mass"], (int)dd["friction"], (int)dd["kinematic"]))
					{
						Debug.Log("file " + firstMsg.stringContent + " not found");
						strToReturn = ErrorToJSON("createItem,"+wName+",FAILED,fileNotFound");
						outgoingMessages.Add(strToReturn);
					}
					else
					{
						while (customItems.ContainsKey(wName))
						{
							if (customItems[wName] != null)
							{
								Destroy(customItems[wName].gameObject);
							}
							customItems.Remove(wName);
						}
						customItems.Add(wName,cic);
						strToReturn = UpdateToJSON("createItem,"+wName+",OK\n");
						outgoingMessages.Add(strToReturn);
						//outgoingMessages.Add("createItem,"+(string)dd["name"]+",OK\n");
					}

					//Debug.Log("DONE in BodyController");
				}
				else if( firstMsg.messageType.CompareTo("say") == 0 ){//AIMessage.AIMessageType.say:
					Debug.Log ("Received command to say " + firstMsg.stringContent);
					updateCustomItems();
					//create item
					speechBubbleController sbc = Instantiate(emptyBubble, new Vector3(), new Quaternion()) as speechBubbleController;
					//string bubbleName;
					if (firstMsg.otherStrings[0] == "P") //the speaker is PAGI guy, position vector is relative to him
					{
						//bubbleName = firstMsg.otherStrings[0] + "_speechBubble";
						sbc.initialize(firstMsg.stringContent, Convert.ToInt32(firstMsg.floatContent), 
					                 GetComponent<Rigidbody2D>().position+firstMsg.vectorContent);
					}
					else if (firstMsg.otherStrings[0] == "N") //there is no speaker; use the position given as absolute
					{
						sbc.initialize(firstMsg.stringContent, Convert.ToInt32(firstMsg.floatContent), 
					                           firstMsg.vectorContent);
					}
					else //the speaker is a custom object
					{
						//find the item to add speech to, add it
						if (!customItems.ContainsKey(firstMsg.otherStrings[0]))
						{
							strToReturn = ErrorToJSON("say," + firstMsg.otherStrings[0] + ",ERR:Speaker_Name_Not_Found");
							outgoingMessages.Add(strToReturn);
							//outgoingMessages.Add("say," + firstMsg.otherStrings[0] + ",ERR:Speaker_Name_Not_Found\n");
						}
						else
						{
							if (customItems[firstMsg.otherStrings[0]] == null)
							{
								customItems.Remove(firstMsg.otherStrings[0]);
								strToReturn = ErrorToJSON("say," + firstMsg.otherStrings[0] + ",ERR:Object_Deleted");
								outgoingMessages.Add(strToReturn);
								//outgoingMessages.Add("say," + firstMsg.otherStrings[0] + ",ERR:Object_Deleted\n");
							}
							else
							{
								//create item
								sbc.initialize(firstMsg.stringContent, Convert.ToInt32(firstMsg.floatContent), 
									               customItems[firstMsg.otherStrings[0]].GetComponent<Rigidbody2D>().position+firstMsg.vectorContent);
							}
							strToReturn = UpdateToJSON("say," + firstMsg.otherStrings[0] + ",OK");
							outgoingMessages.Add(strToReturn);
							//outgoingMessages.Add("say," + firstMsg.otherStrings[0] + ",OK\n");
						}
					}
				}


				else if( firstMsg.messageType.CompareTo("addForceToItem") == 0){//AIMessage.AIMessageType.addForceToItem:
					Debug.Log ("Received command to addForceToItem");
					updateCustomItems();
					//find the item to add force to, add it
					if (!customItems.ContainsKey(firstMsg.stringContent))
					{
						strToReturn = ErrorToJSON("addForceToItem," + firstMsg.stringContent + ",ERR:Item_Name_Not_Found");
						outgoingMessages.Add(strToReturn);
						//outgoingMessages.Add("addForceToItem," + firstMsg.stringContent + ",ERR:Item_Name_Not_Found\n");
					}
					else
					{
						if (customItems[firstMsg.stringContent] == null)
						{
							customItems.Remove(firstMsg.stringContent);
							strToReturn = ErrorToJSON("addForceToItem," + firstMsg.stringContent + ",ERR:Object_Deleted");
							outgoingMessages.Add(strToReturn);
							//outgoingMessages.Add("addForceToItem," + firstMsg.stringContent + ",ERR:Object_Deleted\n");
						}
						else
						{
							customItems[firstMsg.stringContent].GetComponent<Rigidbody2D>().AddForce(firstMsg.vectorContent);
							customItems[firstMsg.stringContent].GetComponent<Rigidbody2D>().AddTorque(firstMsg.floatContent);
							strToReturn = UpdateToJSON("addForceToItem," + firstMsg.stringContent + ",OK");
							outgoingMessages.Add(strToReturn);
							//outgoingMessages.Add("addForceToItem," + firstMsg.stringContent + ",OK\n");
						}
					}
				}
					
				else if( firstMsg.messageType.CompareTo("getInfoAboutItem") == 0){//AIMessage.AIMessageType.getInfoAboutItem:
					Debug.Log ("Received command to getInfoAboutItem");
					//find the item to add force to, add it
					if (!customItems.ContainsKey(firstMsg.stringContent))
					{
						strToReturn = ErrorToJSON("getInfoAboutItem," + firstMsg.stringContent + ",ERR:Item_Name_Not_Found");
						outgoingMessages.Add(strToReturn);
						//outgoingMessages.Add("getInfoAboutItem," + firstMsg.stringContent + ",ERR:Item_Name_Not_Found\n");
					}
					else
					{
						if (customItems[firstMsg.stringContent] == null)
						{
							customItems.Remove(firstMsg.stringContent);
							strToReturn = ErrorToJSON("getInfoAboutItem," + firstMsg.stringContent + ",ERR:Object_Deleted");
							outgoingMessages.Add(strToReturn);
							//outgoingMessages.Add("getInfoAboutItem," + firstMsg.stringContent + ",ERR:Object_Deleted\n");
						}
						else
						{
							worldObject wo = customItems[firstMsg.stringContent];
							//string toReturn = "getInfoAboutItem," + firstMsg.stringContent + ",";
							//toReturn = toReturn + wo.transform.position.x.ToString() + "," + wo.transform.position.y.ToString() + ",";
							//toReturn = toReturn + wo.GetComponent<Rigidbody2D>().velocity.x.ToString() + "," + wo.GetComponent<Rigidbody2D>().velocity.y.ToString() + "\n";
							ItemInfoToAgent itm = new ItemInfoToAgent(firstMsg.stringContent,wo.transform.position.x,wo.transform.position.y,
								wo.GetComponent<Rigidbody2D>().velocity.x, wo.GetComponent<Rigidbody2D>().velocity.y);
							strToReturn = JsonUtility.ToJson(itm);
							outgoingMessages.Add(strToReturn);
							//outgoingMessages.Add(toReturn);
						}
					}
				}
				else if( firstMsg.messageType.CompareTo("destroyItem") == 0){//AIMessage.AIMessageType.destroyItem:
					Debug.Log ("Received command to destroyItem");
					//find the item to add force to, add it
					if (!customItems.ContainsKey(firstMsg.stringContent))
					{
						strToReturn = ErrorToJSON("destroyItem," + firstMsg.stringContent + ",ERR:Item_Name_Not_Found");
						outgoingMessages.Add(strToReturn);
						//outgoingMessages.Add("destroyItem," + firstMsg.stringContent + ",ERR:Item_Name_Not_Found\n");
					}
					else
					{
						if (customItems[firstMsg.stringContent] == null)
						{
							customItems.Remove(firstMsg.stringContent);
							strToReturn = ErrorToJSON("destroyItem," + firstMsg.stringContent + ",WARNING:Object_Already_Deleted");
							outgoingMessages.Add(strToReturn);
							//outgoingMessages.Add("destroyItem," + firstMsg.stringContent + ",WARNING:Object_Already_Deleted\n");
						}
						else
						{
							Destroy(customItems[firstMsg.stringContent].gameObject);
							customItems.Remove(firstMsg.stringContent);
							strToReturn = UpdateToJSON("destroyItem," + firstMsg.stringContent + ",OK");
							outgoingMessages.Add(strToReturn);
							//outgoingMessages.Add("destroyItem," + firstMsg.stringContent + ",OK\n");
						}
					}	
				}
				else if( firstMsg.messageType.CompareTo("print") == 0){//AIMessage.AIMessageType.print:
					Debug.Log("received command to print message");
					Debug.Log("AI-side says: " + firstMsg.stringContent);
					strToReturn = UpdateToJSON("print,OK");
					outgoingMessages.Add(strToReturn);
					//outgoingMessages.Add("print,OK\n");
				}
				else if( firstMsg.messageType.CompareTo("findObj") == 0){//AIMessage.AIMessageType.findObj:
					Debug.Log("received message to find object: " + firstMsg.stringContent);
					//strToReturn = "findObj," + firstMsg.stringContent;
					strToReturn = "";
					string searchType = firstMsg.otherStrings[0];//((string)firstMsg.detail).Trim();
					//Debug.Log(searchType);
					int count = 0;
					if (searchType == "D" || searchType == "PD")
					{
						//Debug.Log("Checking detailed sensor");
						foreach (visualSensor v in visualSensors)
						{
							v.updateSensor();
							if (v.name.Trim() == firstMsg.stringContent.Trim() && count != 0 ){
								strToReturn += ",V" + v.indexX.ToString() + "." + v.indexY.ToString();
							}
							else if( v.name.Trim() == firstMsg.stringContent.Trim() ){
								count = 1;
								strToReturn += "V" + v.indexX.ToString() + "." + v.indexY.ToString();
							}
						}
					}
					if (searchType == "P" || searchType == "PD")
					{
						foreach (visualSensor p in peripheralSensors)
						{
							p.updateSensor();
							if (p.name.Trim() == firstMsg.stringContent.Trim() && count != 0 )
								strToReturn += ",P" + p.indexX.ToString() + "." + p.indexY.ToString();
							else if(p.name.Trim() == firstMsg.stringContent.Trim() ){
								count = 1;
								strToReturn += "P" + p.indexX.ToString() + "." + p.indexY.ToString();
							}
						}
					}
					else if( searchType != "D" )
						throw new Exception("ERR: Invalid sensor type in FindObj command");

					MsgToAgent m = new MsgToAgent(firstMsg.stringContent, strToReturn);
					string finalMsg = JsonUtility.ToJson(m);
					outgoingMessages.Add(finalMsg);
					//outgoingMessages.Add(strToReturn + "\n");
				}
				
				else if( firstMsg.messageType.CompareTo("loadTask") == 0 ){//AIMessage.AIMessageType.loadTask:
					Debug.Log("received message to load new task");
					string finalMsg;
					strToReturn = "loadTask," + firstMsg.stringContent.Trim();
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
					bool loadedOk = true;
					try
					{
						FileSaving fs = new FileSaving(firstMsg.stringContent);
					}
					catch(Exception e)
					{
						string errDesc = e.ToString().Replace('\n',';');
						errDesc = errDesc.Replace('\r',' ');
						finalMsg = ErrorToJSON(strToReturn + ",ERR," + errDesc);
						outgoingMessages.Add(finalMsg);
						//outgoingMessages.Add(strToReturn + ",ERR," + errDesc + "\n");
						loadedOk = false;
					}
					if (loadedOk){
						finalMsg = UpdateToJSON(strToReturn + ",OK");
						outgoingMessages.Add(finalMsg);
						//outgoingMessages.Add(strToReturn + ",OK\n");
					}
				}
				
				else if( firstMsg.messageType.CompareTo("setState") == 0 ){//AIMessage.AIMessageType.setState:
					//is the state already active? If so, replace the time
					State foundState = null;
					foreach (State st in GlobalVariables.activeStates.getCopy())
					{
						if (st.stateName == firstMsg.stringContent)
						{
							foundState = st;
							break;
						}
					}	
					//Debug.Log("foundstate for " + firstMsg.stringContent + " " + (foundState!=null).ToString());
					if (foundState!=null)
					{
						//replace state
						GlobalVariables.activeStates.TryRemove(foundState);
					}
					if (((State)firstMsg.detail).lifeTime != TimeSpan.Zero)
						GlobalVariables.activeStates.Add((State)firstMsg.detail);
					strToReturn = UpdateToJSON("stateUpdated," + firstMsg.stringContent.Trim());
					outgoingMessages.Add(strToReturn);
					//outgoingMessages.Add("stateUpdated," + firstMsg.stringContent.Trim() + "\n");
				}

				else if( firstMsg.messageType.CompareTo("getReflexes") == 0 ){//AIMessage.AIMessageType.getReflexes:
					string toR = "";
					int count = 0;
					foreach (Reflex r in GlobalVariables.activeReflexes.getCopy())
					{
						if( count != 0 )
							toR += "," + r.reflexName;
						else{
							count = 1;
							toR += r.reflexName;
						}
					}
					MsgToAgent m = new MsgToAgent("activeReflexes", toR);
					strToReturn = JsonUtility.ToJson(m);
					outgoingMessages.Add(strToReturn);
					//outgoingMessages.Add(toR + "\n");
				}
				
				else if( firstMsg.messageType.CompareTo("getStates") == 0 ){//AIMessage.AIMessageType.getStates:
					strToReturn = "";
					List<State> allStates = GlobalVariables.activeStates.getCopy();
					foreach (State sta in allStates)
					{
						strToReturn += sta.stateName + ",";
					}
					if (allStates.Count == 0)
						strToReturn += "(none)";
					else
						strToReturn = strToReturn.Substring(0, strToReturn.Length-1);
					MsgToAgent m = new MsgToAgent("activeStates",strToReturn);
					string s = JsonUtility.ToJson(m);
					outgoingMessages.Add(s);
					//outgoingMessages.Add(strToReturn + "\n");
				}
				else if( firstMsg.messageType.CompareTo("removeReflex") == 0){//AIMessage.AIMessageType.removeReflex:
					Reflex re = null;
					foreach (Reflex R in GlobalVariables.activeReflexes.getCopy())
					{
						Debug.Log("comparing " + R.reflexName + " to " + firstMsg.stringContent);
						if (R.reflexName.Trim() == firstMsg.stringContent.Trim())
						{
							re = R;
							break;
						}
					}
					if (re!=null)
					{
						GlobalVariables.activeReflexes.TryRemove(re);
						strToReturn = UpdateToJSON("removedReflex," + firstMsg.stringContent.Trim() + ",OK");
						outgoingMessages.Add(strToReturn);
					}
					else{
						strToReturn = ErrorToJSON("removedReflexFAILED" + firstMsg.stringContent.Trim() + ",FAILED");
						outgoingMessages.Add(strToReturn);
					}
					
				}
				else if( firstMsg.messageType.CompareTo("setReflex") == 0 ){//AIMessage.AIMessageType.setReflex:
					//does a reflex with this name already exist? If so, replace it
					Reflex re = null;
					foreach (Reflex R in GlobalVariables.activeReflexes.getCopy())
					{
						if (R.reflexName.Trim() == firstMsg.stringContent.Trim())
						{
							re = R;
							break;
						}
					}
					if (re!=null)
						GlobalVariables.activeReflexes.TryRemove(re);
					GlobalVariables.activeReflexes.Add((Reflex)firstMsg.detail);
					strToReturn = UpdateToJSON("reflexUpdated," + firstMsg.stringContent);
					outgoingMessages.Add(strToReturn);
				}
			
				else if( firstMsg.messageType.CompareTo("dropItem") == 0){//AIMessage.AIMessageType.dropItem:
					Debug.Log("received command to create " + firstMsg.stringContent + " at " + firstMsg.vectorContent);
					//if required, there is additional content at firstMsg.detail
					//find the asset that matches the name
					bool loaded = false;
					foreach (worldObject s in Resources.LoadAll<worldObject>("Prefabs"))
					{
						if (s.objectName==firstMsg.stringContent.Trim())
						{
							worldObject newObj =  MonoBehaviour.Instantiate(s,//Resources.Load<GameObject>(wo.assetPath) 
							                                                new Vector3(firstMsg.vectorContent.x, firstMsg.vectorContent.y),
							                                                new Quaternion()) as worldObject;
							loaded = true;					                     
							break;
						}
					}
					if (!loaded){
						strToReturn = ErrorToJSON("dropItem," + firstMsg.stringContent + ",FAILED:obj_not_found");
						outgoingMessages.Add(strToReturn);
					}
					else{
						strToReturn = UpdateToJSON("dropItem," + firstMsg.stringContent + ",OK");
						outgoingMessages.Add(strToReturn);
					}
				}
				else if( firstMsg.messageType.CompareTo("addForce") == 0 ){//AIMessage.AIMessageType.addForce:
					Debug.Log("executing addForce command: " + firstMsg.messageType.ToString());
					//do we need to evaluate the force value, e.g. if there is a function?
					switch (firstMsg.stringContent)
					{
						case "TEST":
							/*string s = "";
							int numToSend = int.Parse(firstMsg.floatContent.ToString());
							Debug.Log("creating string ("+numToSend.ToString()+")");
							for (int i=0; i<numToSend; i++)
								s += "X";*/
							Debug.Log("got TEST msg: " + firstMsg.function1.evaluate(getSensorAspectValue));
							//outgoingMessages.Add(s+'\n');
							break;
						/*LHV,LHH - Left hand vertical and horizontal. v is the amount of force (positive or negative) to add in each dimension.
						RHV,RHH - Right hand vertical and horizontal. v is the amount of force (positive or negative) to add in each dimension.
						BMV,BMH - Body vertical and horizontal. v is the amount of force (positive or negative) to add in each dimension.
						BR - Body rotation right or left. v is the amount of torque to use to rotate the body (can be positive or negative).
						RHG,RHR - Right hand grip and release. v is required, but ignored here. A hand is either in a state of gripping or it isn't.
						LHG,LHR - Left hand grip and release. v is required, but ignored here. A hand is either in a state of gripping or it isn't.*/
						case "LHH":
							Vector2 f = GetComponent<Rigidbody2D>().transform.rotation*new Vector2(firstMsg.function1.evaluate(getSensorAspectValue),0);//new Vector2(Mathf.Cos(Mathf.Deg2Rad*-rigidbody2D.rotation)*firstMsg.floatContent,Mathf.Sin(Mathf.Deg2Rad*-rigidbody2D.rotation)*firstMsg.floatContent);
							leftHandRigidBody.AddForce(f);
							GetComponent<Rigidbody2D>().AddForce(-f);
							//rigidbody2D.AddForce(new Vector2(0, 10000));
							strToReturn = UpdateToJSON("LHH,1");
							outgoingMessages.Add(strToReturn);
						break;
						case "LHV":
							f = GetComponent<Rigidbody2D>().transform.rotation*new Vector2(0,firstMsg.function2.evaluate(getSensorAspectValue));//new Vector2(Mathf.Sin(Mathf.Deg2Rad*-rigidbody2D.rotation)*firstMsg.floatContent,Mathf.Cos(Mathf.Deg2Rad*-rigidbody2D.rotation)*firstMsg.floatContent);
							leftHandRigidBody.AddForce(f);//, ForceMode2D.Impulse);
							GetComponent<Rigidbody2D>().AddForce(-f);
							strToReturn = UpdateToJSON("LHV,1");
							outgoingMessages.Add(strToReturn);
						break;
						case "LHvec":
							f = GetComponent<Rigidbody2D>().transform.rotation*
								(new Vector2(firstMsg.function1.evaluate(getSensorAspectValue), firstMsg.function2.evaluate(getSensorAspectValue)));
							leftHandRigidBody.AddForce(f);//, ForceMode2D.Impulse);
							GetComponent<Rigidbody2D>().AddForce(-f);
							strToReturn = UpdateToJSON("LHvec,1");
							outgoingMessages.Add(strToReturn);
							break;
						case "RHH":
							f = GetComponent<Rigidbody2D>().transform.rotation*new Vector2(firstMsg.function1.evaluate(getSensorAspectValue),0);//new Vector2(Mathf.Cos(Mathf.Deg2Rad*-rigidbody2D.rotation)*firstMsg.floatContent,Mathf.Sin(Mathf.Deg2Rad*-rigidbody2D.rotation)*firstMsg.floatContent);
							rightHandRigidBody.AddForce(f);
							GetComponent<Rigidbody2D>().AddForce(-f);
							//rigidbody2D.AddForce(new Vector2(0, 10000));
							strToReturn = UpdateToJSON("RHH,1");
							outgoingMessages.Add(strToReturn);
						break;
						case "RHV":
							f = GetComponent<Rigidbody2D>().transform.rotation*new Vector2(0,firstMsg.function2.evaluate(getSensorAspectValue));//new Vector2(Mathf.Sin(Mathf.Deg2Rad*-rigidbody2D.rotation)*firstMsg.floatContent,Mathf.Cos(Mathf.Deg2Rad*-rigidbody2D.rotation)*firstMsg.floatContent);
							rightHandRigidBody.AddForce(f);//, ForceMode2D.Impulse);
							GetComponent<Rigidbody2D>().AddForce(-f);
							strToReturn = UpdateToJSON("RHV,1");
							outgoingMessages.Add(strToReturn);
						break;
						case "RHvec":
							f = GetComponent<Rigidbody2D>().transform.rotation*
								(new Vector2(firstMsg.function1.evaluate(getSensorAspectValue), firstMsg.function2.evaluate(getSensorAspectValue)));;
							GetComponent<Rigidbody2D>().AddForce(-f);
							rightHandRigidBody.AddForce(f);
							strToReturn = UpdateToJSON("RHvec,1");
							outgoingMessages.Add(strToReturn);
							break;
						case "BMH":
							f = GetComponent<Rigidbody2D>().transform.rotation*new Vector2(firstMsg.function1.evaluate(getSensorAspectValue),0);
						//new Vector2(Mathf.Cos(Mathf.Deg2Rad*rigidbody2D.rotation)*firstMsg.floatContent,Mathf.Sin(Mathf.Deg2Rad*rigidbody2D.rotation)*firstMsg.floatContent);
							GetComponent<Rigidbody2D>().AddForce(f);
							strToReturn = UpdateToJSON("BMH,1");
							outgoingMessages.Add(strToReturn);
						break;
						case "BMV":
							f = GetComponent<Rigidbody2D>().transform.rotation*new Vector2(0,firstMsg.function2.evaluate(getSensorAspectValue));//new Vector2(Mathf.Sin(Mathf.Deg2Rad*-rigidbody2D.rotation)*firstMsg.floatContent,Mathf.Cos(Mathf.Deg2Rad*-rigidbody2D.rotation)*firstMsg.floatContent);
							GetComponent<Rigidbody2D>().AddForce(f);
							Debug.Log("added f: " + f.y);
							strToReturn = UpdateToJSON("BMV,1");
							outgoingMessages.Add(strToReturn);
						break;
						case "BMvec":
							f = GetComponent<Rigidbody2D>().transform.rotation*
								(new Vector2(firstMsg.function1.evaluate(getSensorAspectValue), firstMsg.function2.evaluate(getSensorAspectValue)));
							GetComponent<Rigidbody2D>().AddForce(f);
							strToReturn = UpdateToJSON("BMvec,1");
							outgoingMessages.Add(strToReturn);
							break;
						
						case "J": //jump
							bool foundGround = jump(30000f);
							if (foundGround){
								strToReturn = UpdateToJSON("J,1");
								outgoingMessages.Add(strToReturn);
							}
							else{
								strToReturn = UpdateToJSON("J,0");
								outgoingMessages.Add(strToReturn);
							}
							break;
						case "BR":
							GetComponent<Rigidbody2D>().rotation += firstMsg.function1.evaluate(getSensorAspectValue);
							leftHand.GetComponent<Rigidbody2D>().rotation = GetComponent<Rigidbody2D>().rotation;
							rightHand.GetComponent<Rigidbody2D>().rotation = GetComponent<Rigidbody2D>().rotation;
							GetComponent<Rigidbody2D>().AddForce(Vector2.zero); //forces update of rotation
							strToReturn = UpdateToJSON("BR,1");
							outgoingMessages.Add(strToReturn);
						break;
						case "RHG":
							setGrip(false, true);
							strToReturn = UpdateToJSON("RHG,1");
							outgoingMessages.Add(strToReturn);
							break;
						case "RHR":
							setGrip(false, false);
							strToReturn = UpdateToJSON("RHR,1");
							outgoingMessages.Add(strToReturn);
							break;
						case "LHG":
							setGrip(true, true);
							strToReturn = UpdateToJSON("LHG,1");
							outgoingMessages.Add(strToReturn);
							break;
						case "LHR":
							setGrip(true, false);
							strToReturn = UpdateToJSON("LHR,1");
							outgoingMessages.Add(strToReturn);
							break;
					}	
				}
				else if( firstMsg.messageType.CompareTo("sensorRequest") == 0){//AIMessage.AIMessageType.sensorRequest:
					Debug.Log("checking sensor value " + firstMsg.ToString());
					switch (firstMsg.stringContent[0])
					{
						case 'M': //a full map of the visual field
							if (firstMsg.stringContent.Trim() == "MDN") //detailed visual field (names only)
							{
								outgoingMessages.Add(MDNtoJSON());
							}
							else if (firstMsg.stringContent.Trim() == "MPN") //peripheral visual field (names only)
							{
								outgoingMessages.Add(MPNtoJSON());
							}
							else
							{
								//outgoingMessages.Add("sensorRequest,UNRECOGNIZED_SENSOR_ERROR:"+firstMsg.stringContent.Trim()+"\n");
								string errorMsg = ErrorToJSON("sensorRequest error: UNRECOGNIZED_SENSOR_ERROR");
								outgoingMessages.Add(errorMsg);
							}
						break;
						case 'B': //body touch sensor B0-B7
							if (firstMsg.stringContent[1]=='P') //body position
							{
								Vector2 v = GetComponent<Rigidbody2D>().position;
								BasicSensorToAgent b = new BasicSensorToAgent("BP",v.x,v.y);
								string msg = JsonUtility.ToJson(b);
								outgoingMessages.Add(msg);
							}
							else
							{	
								int sensorNum = int.Parse(firstMsg.stringContent[1].ToString());
								touchSensor sensor = bodySensor[sensorNum];
								sensor.updateSensor();
								sensor.sensorCode = firstMsg.stringContent;
								string msg = JsonUtility.ToJson(sensor);
								outgoingMessages.Add(msg);
							}
							break;
						case 'S': //speed sensor
							Vector2 vec = GetComponent<Rigidbody2D>().GetRelativePointVelocity(Vector2.zero);
							BasicSensorToAgent bMsg = new BasicSensorToAgent("S",vec.x,vec.y);
							string msgStr = JsonUtility.ToJson(bMsg);
							outgoingMessages.Add(msgStr);
							break;
						case 'L': //L0-L4, or LP
							if (firstMsg.stringContent[1]=='P')
							{//proprioception; get sensor position relative to body
								leftHandSensor[4].updateSensor(); //recall sensor 4 is right in the middle of the hand
								Vector2 relativePoint = GetComponent<Rigidbody2D>().GetPoint(leftHandSensor[4].getPosition());
								BasicSensorToAgent b = new BasicSensorToAgent("LP",relativePoint.x,relativePoint.y);
								string msg = JsonUtility.ToJson(b);
								outgoingMessages.Add(msg);		
							}
							else
							{
								int sensorNum = int.Parse(firstMsg.stringContent[1].ToString());
								touchSensor sensor = leftHandSensor[sensorNum];
								sensor.updateSensor();
								sensor.sensorCode = "L" + sensorNum.ToString();
								string msg = JsonUtility.ToJson(sensor);
								outgoingMessages.Add(msg);
							}
						break; 
						case 'R': //R0-R4, or RP
							if (firstMsg.stringContent[1]=='P')
							{	//proprioception; get sensor position relative to body
								rightHandSensor[4].updateSensor();
								Vector2 relativePoint = GetComponent<Rigidbody2D>().GetPoint(rightHandSensor[4].getPosition());
								BasicSensorToAgent b = new BasicSensorToAgent("RP",relativePoint.x, relativePoint.y);
								string msg = JsonUtility.ToJson(b);
								outgoingMessages.Add(msg);
							}
							else
							{
								int sensorNum = int.Parse(firstMsg.stringContent[1].ToString());
								touchSensor sensor = rightHandSensor[sensorNum];
								sensor.updateSensor();
								sensor.sensorCode = "R" + sensorNum.ToString();
								string msg = JsonUtility.ToJson(sensor);
								outgoingMessages.Add(msg);
							}
						break;
						case 'V': //visual sensor V0.0 - V30.20
							string[] tmp = firstMsg.stringContent.Substring(1).Split('.');
							int xCoord = int.Parse(tmp[0]);
							int yCoord = int.Parse(tmp[1]);
							string m;
							if (xCoord>=numVisualSensorsX || yCoord>=numVisualSensorsY)
							{
								m = ErrorToJSON("In SensorRequest V, ERR:IndexOutOfRange");
								outgoingMessages.Add(m);
							}
							visualSensor sen = visualSensors[xCoord,yCoord];
							sen.updateSensor();
							sen.sensorCode = firstMsg.stringContent;
							m = JsonUtility.ToJson(sen);
							outgoingMessages.Add(m);
						break;
						case 'P': //peripheral sensor V0.0 - V15.10
							tmp = firstMsg.stringContent.Substring(1).Split('.');
							xCoord = int.Parse(tmp[0]);
							yCoord = int.Parse(tmp[1]);
							string messageP;
							if (xCoord>=numPeripheralSensorsX || yCoord>=numPeripheralSensorsY)
							{
								messageP = ErrorToJSON("In SensorRequest P, ERR:IndexOutOfRange");
								outgoingMessages.Add(messageP);
							}
							sen = peripheralSensors[xCoord,yCoord];
							sen.updateSensor();
							sen.sensorCode = firstMsg.stringContent;
							messageP = JsonUtility.ToJson(sen);
							outgoingMessages.Add(messageP);
						break;
						case 'A': //rotation sensor
							BasicSensorToAgent bSen = new BasicSensorToAgent("A",Mathf.Deg2Rad*GetComponent<Rigidbody2D>().rotation,0.0f);
							string myMsg = JsonUtility.ToJson(bSen);
							outgoingMessages.Add(myMsg);
							//outgoingMessages.Add("A," + (Mathf.Deg2Rad*GetComponent<Rigidbody2D>().rotation).ToString() + "\n");
						break;
						default:
							string s = ErrorToJSON("In SensorRequest, ERR:Unrecognized_Sensor_Error");
							outgoingMessages.Add(s);
						break;
					}
				}
				else if( firstMsg.messageType.CompareTo("establishConnection") == 0){//AIMessage.AIMessageType.establishConnection:
					break;
				}
				else if( firstMsg.messageType.CompareTo("removeConnection") == 0){//AIMessage.AIMessageType.removeConnection:
					break;
				}
				else{
					throw new Exception("Err: invalid command type specified " + firstMsg.messageType );	
				}
			}
			catch (Exception e)
			{
				strToReturn = UpdateToJSON("ERR: While processing message of type " + firstMsg.messageType + " (see log)");
				outgoingMessages.Add(strToReturn);
			}
		}
		
		//update arm positions
		//leftHand.objectName = "hi";
		Vector2 leftRelativePoint = gameObject.GetComponent<Rigidbody2D>().GetRelativePoint(leftHand.GetComponent<DistanceJoint2D>().connectedAnchor);
		Vector3 leftAnchor = new Vector3(leftRelativePoint.x, leftRelativePoint.y);
		leftArm[0].transform.position = (leftHand.transform.position*1/3 + leftAnchor*2/3);
		leftArm[1].transform.position = (leftHand.transform.position*2/3 + leftAnchor*1/3);
		Vector2 rightRelativePoint = gameObject.GetComponent<Rigidbody2D>().GetRelativePoint(rightHand.GetComponent<DistanceJoint2D>().connectedAnchor);
		Vector3 rightAnchor = new Vector3(rightRelativePoint.x, rightRelativePoint.y);
		rightArm[0].transform.position = (rightHand.transform.position*1/3 + rightAnchor*2/3);
		rightArm[1].transform.position = (rightHand.transform.position*2/3 + rightAnchor*1/3);
		
		//updateSensors();
		
		
		//////keyboard controls///////
		
		//hand gripping
		if (Input.GetKeyDown(KeyCode.LeftShift))
			setGrip(true, true);
		if (Input.GetKeyUp(KeyCode.LeftShift))
			setGrip(true, false);
		if (Input.GetKeyDown(KeyCode.RightShift))
			setGrip(false, true);
		if (Input.GetKeyUp(KeyCode.RightShift))
			setGrip(false, false);
		
		
		if (Input.GetKeyDown(KeyCode.P))
			powerMode = !powerMode;
		float handMoveForce = 50f;
		if (powerMode)
			handMoveForce *= 10;
		//right hand
		if (Input.GetKey(KeyCode.UpArrow)) {//GetKeyDown is one-time press only
			//transform.Translate(new Vector3(1,0,0));
			Vector2 f = GetComponent<Rigidbody2D>().transform.rotation*new Vector2(0,handMoveForce);//new Vector2(Mathf.Sin(Mathf.Deg2Rad*-rigidbody2D.rotation)*firstMsg.floatContent,Mathf.Cos(Mathf.Deg2Rad*-rigidbody2D.rotation)*firstMsg.floatContent);
			rightHandRigidBody.AddForce(f);//, ForceMode2D.Impulse);
			GetComponent<Rigidbody2D>().AddForce(-f);
		}
		if (Input.GetKey (KeyCode.DownArrow)) {
			Vector2 f = GetComponent<Rigidbody2D>().transform.rotation*new Vector2(0,-handMoveForce);//new Vector2(Mathf.Sin(Mathf.Deg2Rad*-rigidbody2D.rotation)*firstMsg.floatContent,Mathf.Cos(Mathf.Deg2Rad*-rigidbody2D.rotation)*firstMsg.floatContent);
			rightHandRigidBody.AddForce(f);//, ForceMode2D.Impulse);
			GetComponent<Rigidbody2D>().AddForce(-f);
		}
		if (Input.GetKey(KeyCode.LeftArrow))
		{
			Vector2 f = GetComponent<Rigidbody2D>().transform.rotation*new Vector2(-handMoveForce,0);//new Vector2(Mathf.Sin(Mathf.Deg2Rad*-rigidbody2D.rotation)*firstMsg.floatContent,Mathf.Cos(Mathf.Deg2Rad*-rigidbody2D.rotation)*firstMsg.floatContent);
			rightHandRigidBody.AddForce(f);//, ForceMode2D.Impulse);
			GetComponent<Rigidbody2D>().AddForce(-f);
		}
		if (Input.GetKey(KeyCode.RightArrow))
		{
			Vector2 f = GetComponent<Rigidbody2D>().transform.rotation*new Vector2(handMoveForce,0);//new Vector2(Mathf.Sin(Mathf.Deg2Rad*-rigidbody2D.rotation)*firstMsg.floatContent,Mathf.Cos(Mathf.Deg2Rad*-rigidbody2D.rotation)*firstMsg.floatContent);
			rightHandRigidBody.AddForce(f);//, ForceMode2D.Impulse);
			GetComponent<Rigidbody2D>().AddForce(-f);
		}		
		//left hand
		if (Input.GetKey (KeyCode.W)) {//GetKeyDown is one-time press only
			//transform.Translate(new Vector3(1,0,0));
			Vector2 f = GetComponent<Rigidbody2D>().transform.rotation*new Vector2(0,handMoveForce);//new Vector2(Mathf.Sin(Mathf.Deg2Rad*-rigidbody2D.rotation)*firstMsg.floatContent,Mathf.Cos(Mathf.Deg2Rad*-rigidbody2D.rotation)*firstMsg.floatContent);
			leftHandRigidBody.AddForce(f);
			GetComponent<Rigidbody2D>().AddForce(-f);
		}
		if (Input.GetKey (KeyCode.S)) {
			Vector2 f = GetComponent<Rigidbody2D>().transform.rotation*new Vector2(0,-handMoveForce);//new Vector2(Mathf.Sin(Mathf.Deg2Rad*-rigidbody2D.rotation)*firstMsg.floatContent,Mathf.Cos(Mathf.Deg2Rad*-rigidbody2D.rotation)*firstMsg.floatContent);
			leftHandRigidBody.AddForce(f);
			GetComponent<Rigidbody2D>().AddForce(-f);
		}
		if (Input.GetKey(KeyCode.A))
		{
			Vector2 f = GetComponent<Rigidbody2D>().transform.rotation*new Vector2(-handMoveForce,0);//new Vector2(Mathf.Sin(Mathf.Deg2Rad*-rigidbody2D.rotation)*firstMsg.floatContent,Mathf.Cos(Mathf.Deg2Rad*-rigidbody2D.rotation)*firstMsg.floatContent);
			leftHandRigidBody.AddForce(f);
			GetComponent<Rigidbody2D>().AddForce(-f);
		}
		if (Input.GetKey(KeyCode.D))
		{
			Vector2 f = GetComponent<Rigidbody2D>().transform.rotation*new Vector2(handMoveForce,0);//new Vector2(Mathf.Sin(Mathf.Deg2Rad*-rigidbody2D.rotation)*firstMsg.floatContent,Mathf.Cos(Mathf.Deg2Rad*-rigidbody2D.rotation)*firstMsg.floatContent);
			leftHandRigidBody.AddForce(f);
			GetComponent<Rigidbody2D>().AddForce(-f);
		}
		
		
		/*if (Input.GetKeyDown(KeyCode.LeftShift))
		{
			leftHandRigidBody.AddForce(new Vector2(-1000, 0));
			rightHandRigidBody.AddForce(new Vector2(-1000, 0));
			rigidbody2D.AddForce(new Vector2(2000, 0));
		}
		if (Input.GetKeyDown(KeyCode.RightShift))
		{
			leftHandRigidBody.AddForce(new Vector2(1000, 0));
			rightHandRigidBody.AddForce(new Vector2(1000, 0));
			rigidbody2D.AddForce(new Vector2(-2000, 0));
		}*/
		
		

		if (Input.GetKeyDown(KeyCode.Space))
			jump(30000f);
		if (Input.GetKey(KeyCode.V))
			GlobalVariables.viewControlsVisible = true;
		
		//Rotate
		if (Input.GetKey(KeyCode.R))
		{
			GetComponent<Rigidbody2D>().rotation += 1.0f;
			GetComponent<Rigidbody2D>().rotation %= 360f;
			leftHandRigidBody.rotation = GetComponent<Rigidbody2D>().rotation;
			rightHandRigidBody.rotation = GetComponent<Rigidbody2D>().rotation;
			//Debug.Log (rigidbody2D.GetRelativePoint(rightAnchor));
			GetComponent<Rigidbody2D>().AddForce(new Vector2(0,0)); //this forces the screen to update his rotation
		}
		if (Input.GetKey(KeyCode.T))
		{
			GetComponent<Rigidbody2D>().rotation -= 1.0f;
			GetComponent<Rigidbody2D>().rotation %= 360f;
			leftHandRigidBody.rotation = GetComponent<Rigidbody2D>().rotation;
			rightHandRigidBody.rotation = GetComponent<Rigidbody2D>().rotation;
			//Debug.Log (rigidbody2D.GetRelativePoint(rightAnchor));
			GetComponent<Rigidbody2D>().AddForce(new Vector2(0,0)); //this forces the screen to update his rotation
		}
		
		//Move
		if (Input.GetKey(KeyCode.F))
			GetComponent<Rigidbody2D>().AddForce(GetComponent<Rigidbody2D>().transform.rotation*(new Vector2(-500f,0)));
		if (Input.GetKey(KeyCode.G))
			GetComponent<Rigidbody2D>().AddForce(transform.rotation*new Vector2(500f,0));
	}
	
	bool jump(float amt)
	{
		//is he touching anything on the ground? Calculate five contact points between bodySensors 3 and 5.
		Vector2[] bottom = new Vector2[]{GetComponent<Rigidbody2D>().GetRelativePoint(new Vector2(0.9f, -0.9f)),
			GetComponent<Rigidbody2D>().GetRelativePoint(new Vector2(0.5f, -1.2f)),
			GetComponent<Rigidbody2D>().GetRelativePoint(new Vector2(0, -1.2f)),
			GetComponent<Rigidbody2D>().GetRelativePoint(new Vector2(-0.5f, -1.2f)),
			GetComponent<Rigidbody2D>().GetRelativePoint(new Vector2(-0.9f, -0.9f))};
		//Debug.Log(bodySensor[4].objectTouched);
		//int layerNum = 8; //Normal objects layer
		Rigidbody2D[] goArray = UnityEngine.MonoBehaviour.FindObjectsOfType(typeof(Rigidbody2D)) as Rigidbody2D[];
		//List<System.Object> goList = new List<System.Object>();
		bool foundGround = false;
		int triggerBoxLayer = LayerMask.NameToLayer("Trigger Boxes");
		for (int n=0; n<bottom.Length; n++)
		{
			for (int i = 0; i < goArray.Length; i++) {
				if (goArray[i] == leftHandRigidBody || goArray[i] == rightHandRigidBody)
					continue;
				if (goArray[i].gameObject.layer == triggerBoxLayer) 
					continue;
				//Debug.Log("found: " + goArray[i]);
				//goList.Add(goArray[i]);
				if (goArray[i].GetComponent<Collider2D>().OverlapPoint(bottom[n]))
				{//connect it at that point
					//Debug.Log("jump found: " + goArray[i]);
					Rigidbody2D obj = goArray[i];
					Vector2 jumpForce = GetComponent<Rigidbody2D>().transform.rotation*new Vector2(0f,amt);
					GetComponent<Rigidbody2D>().AddForce(jumpForce);//new Vector2(Mathf.Sin(Mathf.Deg2Rad*-rigidbody2D.rotation)*30000f,Mathf.Cos(Mathf.Deg2Rad*-rigidbody2D.rotation)*30000f));
					obj.AddForce(-jumpForce);//-new Vector2(Mathf.Sin(Mathf.Deg2Rad*-rigidbody2D.rotation)*30000f,Mathf.Cos(Mathf.Deg2Rad*-rigidbody2D.rotation)*30000f));
					foundGround = true;
					break;
				}
				//}
			}
			if (foundGround)
				break;
		}
		return foundGround;
		//	rigidbody2D.AddForce(new Vector2(Mathf.Sin(Mathf.Deg2Rad*-rigidbody2D.rotation)*-60000f,Mathf.Cos(Mathf.Deg2Rad*-rigidbody2D.rotation)*-60000f));
		//if (garf.rigidbody2D.collider2D.
		//garf.AddForce(new Vector2(Mathf.Sin(Mathf.Deg2Rad*-rigidbody2D.rotation)*-500000f,Mathf.Cos(Mathf.Deg2Rad*-rigidbody2D.rotation)*-500000f));
		
	}
	
	
	/// <summary>
	/// Sets the grip.
	/// </summary>
	/// <param name="forLeftHand">If this is for left hand, otherwise it's for the right hand</param>
	/// <param name="isGrasp">If we want this hand to grasp, otherwise it's to release</param>
	void setGrip(bool forLeftHand, bool isGrasp)
	{
		int handIndex;
		Animator a;
		Rigidbody2D handRigidBody;
		if (forLeftHand)
		{
			handIndex = 0;
			a = leftHand.GetComponent<Animator>();
			handRigidBody = leftHandRigidBody;
		}
		else
		{
			handIndex = 1;
			a = rightHand.GetComponent<Animator>();
			handRigidBody = rightHandRigidBody;
		}
		
		//is it currently touching any objects that are trigger boxes? If so, notify them
		triggerBoxController[] w = FindObjectsOfType<triggerBoxController>();
		foreach (triggerBoxController o in w)
		{
			if (o.GetComponent<Collider2D>().OverlapPoint(handRigidBody.position))
				o.GripOrReleaseHandler(isGrasp, forLeftHand);
		}
		
		if (isGrasp)
		{
			//Debug.Log("grasping");
			if (!handIsClosed[handIndex])
			{
				//close hand, grip whatever
				handIsClosed[handIndex] = true;
				//a = rightHand.GetComponent<Animator>();
				a.SetBool("handClosed", true);
				//find all objects it might possibly grip
				
				int layerNum = 8; //Normal objects layer
				worldObject[] goArray = FindObjectsOfType(typeof(worldObject)) as worldObject[];
				List<System.Object> goList = new List<System.Object>();
				for (int i = 0; i < goArray.Length; i++) {
					if (goArray[i].gameObject.layer == layerNum) {
						//Debug.Log("found: " + goArray[i].collider2D);
						//goList.Add(goArray[i]);
						if (goArray[i].GetComponent<Collider2D>().OverlapPoint(handRigidBody.position))
							
						{
							//connect it at that point
							worldObject obj = goArray[i];
							Debug.Log("connecting hand to " + obj + " at " + handRigidBody.position);
							handJoint[handIndex] = obj.gameObject.AddComponent<DistanceJoint2D>();
							handJoint[handIndex].anchor = obj.GetComponent<Rigidbody2D>().GetPoint(handRigidBody.position);
							//Debug.Log(obj.rigidbody2D.GetPoint(rightHandRigidBody.position));
							handJoint[handIndex].connectedBody = handRigidBody;
							handJoint[handIndex].connectedAnchor = new Vector2(0,0);//Vector2.zero; //the position on the hand that grabs it
							handJoint[handIndex].distance = 0;
							//obj.rigidbody2D.position = rightHandRigidBody.position;
						}
					}
				}
				
				
			}
		}
		else
		{
			//Debug.Log("releasing");
			if (handIsClosed[handIndex])
			{//open hand
				//Debug.Log("opening hand");
				handIsClosed[handIndex] = false;
				//a = rightHand.GetComponent<Animator>();
				a.SetBool("handClosed", false);
				//drop object
				if (handJoint[handIndex]!=null)
				{
					Destroy(handJoint[handIndex]);
					handJoint[handIndex] = null;
				}
			}
		}
	}
	
	void OnTouchedRewardOrPunishment(float msg)
	{
		Debug.Log("endorphins " + msg);
	}
}
