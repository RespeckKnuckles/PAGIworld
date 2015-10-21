﻿using UnityEngine;
//using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
//using System.Runtime.Serialization.Formatters.Soap;
using System.Runtime.Serialization.Formatters.Binary;
//using System.Xml.Serialization;

[System.Serializable()]
public class worldObjectSavedToFile
{
	public string assetPath;
	public int prefabID;
	public float positionX, positionY;
	public float rotation;
	public string objName;
	public Dictionary<string,System.Object> specialValues;
	
	public worldObjectSavedToFile(worldObject w)
	{
		//store the world object's relevant details
		prefabID = w.prefabID;
		if (w.GetComponents<Rigidbody2D> ().Length > 0) {
			positionX = w.rigidbody2D.transform.position.x;
			positionY = w.rigidbody2D.transform.position.y;
			rotation = w.rigidbody2D.transform.rotation.eulerAngles.z;
		}
		//Debug.Log ("rotation of " + w.objectName + " is saved as " + rotation.ToString ());
		objName = w.objectName;
		specialValues = w.valuesToSave;
	}
}

/// <summary>
/// Represents all saveable details of a single task, in serialized version for file writing
/// </summary>
[System.Serializable()]
public class FileSaving{
	
	public string taskDescriptor = "";
	public float bodyX, bodyY, bodyRotation;
	public float leftHandX, leftHandY;
	public float rightHandX, rightHandY;
	public List<worldObjectSavedToFile> worldObjects = new List<worldObjectSavedToFile>();
	
	float gravityX, gravityY;
	float gameSpeed;
	bool centerCamera = false; //keeps camera centered on PAGI guy
	bool showDetailedVisionMarkers = false;	
	bool showPeripheralVisionMarkers = false;

	
	[System.NonSerialized]
	bodyController mainBody;
	[System.NonSerialized]
	worldObject rightHand, leftHand;

	/// <summary>
	/// creates an empty instance and automatically saves the current task
	/// </summary>
	public FileSaving()
	{
		loadObjs();
		storeCurrentTask();
	}
	
	/// <summary>
	/// opens fileName and automatically loads it over the current task
	/// </summary>
	/// <param name="fileName">File name.</param>
	public FileSaving(string fileName)
	{
		loadObjs();
		//load from fileName
		Stream s = File.OpenRead(fileName);
		BinaryFormatter bf = new BinaryFormatter();
		FileSaving loaded = (FileSaving)bf.Deserialize(s);
		s.Close();
		//copy data from the loaded class to this
		taskDescriptor = loaded.taskDescriptor;
		worldObjects = loaded.worldObjects;
		bodyX = loaded.bodyX;
		bodyY = loaded.bodyY;
		bodyRotation = loaded.bodyRotation;
		rightHandX = loaded.rightHandX;
		rightHandY = loaded.rightHandY;
		leftHandX = loaded.leftHandX;
		leftHandY = loaded.leftHandY;
		gravityX = loaded.gravityX;
		gravityY = loaded.gravityY;
		gameSpeed = loaded.gameSpeed;
		centerCamera = loaded.centerCamera;
		showDetailedVisionMarkers = loaded.showDetailedVisionMarkers;
		showPeripheralVisionMarkers = loaded.showPeripheralVisionMarkers;

		loadToCurrentTask();
	}
	
	void loadObjs()
	{
		worldObject[] goArray = UnityEngine.MonoBehaviour.FindObjectsOfType(typeof(worldObject)) as worldObject[];
		foreach (worldObject obj in goArray)
		{
			if (obj.objectName == "mainBody")
				mainBody = (bodyController)obj;
			else if (obj.objectName == "leftHand")
				leftHand = obj;
			else if (obj.objectName == "rightHand")
				rightHand = obj;
		}
	}
	
