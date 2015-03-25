// 
// FluvioSetTimeSettings.cs
//  
// Author:
//       Josh Montoute <josh@thinksquirrel.com>
// 
// Copyright (c) 2011-2013 Thinksquirrel Software, LLC
//
//
using UnityEngine;
using System.Collections;

[AddComponentMenu("Fluvio Example Project/Set Time Settings")]
public class FluvioSetTimeSettings : MonoBehaviour {
	
	public float deltaTime = .02f;
	public float maxDeltaTime = .0333333f;
	
	void Awake()
	{
		Time.fixedDeltaTime = deltaTime;
		Time.maximumDeltaTime = maxDeltaTime;
	}
}
