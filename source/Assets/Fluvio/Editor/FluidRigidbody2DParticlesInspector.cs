// 
// FluidRigidbody2DParticles.cs
//  
// Author:
//       Josh Montoute <josh@thinksquirrel.com>
// 
// Copyright (c) 2011-2014 Thinksquirrel Software, LLC
//
#if !(UNITY_3_5 || UNITY_4_0 || UNITY_4_1 || UNITY_4_2)
using UnityEngine;
using UnityEditor;
using ThinksquirrelSoftware.Fluvio.Plugins;

namespace ThinksquirrelSoftware.FluvioEditor
{
	[CustomEditor(typeof(FluidRigidbody2DParticles))]
	[CanEditMultipleObjects]
	public class FluidRigidbody2DParticlesInspector : FluidPluginInspector
	{
		public override void OnInspectorGUI()
		{
			DrawInspector();
		}
	}
}
#endif