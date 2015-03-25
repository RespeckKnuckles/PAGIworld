using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MainScreen : MonoBehaviour {

	
	// Update is called once per frame
	void Update () {
		//Debug.Log("loading scene");
		//Application.LoadLevel("scene");
	}
	
	void OnGUI()
	{
		//Debug.Log("hi");
		GUILayout.BeginHorizontal();
		
		GUILayout.Label("PAGI-World\nA physically realistic simulation environment for artificial general intelligence systems");
		
		GUILayout.EndHorizontal();
		
		GUILayout.BeginHorizontal();
		
		if (GUILayout.Button("Load Main Task"))
			Application.LoadLevel("scene");
		
		#if UNITY_EDITOR
			if (GUILayout.Button("Load Task From File"))
			{
				string path = (UnityEditor.EditorUtility.OpenFilePanel(
					"Open Task File",
					"",
					"tsk"));
				if (path!="" && path!=null)
				{
					Application.LoadLevel("scene");
					//remove all world objects currently in scene (except for body/hands)
					worldObject[] goArray = UnityEngine.MonoBehaviour.FindObjectsOfType(typeof(worldObject)) as worldObject[];
					List<string> doNotRemove = new List<string>() { "leftHand", "rightHand", "mainBody" };
					foreach (worldObject obj in goArray)
					{
						if (!doNotRemove.Contains(obj.objectName))
						{
							//Debug.Log("destroying " + obj.objectName);
							Destroy(obj.gameObject);
						}
					}
					
					FileSaving g = new FileSaving(path);
				}
				//Application.LoadLevel("scene");
			}
		#endif
			
		GUILayout.EndHorizontal();
	}
}
