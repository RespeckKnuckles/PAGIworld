using UnityEngine;
using System.Collections;

public class WireScript : MonoBehaviour {

	public bool reset_wire = false;
	public float Elasticity = 1;
	private float previous_time;

	// Use this for initialization
	void Start () {
		Component [] rbs = GetComponentsInChildren<Rigidbody2D> ();
		foreach (Rigidbody2D rb in rbs) {
			rb.fixedAngle=true;
		}
	}

	void reset() {
		Component [] transforms = GetComponentsInChildren<Transform> ();
		foreach (Transform t in transforms) {
			t.eulerAngles = new Vector3(0,0,0);
		}
		reset_wire = false;
	}

	// Update is called once per frame
	void Update () {
		if(Input.GetKeyUp(KeyCode.Space)) {
			Component[] distance_joints = GetComponentsInChildren<DistanceJoint2D> ();
			foreach (DistanceJoint2D j in distance_joints) { Destroy(j);}
		}
		if (reset_wire) {reset();}
	}
}
