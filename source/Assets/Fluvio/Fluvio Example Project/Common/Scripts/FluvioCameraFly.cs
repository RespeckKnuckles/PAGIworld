using UnityEngine;
using System.Collections;

[AddComponentMenu("Fluvio Example Project/Camera Fly")]
public class FluvioCameraFly : MonoBehaviour
{ 
	public float mainSpeed = 100.0f;
	public float cameraSensitivity = 3.0f;
	
	Vector3 lastMouse = new Vector3(255, 255, 255);
	bool initialized = false;
	
	void Start()
	{
		Screen.lockCursor = true;
		Screen.showCursor = false;
		initialized = true;
	}

	void OnEnable()
	{
		if (initialized)
		{
			Screen.lockCursor = true;
			Screen.showCursor = false;
		}
	}

	void OnDisable()
	{
		Screen.lockCursor = false;
		Screen.showCursor = true;
	}

	void Update ()
	{	
	    lastMouse = new Vector3(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"), 0); 
	    lastMouse = new Vector3(-lastMouse.y * cameraSensitivity, lastMouse.x * cameraSensitivity, 0 );
	    lastMouse = new Vector3(transform.eulerAngles.x + lastMouse.x , transform.eulerAngles.y + lastMouse.y, 0); 
	
	    transform.eulerAngles = lastMouse;
	
	    Vector3 p = GetBaseInput(); 
	
		p = p * mainSpeed;
	    p = p * Time.deltaTime;
	
	   transform.Translate(p);
	}

 
	Vector3 GetBaseInput()
	{
	    Vector3 p_Velocity = Vector3.zero;
	
	    if (Input.GetKey(KeyCode.W))
	        p_Velocity.z += 1;
	    if (Input.GetKey (KeyCode.S))
	        p_Velocity.z -= 1;
	    if (Input.GetKey (KeyCode.A))
	        p_Velocity.x -= 1;
	    if (Input.GetKey (KeyCode.D))
	        p_Velocity.x += 1;
		
	    return p_Velocity;
	}
}
