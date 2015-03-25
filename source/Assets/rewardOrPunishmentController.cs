using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class rewardOrPunishmentController : worldObject {

	public bool disappearAfterTouching = true;
	/// <summary>
	/// how much direct pleasure this gives the agent when it touches a body sensor
	/// </summary>
	public float endorphins;
	
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
	
	void OnCollisionEnter2D(Collision2D coll) {
		if (coll.gameObject.name == "body")
		{
			//send message to body about the pain or pleasure caused by this object.
			//which sensor on the body is it closest to?
			Vector2 touchedAt = coll.gameObject.rigidbody2D.GetPoint(coll.contacts[0].point);
			//Debug.Log(touchedAt);
			Vector2[] sensorPositions = new Vector2[]{
				new Vector2(0, 1.2f), new Vector2(0.9f, 0.9f), new Vector2(1.2f, 0), new Vector2(0.9f, -0.9f),
				new Vector2(0, -1.2f), new Vector2(-0.9f, -0.9f), new Vector2(-1.2f, 0), new Vector2(-0.9f, 0.9f)};
			float[] distances = new float[sensorPositions.Length];
			int sensorNum = 0;
			for (int i=0; i<sensorPositions.Length; i++)
			{
				distances[i] = Vector2.Distance(sensorPositions[i], touchedAt);
				if (distances[i] < distances[sensorNum])
					sensorNum = i;
			}
			//Debug.Log("closest sensor was " + sensorNum);
			GlobalVariables.outgoingMessages.Add("RD," + endorphins.ToString() + "," + sensorNum.ToString() + "\n");
			coll.gameObject.SendMessage("OnTouchedRewardOrPunishment", endorphins, SendMessageOptions.DontRequireReceiver);
			
			Screen.showCursor = true;
			Destroy(gameObject);
		}
	}
}
