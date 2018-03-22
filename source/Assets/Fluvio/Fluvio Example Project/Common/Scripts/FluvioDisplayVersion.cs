// 
// DisplayVersion.cs
//  
// Author:
//       Josh Montoute <josh@thinksquirrel.com>
// 
// Copyright (c) 2011-2013 Thinksquirrel Software, LLC
//
//
using UnityEngine;
using System.Collections;
using ThinksquirrelSoftware.Fluvio;

[RequireComponent(typeof(GUIText))]
[AddComponentMenu("Fluvio Example Project/Version Display")]
public class FluvioDisplayVersion : MonoBehaviour {

	void Awake()
	{
		GetComponent<GUIText>().text = "Fluvio v. " + VersionInfo.version;
	}
}
