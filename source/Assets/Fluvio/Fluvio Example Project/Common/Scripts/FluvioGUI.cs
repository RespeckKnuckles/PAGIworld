// 
// FluvioGUI.cs
//  
// Author:
//       Josh Montoute <josh@thinksquirrel.com>
// 
// Copyright (c) 2011-2013 Thinksquirrel Software, LLC
//
//
using ThinksquirrelSoftware.Fluvio;
using ThinksquirrelSoftware.Fluvio.Emitters;
using ThinksquirrelSoftware.Fluvio.Plugins;
using UnityEngine;

[AddComponentMenu("Fluvio Example Project/Fluvio GUI")]
public class FluvioGUI : MonoBehaviour {
	
	private static Rect areaRect = new Rect(4, 184, 510, 256);
	private static bool closeWindow = true;
	public FluvioCameraFly cameraFly;
	public FluvioTouch exampleTouch;
	public GUILayer guiLayer;
	public FluvioParticleCountSlider particleCountSlider;
	public FluvioQualitySettings qualitySettings;
	public bool allowInteractive = true;
	public bool allowTransform = true;
	public bool allowReset = true;
	public bool allowToggle = true;
	private bool doSleep;
	private bool cancelSleep;
	
	private float interactiveX;
	private float interactiveY;
	private float interactiveZ;
	
	private float positionX;
	private float positionY;
	private float positionZ;
	
	private float rotationX;
	private float rotationY;
	private float rotationZ;
	
	private Fluid[] fluids;
	private FluidEmitter[] emitters = new FluidEmitter[10];
	
	private bool dontDraw = false;
	
	void Awake()
	{
		fluids = Fluid.All();
	}
	
	void OnGUI()
	{	
		if (!dontDraw)
			areaRect = ClampToScreen(GUILayout.Window(0, areaRect, DoWindow, "Fluvio 2"));
	}
	
