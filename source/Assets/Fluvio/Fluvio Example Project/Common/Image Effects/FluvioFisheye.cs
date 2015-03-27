using UnityEngine;
using System.Collections;

[AddComponentMenu("Fluvio Example Project/Image Effects/Fisheye")]
public class FluvioFisheye : FluvioImageEffectBase {

	public float strengthX = 0.05f;
	public float strengthY = 0.05f;
	
	void OnRenderImage (RenderTexture source, RenderTexture destination) {		
				
		float oneOverBaseSize = 80.0f / 512.0f; // to keep values more like in the old version of fisheye
		
		float ar = (source.width * 1.0f) / (source.height * 1.0f);
		
		material.SetVector ("intensity", new Vector4 (strengthX * ar * oneOverBaseSize, strengthY * oneOverBaseSize, strengthX * ar * oneOverBaseSize, strengthY * oneOverBaseSize));
		Graphics.Blit (source, destination, material); 	
	}
}