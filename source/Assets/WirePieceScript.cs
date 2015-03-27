using UnityEngine;
using System.Collections;

public class WirePieceScript : MonoBehaviour {

	private static bool rotated = false;
	private float collision_start_time = 0;
	private bool time_set = false;
	private static int body_contact = 0;
	private static int direction = -1;

	// Use this for initialization
	void Start () {
		InvokeRepeating("setRotated",1,1F);
	}

	void setRotated() {
		rotated = false;
	}

	void OnCollisionEnter2D(Collision2D other) {

		if(other.gameObject.name == "body") { body_contact = int.Parse(gameObject.name); }

		int link_number;
		bool is_itself = int.TryParse (other.gameObject.name, out link_number);	//if collision name is not a number and therefore not part of wire
		if (!is_itself && int.Parse(gameObject.name) != 1) {
			if(!rotated) {
				if (other.gameObject.name != "rightHand" || other.gameObject.name != "leftHand") {

					float elasticity = GameObject.Find("Small Wire").GetComponent<WireScript>().Elasticity;
					float angle = Vector2.Angle(gameObject.rigidbody2D.velocity, -other.contacts[0].normal);

					if(other.relativeVelocity.magnitude >= elasticity && other.gameObject.name != "lava") {
						Debug.Log(gameObject.name + " touched " + other.gameObject.name + " with velocity " + other.relativeVelocity.magnitude + " ands angle " + angle);

						float x = gameObject.transform.eulerAngles.x;
						float y = gameObject.transform.eulerAngles.y;
						float z = gameObject.transform.eulerAngles.z;
						if(angle <= 90) {
							gameObject.transform.eulerAngles = new Vector3(x,y,z - /*45)*/ (angle*(other.relativeVelocity.magnitude))/50);
						}
						else if (angle > 90 && angle <= 180){
							gameObject.transform.eulerAngles = new Vector3(x,y,z + /*45)*/ (angle*(other.relativeVelocity.magnitude))/50);
						}
						else{}
						rotated = true;
					}
				}
			}
		}
	}

	void OnCollisionExit2D(Collision2D other) {
		if (other.gameObject.name == "body") { body_contact = 0; }
	}
	
	// Update is called once per frame
	void Update () {

		if(body_contact > 0) {
			if(GameObject.Find("body").transform.position.y - GameObject.Find(body_contact.ToString()).transform.position.y > 0 ) { direction = 0; }
			else if(GameObject.Find("body").transform.position.y - GameObject.Find(body_contact.ToString()).transform.position.y < 0) { direction = 1; }
			else {direction = -1; }

			Component [] transforms = GameObject.Find ("Small Wire").GetComponentsInChildren<Transform> ();
			foreach (Transform t in transforms) {
				if(t.GetComponent<DistanceJoint2D>() != null) {
					int start = body_contact;
					int end = int.Parse(t.name);
					if(direction == 0) {									//direction==0 : body is above wire
						for(int i = start; i <= end; i++) {
							GameObject i_string = GameObject.Find(i.ToString());
							i_string.transform.eulerAngles = new Vector3 ( i_string.transform.eulerAngles.x,
							                                              i_string.transform.eulerAngles.y, (i_string.transform.eulerAngles.z + (i*i)*.01f));
						}
					}
					else if( direction == 1) {								//direction==1 : wire is above body
						for(int i = start; i <= end; i++) {
							GameObject i_string = GameObject.Find(i.ToString());
							i_string.transform.eulerAngles = new Vector3 ( i_string.transform.eulerAngles.x,
							                                              i_string.transform.eulerAngles.y, (i_string.transform.eulerAngles.z - (i*i)*.01f));
						}
					}
					else{}
				}
			}
		}
	}
}
