using UnityEngine;
using System.Collections;

public class RungScript : MonoBehaviour {

	// Use this for initialization
	void Start () {
		InvokeRepeating("setRungs",1,.5F);
	}

	void setRungs() {
		GameObject body = GameObject.Find ("body");
		float x_min = gameObject.collider2D.bounds.center.x - gameObject.collider2D.bounds.extents.x;
		float x_max = gameObject.collider2D.bounds.center.x + gameObject.collider2D.bounds.extents.x;
		if (body.transform.position.x > x_min && body.transform.position.x < x_max) {

			foreach (Transform child in transform) {
				if (child.name == "apple") { continue; }

				if (body.transform.position.y > child.transform.position.y) {
					child.collider2D.isTrigger = false;
					if (child.name != "base") {
						child.GetComponent<HingeJoint2D> ().useLimits = true;
						//COMMENT FOR HARD MODE
						/*for (int rung_index = int.Parse(child.name) + 1; rung_index<transform.childCount-1; rung_index++) {
							transform.GetChild(rung_index).collider2D.isTrigger=true;
							if(transform.GetChild(rung_index).name != "base") {
								transform.GetChild(rung_index).GetComponent<HingeJoint2D>().useLimits=false;
								if(rung_index ==transform.childCount-2) {
									transform.GetChild(rung_index).GetComponent<HingeJoint2D>().useLimits=true;
								}
							}
						}*/
						//-------------------------------------------------------------------------------
					} else { GameObject.Find ("apple").rigidbody2D.isKinematic = false; }
					break;
				}
				if (body.transform.position.y < child.transform.position.y) {
					child.collider2D.isTrigger = true;
					if (child.name != "base") {
						child.GetComponent<HingeJoint2D> ().useLimits = false;
					} else { GameObject.Find ("apple").rigidbody2D.isKinematic = true; }
				}
			}
		}
		else { 
			foreach (Transform child in transform) {
				if (child.name == "apple") { continue; }
				child.collider2D.isTrigger = true;
				if (child.name != "base") {
					child.GetComponent<HingeJoint2D> ().useLimits = false;
				} else { GameObject.Find ("apple").rigidbody2D.isKinematic = true; }
			}
		}
	}

	// Update is called once per frame
	void Update () {

	}
}
