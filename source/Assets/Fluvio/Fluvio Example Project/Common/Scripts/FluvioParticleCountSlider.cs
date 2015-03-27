using UnityEngine;
using System.Collections;
using ThinksquirrelSoftware.Fluvio.Emitters;

[AddComponentMenu("Fluvio Example Project/Particle Count Slider")]
public class FluvioParticleCountSlider : MonoBehaviour {
	
	public Rect rect;
	public FluidEmitter emitter;
	public float min = 0;
	public float max = 800;
	
	public Rect guiRect { get; private set; }
	void OnGUI()
	{
		guiRect = new Rect(Screen.width - rect.width - rect.x, rect.y, rect.width, rect.height);
		GUILayout.BeginArea(guiRect);
		GUILayout.Label("Max Particles: " + emitter.maxParticles.ToString());
		emitter.maxParticles = (int)GUILayout.HorizontalSlider(emitter.maxParticles, min, max);
		GUILayout.EndArea();
	}
}
