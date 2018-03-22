using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class explodingObjectController : worldObject {
	
	public Sprite explosionSprite;
	public GameObject explosionIgniter; //the object which, when it touches this one, causes an explosion
	public bool isExploding = false;
	public int explosionRemaining = 70;
	
	// Use this for initialization
	void Start () {
	}
	
	// Update is called once per frame
	void Update () {
		if (isExploding)
		{
			if (explosionRemaining > 0)
				explosionRemaining--;
			else
				Destroy(gameObject);
		}
	}
	
	void OnCollisionEnter2D(Collision2D collision) {
		//List<string> acceptableTransformers = new List<string>{"medkit", "medkit_large", "healthpack_lefthalf", "healthpack_righthalf"};
		//if (acceptableTransformers.Contains(collision.gameObject.name))
		string collisionObjName = collision.gameObject.name;
		if (collisionObjName.EndsWith("(Clone)"))
			collisionObjName = collisionObjName.Substring(0, collisionObjName.Length-7);
		//Debug.Log("new name is " + collisionObjName);
		//Debug.Log("looking for " + explosionIgniter.name);
		if (collisionObjName == explosionIgniter.name)
		{
			//find all objects that this object is connected to
			//GameObject p = collision.gameObject.transform.parent.gameObject;
			//foreach (worldObject c in p.transform.GetComponentsInChildren<worldObject>())
			//	Destroy(c.gameObject);
			Destroy(collision.gameObject);
			isExploding = true;
			GetComponent<SpriteRenderer>().sprite = explosionSprite;
			//Destroy(gameObject);
			gameObject.GetComponent<Rigidbody2D>().velocity.Set(0,0);
		}
	}
}