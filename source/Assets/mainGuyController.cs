using UnityEngine;
using System.Collections;

public class mainGuyController : worldObject {

	// Use this for initialization
	void Start () {
		temperature = 50f;
		for (int i=0; i<texture.Length; i++) texture[i] = 1.0f;
	}
	
	// Update is called once per frame
	void Update () {
		//Debug.Log ("velo" + rigidbody2D.velocity);
	}
}