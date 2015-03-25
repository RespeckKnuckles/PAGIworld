using UnityEngine;
using System.Collections;


public class dispenserScript : worldObject {


	public GameObject apple;
	public GameObject poison;

	public string type;					//type of dispenser
	public int rand_denominator;		//a 1 in rand_denominator chance of spawning an apple on press
	public int extinction_count;		//the number of presses before extinction
	public Vector3 dispense_offset;		//where to spawn the gameObject relative to the dispenser

	bool canDispense = true;
	int extC = 0;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		if (gameObject.GetComponent<DistanceJoint2D> () != null && canDispense == true) {
			Debug.Log ("Being grabbed!");
			canDispense = false;
			int num = 0;
			switch(type){
				case "good":
					Instantiate(apple);
					apple.transform.position = gameObject.transform.position;
					apple.transform.position += new Vector3(dispense_offset.x, dispense_offset.y, dispense_offset.z);
					break;
				case "bad":
					Instantiate(poison);
					poison.transform.position = gameObject.transform.position;
					poison.transform.position += new Vector3(dispense_offset.x, dispense_offset.y, dispense_offset.z);
					break;
				case "good_rand":
					num = Random.Range(1, rand_denominator+1);
					if(num == 1){
						Instantiate(apple);
						apple.transform.position = gameObject.transform.position;
						apple.transform.position += new Vector3(dispense_offset.x, dispense_offset.y, dispense_offset.z);
					}
					break;
				case "extinction_hard":
					if(extC<extinction_count){
						Instantiate(apple);
						apple.transform.position = gameObject.transform.position;
						apple.transform.position += new Vector3(dispense_offset.x, dispense_offset.y, dispense_offset.z);
					}
					else{
						Instantiate(poison);
						poison.rigidbody2D.position = gameObject.transform.position;
						poison.transform.position += new Vector3(dispense_offset.x, dispense_offset.y, dispense_offset.z);
					}
					extC++;
					break;
				case "extinction_soft":
					if(extC<extinction_count){
						Instantiate(apple);
						apple.transform.position = gameObject.transform.position;
						apple.transform.position += new Vector3(dispense_offset.x, dispense_offset.y, dispense_offset.z);
					}
					extC++;
					break;
				case "extinction_rand":
					if(extC<extinction_count){
						num = Random.Range(1, rand_denominator+1);
						if(num == 1){
							extC++;
							Instantiate(apple);
							apple.transform.position = gameObject.transform.position;
							apple.transform.position += new Vector3(dispense_offset.x, dispense_offset.y, dispense_offset.z);
						}
					}
					
					break;
			}

		}
		else if(gameObject.GetComponent<DistanceJoint2D> () != null && canDispense == false){

		}
		else{
			canDispense = true;
		}
	}
}
