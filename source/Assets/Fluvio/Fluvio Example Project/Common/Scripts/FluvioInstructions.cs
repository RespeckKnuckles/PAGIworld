// 
// FluvioInstructions.cs
//  
// Author:
//       Josh Montoute <josh@thinksquirrel.com>
// 
// Copyright (c) 2011-2013 Thinksquirrel Software, LLC
//
//
using UnityEngine;
using System.Collections;

[AddComponentMenu("Fluvio Example Project/Instructions")]
public class FluvioInstructions : MonoBehaviour {
	
	public bool showCameraControls = true;
	public bool showAltCameraControls = false;
	public bool showClickText = true;
	public bool showClickModifier = false;
	public bool showToggleInterface = true;
	
	void Awake()
	{
#if (UNITY_IPHONE || UNITY_ANDROID) && !UNITY_EDITOR
		guiText.text = "Tap - Pull fluid";
#else
		string cam = showCameraControls ?
			"Mouse - Orbit/Pan/Zoom | "
			: (showAltCameraControls ? "WASD/MouseLook Camera  | " :
			"");
		
		string click = "";
		
		if (showClickText)
		{
			click = showClickModifier ?
				" | Shift + L/R click - Pull/Push fluid"
				:
				" | L/R click - Pull/Push fluid";
		}
		
		string tog = showToggleInterface ? " | X - Toggle Interface" : "";
		
		guiText.text = 
			cam +
			"Space - Slow motion | " +
			"L - Debug Information" +
			click + tog;
#endif
	}
}