	void DoWindow(int windowID)
	{
		if (GUI.Button(new Rect(areaRect.width - 22, 2, 20, 15), "-"))
		{
			closeWindow ^= true;
			if (!closeWindow)
			{
				areaRect.width = 170;
				if (allowInteractive)
					areaRect.width += 170;
				if (allowTransform)
					areaRect.width += 170;
				areaRect.height = 256;
			}
			
		}
		
		if (closeWindow)
		{
			areaRect.width = 250;
			areaRect.height = 70;
			GUILayout.BeginHorizontal();
			GUILayout.Label(Application.loadedLevelName);
			if (Application.levelCount > 1)
			{
				if (GUILayout.Button("<", GUILayout.Width(25)))
				{
					if (Application.loadedLevel == 0)
					{
						Application.LoadLevel(Application.levelCount - 1);
					}
					else
					{
						Application.LoadLevel(Application.loadedLevel - 1);
					}
				}
			}
				if (GUILayout.Button("R", GUILayout.Width(25)))
				{
					Application.LoadLevel(Application.loadedLevel);
				}
			if (Application.levelCount > 1)
			{
				if (GUILayout.Button(">", GUILayout.Width(25)))
				{
					if (Application.loadedLevel == Application.levelCount - 1)
					{
						Application.LoadLevel(0);
					}
					else
					{
						Application.LoadLevel(Application.loadedLevel + 1);
					}
				}
			}
			GUILayout.EndHorizontal();
			GUILayout.FlexibleSpace();
			GUI.DragWindow();
			return;
		}
		
		GUILayout.BeginHorizontal();	
		
		if (allowInteractive)
		{
			GUILayout.BeginVertical(GUILayout.Width(170));
			GUILayout.FlexibleSpace();
			
			GUILayout.Label("Interactive Force: (" + (int)interactiveX + "," + (int)interactiveY + "," + (int)interactiveZ + ")");
			
			GUILayout.BeginHorizontal();
			if (GUILayout.Button("X", GUILayout.Width(30)))
			{
				interactiveX = 0;
			}
			interactiveX = GUILayout.HorizontalSlider(interactiveX, -100, 100, GUILayout.ExpandWidth(true));
			GUILayout.EndHorizontal();
			
			GUILayout.BeginHorizontal();
			if (GUILayout.Button("Y", GUILayout.Width(30)))
			{
				interactiveY = 0;
			}
			interactiveY = GUILayout.HorizontalSlider(interactiveY, -100, 100, GUILayout.ExpandWidth(true));
			GUILayout.EndHorizontal();
			
			GUILayout.BeginHorizontal();
			if (GUILayout.Button("Z", GUILayout.Width(30)))
			{
				interactiveZ = 0;
			}
			interactiveZ = GUILayout.HorizontalSlider(interactiveZ, -100, 100, GUILayout.ExpandWidth(true));
			GUILayout.EndHorizontal();
			
			GUILayout.FlexibleSpace();
			GUILayout.EndVertical();
		}

		if (allowTransform)
		{
			
			GUILayout.BeginVertical(GUILayout.Width(170));
			GUILayout.FlexibleSpace();
			
			GUILayout.Label("Position: (" + (int)positionX + "," + (int)positionY + "," + (int)positionZ + ")");
			
			GUILayout.BeginHorizontal();
			if (GUILayout.Button("X", GUILayout.Width(30)))
			{
				positionX = 0;
			}
			positionX = GUILayout.HorizontalSlider(positionX, -100, 100, GUILayout.ExpandWidth(true));
			GUILayout.EndHorizontal();
			
			GUILayout.BeginHorizontal();
			if (GUILayout.Button("Y", GUILayout.Width(30)))
			{
				positionY = 0;
			}
			positionY = GUILayout.HorizontalSlider(positionY, -100, 100, GUILayout.ExpandWidth(true));
			GUILayout.EndHorizontal();
			
			GUILayout.BeginHorizontal();
			if (GUILayout.Button("Z", GUILayout.Width(30)))
			{
				positionZ = 0;
			}
			positionZ = GUILayout.HorizontalSlider(positionZ, -100, 100, GUILayout.ExpandWidth(true));
			GUILayout.EndHorizontal();
			
			GUILayout.Label("Rotation: (" + (int)rotationX + "," + (int)rotationY + "," + (int)rotationZ + ")");
			
			GUILayout.BeginHorizontal();
			if (GUILayout.Button("X", GUILayout.Width(30)))
			{
				rotationX = 0;
			}
			rotationX = GUILayout.HorizontalSlider(rotationX, -180, 180, GUILayout.ExpandWidth(true));
			GUILayout.EndHorizontal();
			
			GUILayout.BeginHorizontal();
			if (GUILayout.Button("Y", GUILayout.Width(30)))
			{
				rotationY = 0;
			}
			rotationY = GUILayout.HorizontalSlider(rotationY, -180, 180, GUILayout.ExpandWidth(true));
			GUILayout.EndHorizontal();
			
			GUILayout.BeginHorizontal();
			if (GUILayout.Button("Z", GUILayout.Width(30)))
			{
				rotationZ = 0;
			}
			rotationZ = GUILayout.HorizontalSlider(rotationZ, -180, 180, GUILayout.ExpandWidth(true));
			GUILayout.EndHorizontal();
			
			GUILayout.FlexibleSpace();
			GUILayout.EndVertical();
		}
		
		GUILayout.BeginVertical(GUILayout.Width(170));
		GUILayout.FlexibleSpace();
		
		GUILayout.Label(Application.loadedLevelName);
		if (GUILayout.Button("Restart Scene"))
		{
			Application.LoadLevel(Application.loadedLevel);
		}
		
		GUILayout.BeginHorizontal();
		if (Application.levelCount > 1)
		{
			if (GUILayout.Button("Prev"))
			{
				if (Application.loadedLevel == 0)
				{
					Application.LoadLevel(Application.levelCount - 1);
				}
				else
				{
					Application.LoadLevel(Application.loadedLevel - 1);
				}
			}
			if (GUILayout.Button("Next"))
			{
				if (Application.loadedLevel == Application.levelCount - 1)
				{
					Application.LoadLevel(0);
				}
				else
				{
					Application.LoadLevel(Application.loadedLevel + 1);
				}
			}
		}
		GUILayout.EndHorizontal();
		
		if (GUILayout.Button("Toggle Fullscreen"))
		{
			if (Screen.fullScreen)
			{
				Screen.fullScreen = false;
			}
			else
			{
				Resolution res = Screen.currentResolution;
				Screen.SetResolution(res.width, res.height, true, res.refreshRate);
			}
		}
		
		GUILayout.FlexibleSpace();
		GUILayout.EndVertical();
		
		GUILayout.EndHorizontal();
		
		if (allowReset)
		{
			if (GUILayout.Button("Reset Fluid(s)"))
			{
				foreach(Fluid fluid in fluids)
				{
					fluid.ResetObject();
				}
			}
		}
		
		GUI.DragWindow();
	}
	
