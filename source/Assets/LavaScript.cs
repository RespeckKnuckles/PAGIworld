using UnityEngine;
using System.Collections;

public class LavaScript : MonoBehaviour {

	public bool Soft = true;

	// Use this for initialization
	void Start () {
	
	}

	void OnCollisionEnter2D(Collision2D other) {
		if (other.transform.parent != null) {
			if(other.transform.root.name == "Small Wire") {
				//other.transform.root.GetComponent<WireScript>().reset_wire=true;
			}
		}
	}

	bool reverseDeformation() {
		bool done = true;
		GameObject wire = GameObject.Find("Small Wire");
		Component[] transforms = wire.GetComponentsInChildren<Transform> ();
		foreach (Transform t in transforms) {
			if(t.eulerAngles.z <= 0) {
				t.eulerAngles = new Vector3(0,0,0);
			}
			else {
				done = false;
				//Debug.Log(t.name + " is still colliding");
				for(int i =0; i<10; i++) {
					float step_size = t.eulerAngles.z/100;
					if(t.eulerAngles.z - step_size > 0 && step_size > .001) {
						t.eulerAngles = new Vector3(0,0, (t.eulerAngles.z - step_size));
					}
					else { 
						t.eulerAngles = new Vector3(0,0,0); 
					}
				}
			}
		}
		return done;
	}

	void OnCollisionStay2D(Collision2D other) {
		if (Soft) {
						bool done = false;
						if (other.transform.root.name == "Small Wire" && !done) {
								done = reverseDeformation (); 
						}
				} else {
						//UNCOMMENT FOR HARD FIX
						if (other.transform.root.name == "Small Wire") {
								Component[] transforms = other.transform.root.gameObject.GetComponentsInChildren<Transform> ();
								foreach (Transform t in transforms) {
										t.eulerAngles = new Vector3 (0, 0, 0);
								}
						}
				}
	}

	// Update is called once per frame
	void Update () {
	
	}
}
