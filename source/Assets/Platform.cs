using UnityEngine;
using System.Collections;

public class Platform : MonoBehaviour {
	
	private float useSpeed;
	public float directionSpeed = 9.0f;
	float origX,origY;
	public float distance = 10.0f;
	public bool vertical = false;
	
	// Use this for initialization
	void Start ()
	{
		origX = transform.position.x;
		origY = transform.position.y;
		useSpeed = -directionSpeed;
	}

	// Update is called once per frame
	void Update ()
	{
		if (!vertical) {
						if (origX - transform.position.x > distance) {
								useSpeed = directionSpeed; //flip direction
						} else if (origX - transform.position.x < -distance) {
								useSpeed = -directionSpeed; //flip direction
						}
						transform.Translate (useSpeed * Time.deltaTime, 0, 0);
				} else {
						if (origY - transform.position.y > distance) {
								useSpeed = directionSpeed; //flip direction
						} else if (origY - transform.position.y < -distance) {
								useSpeed = -directionSpeed; //flip direction
						}
						transform.Translate (0, useSpeed * Time.deltaTime, 0);
				}
	}
}


