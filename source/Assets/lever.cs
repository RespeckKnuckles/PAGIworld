using UnityEngine;
using System.Collections;

public class lever : worldObject {
	public GameObject beam;
	// Use this for initialization
	private bool active = false;
	private bool reset = true;
	private HingeJoint2D h;
	private JointMotor2D j;
	void Start () {
		h = beam.GetComponent<HingeJoint2D> ();
	}
	void FixBeam()
	{
		Debug.Log ("FIXING");
		if (h.jointAngle > 0) 
		{
			j.motorSpeed = -20;
			j.maxMotorTorque = 400000;
			h.motor = j;
			h.useMotor = true;
			/*while (h.jointAngle >0) 
			{
				continue;
			}*/
			j.motorSpeed = 0;
			h.motor = j;
		}
		if (h.jointAngle < 0) 
		{
			j.motorSpeed = 20;
			j.maxMotorTorque = 400000;
			h.motor = j;
			h.useMotor = true;
			/*while (h.jointAngle < 0) 
			{
				continue;
			}*/
			j.motorSpeed = 0;
			h.motor = j;
		}
	}
	// Update is called once per frame
	void Update () {
		if (gameObject.GetComponent<HingeJoint2D> ().jointAngle <= -30) {
			if (reset == true) {
				if (active == false) {
					active = true;
					reset = false;
					FixBeam();
				}
				else if (active == true) {
					active = false;
					reset = false;
					h.useMotor = false;
				}
			}
		}
		if (gameObject.GetComponent<HingeJoint2D> ().jointAngle >= 30) {
			reset = true;
		}
	}
}