	Rect ClampToScreen(Rect r)
	{
	    r.x = Mathf.Clamp(r.x,0,Screen.width-r.width);
	    r.y = Mathf.Clamp(r.y,0,Screen.height-r.height);
	    return r;
	}
	
	void Update()
	{	
		Vector2 m = new Vector2(Input.mousePosition.x, Screen.height - Input.mousePosition.y);
		Rect rect1 = areaRect;
		Rect rect2 = particleCountSlider ? particleCountSlider.guiRect : rect1;
		Rect rect3 = qualitySettings ? qualitySettings.guiRect : rect1;
		
		if (cameraFly)
		{
			if (cameraFly.enabled)
			{
				if (Input.GetKeyUp(KeyCode.Escape))
					cameraFly.enabled = false;
			}
			else
			{
				if ((Input.GetMouseButtonUp(0) || Input.GetMouseButtonUp(1)) && !rect1.Contains(m) && !rect2.Contains(m) && !rect3.Contains(m))
					cameraFly.enabled = true;
			}
		}
		
		if (Input.GetKeyUp(KeyCode.X))
			dontDraw ^= true;
		
		if (guiLayer)
			guiLayer.enabled = !dontDraw;
		
		if (particleCountSlider)
			particleCountSlider.enabled = !dontDraw;
		
		if (qualitySettings)
			qualitySettings.enabled = !dontDraw;
		
		if (dontDraw)
			return;
		
		if (rect1.Contains(m) || rect2.Contains(m) || rect3.Contains(m))
		{
			doSleep = true;
		}
		else if (Input.GetMouseButtonDown(0))
		{
			cancelSleep = true;
		}
		if (Input.GetMouseButtonUp(0))
		{
			cancelSleep = false;
		}
		if (cancelSleep)
		{
			doSleep = false;
			return;
		}
		if (doSleep)
		{
			if (Input.GetMouseButton(0))
			{	
				if (exampleTouch)
					exampleTouch.Sleep();
			}
			else
			{
				doSleep = false;
			}
		}
	}
	
	void FixedUpdate()
	{
		FluidEmitter emitter;
		
		foreach(Fluid fluid in fluids)
		{
			if (allowInteractive)
			{
				fluid.interactiveForce = new Vector3(interactiveX, interactiveY, interactiveZ);
			}
			if (allowTransform)
			{
				int c = fluid.GetEmitters(emitters);
				
				for(int i = 0; i < c; i++)
				{
					emitter = emitters[i];
					if (emitter)
					{
						emitter.cachedTransform.localPosition = new Vector3(positionX, positionY, positionZ);
						emitter.cachedTransform.localEulerAngles = new Vector3(rotationX, rotationY, rotationZ);
					}
				}
			}
		}
	}
}