using UnityEngine;
using System.Collections;
using ThinksquirrelSoftware.Fluvio.Plugins;

[AddComponentMenu("Fluvio Example Project/Underwater Effects")]
public class FluvioUnderwaterEffects : MonoBehaviour {
	
	public GameObject aboveWaterObject;
	public GameObject underwaterObject;
	public AudioReverbZone reverbZone;
	public FluvioFisheye fishEye;
	public FluvioTwirlEffect twirlEffect;
	public MonoBehaviour globalFog;
	public ParticleSystem bubbles;
	public Projector[] projectors;
	
	void OnTriggerEnter()
	{
		if (reverbZone)
			reverbZone.enabled = true;
		
		if (aboveWaterObject && underwaterObject)
		{
#if UNITY_3_5
			aboveWaterObject.SetActiveRecursively(false);
			underwaterObject.SetActiveRecursively(true);
#else
			aboveWaterObject.SetActive(false);
			underwaterObject.SetActive(true);
#endif
		}
		
		if (fishEye)
			fishEye.enabled = true;
		
		if (twirlEffect)
			twirlEffect.enabled = true;
		
		if (globalFog)
			globalFog.enabled = true;
		
		if (bubbles)
		{
			bubbles.GetComponent<Renderer>().enabled = true;
		}
		
		foreach(Projector p in projectors)
		{
			p.enabled = true;
		}
	}
	
	void LateUpdate()
	{
		if (!reverbZone.enabled)
			return;
		
		if (fishEye)
		{
			fishEye.strengthX = Mathf.Sin(Time.time) * .1f;
			fishEye.strengthY = Mathf.Cos(Time.time) * .1f;
		}
		
		if (twirlEffect)
		{
			twirlEffect.angle = Mathf.Sin(Time.time * 5f) * 2f;
		}
	}
	
	void OnTriggerExit()
	{
		if (reverbZone)
			reverbZone.enabled = false;
		
		if (aboveWaterObject && underwaterObject)
		{
#if UNITY_3_5
			aboveWaterObject.SetActiveRecursively(true);
			underwaterObject.SetActiveRecursively(false);
#else
			aboveWaterObject.SetActive(true);
			underwaterObject.SetActive(false);
#endif
		}
		
		if (fishEye)
			fishEye.enabled = false;
		
		if (twirlEffect)
			twirlEffect.enabled = false;
		
		if (globalFog)
			globalFog.enabled = false;
		
		if (bubbles)
		{
			bubbles.GetComponent<Renderer>().enabled = false;
		}
		
		foreach(Projector p in projectors)
		{
			p.enabled = false;
		}
	}
}