	void loadToCurrentTask()
	{
		//place all world objects
		foreach (worldObjectSavedToFile wo in worldObjects)
		{
			//find the asset path of the object with this prefab id, search through all prefabs
			bool loaded = false;
			foreach (worldObject s in Resources.LoadAll<worldObject>("Prefabs"))
			{
				if (s.prefabID==wo.prefabID)
				{
					 worldObject newObj =  MonoBehaviour.Instantiate(s,//Resources.Load<GameObject>(wo.assetPath) 
					                          new Vector3(wo.positionX, wo.positionY),
					                          new Quaternion(0,0,wo.rotation,0)) as worldObject;
					newObj.transform.rotation = Quaternion.Euler(0, 0, wo.rotation);
					Debug.Log ("rotation of " + wo.objName + " is loaded as " + wo.rotation.ToString ());
					newObj.valuesToSave = wo.specialValues;
					newObj.loadVals();
					loaded = true;					                     
					break;
				}
			}
			
			if (!loaded)
				Debug.Log("Couldn't load item " + wo.objName);
			
		}//"Prefabs/food_and_pain/bacon"
		
		//set position/rotation of body, hands
		mainBody.rigidbody2D.transform.position = new Vector2(bodyX, bodyY);
		mainBody.rigidbody2D.velocity = Vector2.zero;
		//Debug.Log("x " + bodyX + " Y " + bodyY);
		mainBody.rigidbody2D.transform.rotation = Quaternion.Euler(0, 0, bodyRotation);
		rightHand.transform.position = new Vector2(rightHandX, rightHandY);
		leftHand.transform.position = new Vector2(leftHandX, leftHandY);
		rightHand.rigidbody2D.transform.rotation = mainBody.rigidbody2D.transform.rotation;
		leftHand.rigidbody2D.transform.rotation = mainBody.rigidbody2D.transform.rotation;
		mainBody.rigidbody2D.AddForce(new Vector2(0,0)); //this forces the screen to update his rotation

		//load environment variables like gravity, game speed, etc.
		Physics2D.gravity = new Vector2(gravityX, gravityY);
		Time.timeScale = gameSpeed;
		GlobalVariables.centerCamera = centerCamera;
		GlobalVariables.showDetailedVisionMarkers = showDetailedVisionMarkers;
		GlobalVariables.showPeripheralVisionMarkers = showPeripheralVisionMarkers;
		
		mainBody.customItems = new Dictionary<string, worldObject>();
		mainBody.indexCustomItems();
	}

	public void storeCurrentTask()
	{
		//store position/rotation of body, hands
		bodyX = mainBody.rigidbody2D.transform.position.x;
		bodyY = mainBody.rigidbody2D.transform.position.y;
		bodyRotation = mainBody.rigidbody2D.transform.rotation.eulerAngles.z;
		rightHandX = rightHand.transform.position.x;
		rightHandY = rightHand.transform.position.y;
		leftHandX = leftHand.transform.position.x;
		leftHandY = leftHand.transform.position.y;

		//store environment variables like gravity, game speed, etc.
		gravityX = Physics2D.gravity.x;
		gravityY = Physics2D.gravity.y;
		gameSpeed = Time.timeScale;
		centerCamera = GlobalVariables.centerCamera;
		showDetailedVisionMarkers = GlobalVariables.showDetailedVisionMarkers;
		showPeripheralVisionMarkers = GlobalVariables.showPeripheralVisionMarkers;

		
		//save all world objects
		worldObject[] goArray = UnityEngine.MonoBehaviour.FindObjectsOfType(typeof(worldObject)) as worldObject[];
		foreach (worldObject obj in goArray)
		{
			if (obj.prefabID < 0)
			{
				string n = obj.objectName;
				if (n!="mainBody" && n!="leftHand" && n!="rightHand")
					Debug.Log("PREFAB ID INVALID: THIS OBJECT WILL NOT BE SAVED " + obj.objectName + " id " + obj.prefabID);
			}
			else
			{
				obj.saveVals();
				worldObjects.Add (new worldObjectSavedToFile(obj));
			}
		}
	}
	
	public void writeToFile(string fileName)
	{
		Stream s = File.Open(fileName, FileMode.Create);
		BinaryFormatter bf = new BinaryFormatter();
		bf.Serialize(s, this);
		s.Close();
	}
	
}
