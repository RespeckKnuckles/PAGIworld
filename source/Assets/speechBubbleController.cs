using UnityEngine;
using System.Collections;

public class speechBubbleController : worldObject {
		
	System.DateTime startTime, endTime;
	string displayText;
	int timeout;
	Vector2 position; //in world coords
	Vector2 topLeftPosition; //in screen point coordinates
	Vector2 size; //in pixels

	public void initialize(string displayText, int timeout, Vector2 position)
	{
		this.displayText = displayText;
		this.timeout = timeout;
		//setup lifetime timeout
		startTime = System.DateTime.Now;
		endTime = startTime.Add (new System.TimeSpan (0, 0, timeout)); 
		//estimate size of box based on text, and then resize and reposition
		float scale = Mathf.Max (1.7f, 0.031f * (float)displayText.Length);
		gameObject.transform.localScale = new Vector3 (scale, scale, 0);
		Debug.Log ("new scale: " + scale);
		size = new Vector2 (64f * scale, 40f * scale);

		//update top-left position
		this.position = position + new Vector2(0, scale*2.5f); //shift upwards so bottom edge is at the position. 4.2?
		gameObject.GetComponent<Rigidbody2D>().position = this.position;
		topLeftPosition = Camera.main.WorldToScreenPoint (this.position) - new Vector3 (size.x / 2, size.y / -2, 0);
		Debug.Log (topLeftPosition);

		objectName = "\"" + displayText.Replace ('"', '\'').Replace (',', ';') + "\"";
	}

	void OnGUI()
	{
		Color oldColor = GUI.color;
		GUI.color = Color.black;
		GUI.Label(new Rect(topLeftPosition.x, (Screen.height - topLeftPosition.y), size.x, size.y), displayText);
		GUI.color = oldColor;
	}

	// Use this for initialization
	void Start () {
	}
	
	// Update is called once per frame
	void Update () {
		if (System.DateTime.Now > endTime) {
			//Debug.Log ("destroying");
			Destroy (this.gameObject);
		}
	}
}
