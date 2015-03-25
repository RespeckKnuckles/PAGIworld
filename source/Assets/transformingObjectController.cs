using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class transformingObjectController : worldObject {

	public Sprite beforeSprite;
	public Sprite afterSprite;
	public bool isBeforeState = true;
	public string beforeType;
	public string afterType;
	
	// Use this for initialization
	void Start () {
		objectType = beforeType;
	}
	
	// Update is called once per frame
	void Update () {
		if (!isBeforeState)
			setToAfterState();
		else
			setToBeforeState();
	}
	
	void setToAfterState()
	{
		isBeforeState = false;
		GetComponent<SpriteRenderer>().sprite = afterSprite;
		objectType = afterType;
	}
	
	void setToBeforeState()
	{
		isBeforeState = true;
		GetComponent<SpriteRenderer>().sprite = beforeSprite;
		objectType = beforeType;
	}
	
	void OnCollisionEnter2D(Collision2D collision) {
		List<string> acceptableTransformers = new List<string>{"medkit", "medkit_large", "healthpack_lefthalf", "healthpack_righthalf"};
		
		string collisionObjName = collision.gameObject.name;
		if (collisionObjName.EndsWith("(Clone)"))
			collisionObjName = collisionObjName.Substring(0, collisionObjName.Length-7);
			
		if (acceptableTransformers.Contains(collisionObjName))
		{
			//find all objects that this object is connected to
			Transform t = collision.gameObject.transform.parent;
			if (t)
			{//it has no parent
				GameObject p = t.gameObject;
				foreach (worldObject c in p.transform.GetComponentsInChildren<worldObject>())
					Destroy(c.gameObject);
				//Destroy(collision.gameObject);
			}
			else
				Destroy(collision.gameObject);
			setToAfterState();
		}
	}
}
