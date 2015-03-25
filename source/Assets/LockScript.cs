using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class LockScript : MonoBehaviour {
	public GameObject key_prefab;
	public int keys=0;

	// Use this for initialization
	void Start () {
		foreach (Transform child in transform.parent) { if (child.name == "key") {keys++;} }
	}

	// Update is called once per frame
	void Update () {
		if (keys.Equals(0)) {					//if all locks are unlocked
			foreach (Transform child in transform.parent) {
				if (child.name == "open_piece") {
					child.GetComponent<HingeJoint2D> ().useLimits = false;
					break;
				}
			}
		}
	}
}
