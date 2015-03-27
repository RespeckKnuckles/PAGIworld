using UnityEngine;
using System.Collections;

public class ButtonScript : MonoBehaviour {

	// Use this for initialization
	void Start () {

	}

	void OnCollisionEnter2D(Collision2D collision) {

		if (gameObject.name.Equals("button")) {
			foreach (Transform child in transform.parent)  {
				if (child.name == "open_piece") {
					Debug.Log ("Found sibling "+child.name);
					child.GetComponent<HingeJoint2D>().useLimits=false;
				}
			}
		}
	}

	// Update is called once per frame
	void Update () {

	}
}
