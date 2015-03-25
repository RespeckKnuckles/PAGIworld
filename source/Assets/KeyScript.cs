using UnityEngine;
using System.Collections;

public class KeyScript : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	//remove key from scene and pop 1 lock off lock
	void OnTriggerEnter2D(Collider2D collision) {
		foreach (Transform child in transform.parent) {
			if (child.name == "lock") {
				child.GetComponent<LockScript>().keys--;
				break;
			}
		}
		DestroyObject (gameObject);
	}

	void OnTriggerExit2D(Collider2D collision) {

	}
	// Update is called once per frame
	void Update () {
	
	}
}
