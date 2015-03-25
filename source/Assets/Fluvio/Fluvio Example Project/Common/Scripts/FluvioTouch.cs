// 
// FluvioTouch.cs
//  
// Author:
//       Josh Montoute <josh@thinksquirrel.com>
// 
// Copyright (c) 2011-2013 Thinksquirrel Software, LLC
//
//
using UnityEngine;
using ThinksquirrelSoftware.Fluvio;
using ThinksquirrelSoftware.Fluvio.ObjectModel;
using System.Collections;

[AddComponentMenu("Fluvio Example Project/Touch Controls")]
public class FluvioTouch : MonoBehaviour {
	
	private enum TouchMode
	{
		Pull = 1,
		Push = -1
	}
	public Fluid fluid;
	public float force = 100f;
	public float maxForce = 200f;
	public float maxDistance = 10f;
	public bool requireClick = true;
	public bool requireModifier = true;
	public bool invert = false;
	TouchMode touchMode = TouchMode.Pull;
	public Transform target;
	
	FluidParticle[] particles;
	
	private float z;
	private bool sleep;
	
	public void Sleep()
	{
		sleep = true;
	}
	
	void OnEnable()
	{
		fluid.OnPostSolve += OnPostSolve;
	}
	
	void OnDisable()
	{
		fluid.OnPostSolve -= OnPostSolve;
	}
	
	void Update()
	{
		z = target ? Vector3.Distance(Camera.main.transform.position, target.position) : -Camera.main.transform.position.z;
	}
	
	void OnPostSolve(FluvioTimeStep timeStep)
	{
		if (sleep)
		{
			sleep = false;
			return;
		}
		
		fluid.GetParticles(ref particles);
		
#if !(UNITY_IPHONE || UNITY_ANDROID) || UNITY_EDITOR
		bool doTouch = !requireClick ? true : (Input.GetMouseButton(0) || Input.GetMouseButton(1));
		if (doTouch)
		{
			if (Input.GetMouseButton(0) || !requireClick)
			{
				touchMode = invert ? TouchMode.Push : TouchMode.Pull;
			}
			else
			{
				touchMode = invert ? TouchMode.Pull : TouchMode.Push;
			}
			Vector3 point = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, z));
			float f = (int)touchMode * force;
			for(int i = 0; i < particles.Length; i++)
			{
                FluidParticle p = particles[i];
				
				if (!p.enabled)
					continue;

				float dist = (p.position - point).sqrMagnitude;
				if (dist <= maxDistance *  maxDistance)
				{
					p.AddForce((point - p.position).normalized * f);
				}
				
				particles[i] = p;
			}
		}
#else
		foreach(Touch t in Input.touches)
		{
			Vector3 point = Camera.main.ScreenToWorldPoint(new Vector3(t.position.x, t.position.y, z));
			float f = force * (int)touchMode / (Input.touchCount * .5f);
			for(int i = 0; i < particles.Length; i++)
			{
				FluidParticle p = particles[i];
				
				if (!p.enabled)
					continue;
				
				float dist = (p.position - point).sqrMagnitude;
				if (dist <=  maxDistance * maxDistance)
				{
					p.AddForce((point - p.position).normalized * f);
				}
				
				particles[i] = p;
			}
		}
#endif
        fluid.SetParticles(particles, particles.Length);
	}
}