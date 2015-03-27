using UnityEngine;
using System.Collections;

public class GhostPlatform : MonoBehaviour {

	
	// Use this for initialization
	void Start ()
	{

	}
	void OnTriggerEnter2D(Collider2D other) {
		other.transform.parent = gameObject.transform;
	}

	void OnTriggerExit2D(Collider2D other) {
		other.transform.parent = null;
		//other.transform.localScale = new Vector2(1,1);
	}

	// Update is called once per frame
	void Update ()
	{

	}
}



