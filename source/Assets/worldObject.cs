using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class worldObject:MonoBehaviour{ //describes objects in the world
	public float temperature = 0f;
	public float[] texture = new float[4];
	public float[] visualFeatures = new float[4];
	public string objectName = "unnamedObject";
	public string objectType = "defaultType";
	public bool canBeDragged = true;
	/// <summary>
	/// This uniquely identifies which prefab this object came from. If it's -1, then you won't be able
	/// to save this and trying to save to file will generate an exception.
	/// </summary>
	public int prefabID = -1;
	
	Quaternion startAngle;
	float phi; //used for calculating mouse position
	Vector2 startRelativePos; //this will tell us where on the object the mouse clicked (relatively)
	
	//public bool deleteObject = false; //determines if the object is being held over the trash can
	
	//used if the worldObject has special data values that need to be saved or restored when loading/saving to file
	public Dictionary<string,System.Object> valuesToSave = new Dictionary<string, System.Object>(); 
	/// <summary>
	/// Called when saving to file. All non-standard data values need to be written to "valuesToSave".
	/// </summary>
	public virtual void saveVals() {}
	
	/// <summary>
	/// Called when loading from file. Load all non-standard data from "valuesToSave".
	///</summary> 
	public virtual void loadVals() {}


	
	//very basic mouse dragging code:
	void OnMouseDrag()
	{
		if (canBeDragged && gameObject.rigidbody2D != null)	
		{
			
			Vector3 point = Camera.main.ScreenToWorldPoint(Input.mousePosition); //convert mouse location to world coordinates
			point.z = gameObject.transform.position.z;
			//calculate phi
			//float t = Mathf.Tan(startAngle.a
			//phi = ((startRelativePos.x/startRelativePos.y)+Mathf.Tan(
			
			gameObject.transform.position = point - startAngle*(new Vector3(startRelativePos.x, startRelativePos.y, 0));
			
			//rigidbody2D.position = startRelativePos + new Vector2(point.x, point.y);
			//Screen.showCursor = false;
			
			rigidbody2D.velocity = Vector2.zero;//.Set(10,100);
			rigidbody2D.angularVelocity = 0;
			rigidbody2D.transform.rotation = startAngle;
			//rigidbody2D.isKinematic = false;
			
		}
	}
	
	void OnMouseDown()
	{
		startAngle = rigidbody2D.transform.rotation;
		Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
		startRelativePos = rigidbody2D.GetPoint(new Vector2(mousePos.x, mousePos.y));
		
		if (GlobalVariables.mouseDeleting)
		{
			GlobalVariables.mouseDeleting = false;
			Cursor.SetCursor(null, Vector2.zero, CursorMode.ForceSoftware);
			Destroy(this.gameObject);
		}
		//Debug.Log("you clicked " + objectName);
		/*if (deleteHeld)
		{
			Debug.Log("yup");
			Destroy (this.gameObject);
		}
		else {
			Debug.Log("nope");
				}*/
	}
	
	void OnMouseUp()
	{
		Screen.showCursor = true;
	}
	
	/*public bool deleteHeld = false;
	void Update()
	{
		if (Input.GetKeyDown(KeyCode.D))
		{
			deleteHeld = true;
			Debug.Log ("hi");
			}
		else//if (Input.GetKeyUp(KeyCode.Delete))
			deleteHeld = false;
	}*/
	
}