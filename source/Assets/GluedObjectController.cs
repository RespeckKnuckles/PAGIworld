using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GluedObjectController : worldObject {

	//public GameObject remains;
	public float kE_Requirement = 15;
	GameObject p;
	
	// Use this for initialization
	void Start () {
		p = transform.parent.gameObject;
		//Debug.Log(p.name + "," + name);
	}
	/*
	public static float KineticEnergy(Collision2D collision){
		// mass in kg, velocity in meters per second, result is
		return 0.5f*GameObject.Find("breakable_box").rigidbody2D.mass*Mathf.Pow(collision.relativeVelocity.magnitude,2);
	}*/
	
	void OnCollisionEnter2D(Collision2D collision) {
		bool destroyAll = false;
		
		foreach (worldObject w in p.GetComponentsInChildren<worldObject>())
			foreach (HingeJoint2D h in w.GetComponents<HingeJoint2D>())
			{
				float mag = h.GetReactionForce(1.0f).magnitude;
				//Debug.Log(mag);
				if (mag>kE_Requirement)
				{
					destroyAll = true;
				}
			}
		if (destroyAll)
			foreach (worldObject w in p.GetComponentsInChildren<worldObject>())
				foreach (HingeJoint2D h in w.GetComponents<HingeJoint2D>())
					Destroy(h);
		
		//Debug.Log(name + ": " + isConnected().ToString());
	}
	
	/// <summary>
	/// Checks if this is still connected to something else
	/// </summary>
	/// <returns><c>true</c>, if severed was ised, <c>false</c> otherwise.</returns>
	public bool isConnected()
	{
		//Debug.Log("initiator: " + name + p.GetComponentsInChildren<worldObject>().Length.ToString());
		foreach (worldObject w in p.GetComponentsInChildren<worldObject>())
		{
			//Debug.Log("checking " + w.name);
			if (w.GetComponents<HingeJoint2D>().Length > 0)
				return true;
			//else
			//	Debug.Log("was zero");
		}
		return false;
	}
	
	
	// Update is called once per frame
	void Update () {
		
	}
}