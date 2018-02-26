using UnityEngine;
using System.Collections;

[AddComponentMenu("Fluvio Example Project/Quality Settings")]
public class FluvioQualitySettings : MonoBehaviour {
	
	[System.Serializable]
	public class ShadowSettings
	{
		public LightShadows shadows;
		public ShadowProjection shadowProjection;
		public int shadowCascades;
		public float shadowDistance;
		
		public ShadowSettings(LightShadows shadows, ShadowProjection shadowProjection, int shadowCascades, float shadowDistance)
		{
			this.shadows = shadows;
			this.shadowProjection = shadowProjection;
			this.shadowCascades = shadowCascades;
			this.shadowDistance = shadowDistance;
		}
	}
	
	public Rect guiRect { get; private set; }
	
	// V-sync (isn't affected by quality settings)
	public bool vSync;
	
	// Particles
	public bool disableMediumParticleRenderersOnly;
	public GameObject[] mediumQualityParticleSystems;
	public bool disableHighParticleRenderersOnly;
	public GameObject[] highQualityParticleSystems;
	public bool disableUltraParticleRenderersOnly;
	public GameObject[] ultraQualityParticleSystems;
	
	// Textures
	public int lowQualityTextureSetting = 2;
	public int mediumQualityTextureSetting = 1;
	public int highQualityTextureSetting = 0;
	
	// Shadows
	public ShadowSettings lowQualityShadowSetting = new ShadowSettings(LightShadows.Hard, ShadowProjection.CloseFit, 0, 30);
	public ShadowSettings mediumQualityShadowSetting = new ShadowSettings(LightShadows.Hard, ShadowProjection.CloseFit, 0, 50);
	public ShadowSettings highQualityShadowSetting = new ShadowSettings(LightShadows.Soft, ShadowProjection.CloseFit, 2, 60);
	public ShadowSettings ultraQualityShadowSetting = new ShadowSettings(LightShadows.Soft, ShadowProjection.CloseFit, 4, 60);
	
	// Level of detail
	public float lowLevelOfDetail = 0.3f;
	public float mediumLevelOfDetail = 0.7f;
	public float highLevelOfDetail = 1.5f;
	public float ultraLevelOfDetail = 2f;
	
	// Anisotropic filtering
	public AnisotropicFiltering lowAnisotropicFiltering = AnisotropicFiltering.Disable;
	public AnisotropicFiltering mediumAnisotropicFiltering = AnisotropicFiltering.Enable;
	public AnisotropicFiltering highAnisotropicFiltering = AnisotropicFiltering.ForceEnable;
	
	// Post effects
	/*
	public MonoBehaviour[] lowQualityPostEffects;
	public MonoBehaviour[] mediumQualityPostEffects;
	public MonoBehaviour[] highQualityPostEffects;
	public MonoBehaviour[] ultraQualityPostEffects;
	*/

	bool lastSync;
	
	Light[] lights;
	string[] selectionGrid = new string[]{"Low", "Medium", "High", "Ultra"};
	string[] selectionGrid2 = new string[]{"Low", "Medium", "High"};
	string[] selectionGrid3 = new string[]{"Off", "Low", "Medium", "High", "Ultra"};
	string[] selectionGrid4 = new string[]{"Low", "Medium", "High", "Ultra", "Custom"};
	
	int overall;
	int particleCustom;
	int textureCustom;
	int shadowCustom;
	int lodCustom;
	int anisotropicFilteringCustom;
	int postEffectsCustom;
	
	bool softParticles;
	
	void ToggleGameObjectArray(GameObject[] array, bool toggle, bool disableRenderer)
	{
		foreach(GameObject go in array)
			
			if (disableRenderer)
#if UNITY_3_5
				go.GetComponent<ParticleSystem>().renderer.enabled = toggle;
#else				
				go.GetComponent<ParticleSystem>().GetComponent<Renderer>().enabled = toggle;
#endif
			else
#if UNITY_3_5
				go.SetActiveRecursively(toggle);
#else
				go.SetActive(toggle);
#endif

	}
	
	void ToggleMonoBehaviourArray(MonoBehaviour[] array, bool toggle)
	{
		foreach(MonoBehaviour b in array)
			b.enabled = toggle;
	}
	
	void DisableShadows()
	{
		foreach(Light l in lights)
			l.shadows = LightShadows.None;
		
		LightmapSettings.lightmapsMode = LightmapsMode.NonDirectional;
	}
	
	void SetShadows(ShadowSettings setting)
	{
		if (LightmapSettings.lightmapsMode == LightmapsMode.NonDirectional)
			LightmapSettings.lightmapsMode = LightmapsMode.CombinedDirectional;
		
		foreach(Light l in lights)
			l.shadows = setting.shadows;
		
		QualitySettings.shadowProjection = setting.shadowProjection;
		QualitySettings.shadowCascades = setting.shadowCascades;
		QualitySettings.shadowDistance = setting.shadowDistance;
	}
	
	void SetQuality(FluvioQualitySetting setting)
	{
		switch(setting)
		{
		case FluvioQualitySetting.Low:
			QualitySettings.SetQualityLevel(0, true);
			ToggleGameObjectArray(ultraQualityParticleSystems, false, disableUltraParticleRenderersOnly);
			ToggleGameObjectArray(highQualityParticleSystems, false, disableHighParticleRenderersOnly);
			ToggleGameObjectArray(mediumQualityParticleSystems, false, disableMediumParticleRenderersOnly);
			
			QualitySettings.masterTextureLimit = mediumQualityTextureSetting;
			
			SetShadows(lowQualityShadowSetting);
			
			QualitySettings.lodBias = lowLevelOfDetail;
			QualitySettings.anisotropicFiltering = lowAnisotropicFiltering;
			
			/*
			ToggleMonoBehaviourArray(ultraQualityPostEffects, false);
			ToggleMonoBehaviourArray(highQualityPostEffects, false);
			ToggleMonoBehaviourArray(mediumQualityPostEffects, false);
			ToggleMonoBehaviourArray(lowQualityPostEffects, true);
			*/

			overall = 0;
			particleCustom = 0;
			textureCustom = 1;
			lodCustom = 0;
			anisotropicFilteringCustom = 0;
			shadowCustom = 1;
			postEffectsCustom = 1;
			
			softParticles = false;
			
			break;
		case FluvioQualitySetting.Medium:
			QualitySettings.SetQualityLevel(1, true);
			ToggleGameObjectArray(ultraQualityParticleSystems, false, disableUltraParticleRenderersOnly);
			ToggleGameObjectArray(highQualityParticleSystems, false, disableHighParticleRenderersOnly);
			ToggleGameObjectArray(mediumQualityParticleSystems, true, disableMediumParticleRenderersOnly);
			
			QualitySettings.masterTextureLimit = mediumQualityTextureSetting;
			
			SetShadows(mediumQualityShadowSetting);
			
			QualitySettings.lodBias = mediumLevelOfDetail;
			QualitySettings.anisotropicFiltering = mediumAnisotropicFiltering;
			
			/*
			ToggleMonoBehaviourArray(ultraQualityPostEffects, false);
			ToggleMonoBehaviourArray(highQualityPostEffects, false);
			ToggleMonoBehaviourArray(mediumQualityPostEffects, true);
			ToggleMonoBehaviourArray(lowQualityPostEffects, true);
			*/

			overall = 1;
			particleCustom = 1;
			textureCustom = 1;
			lodCustom = 1;
			anisotropicFilteringCustom = 1;
			shadowCustom = 2;
			postEffectsCustom = 2;
			softParticles = false;
			
			break;
		case FluvioQualitySetting.High:
			QualitySettings.SetQualityLevel(2, true);
			ToggleGameObjectArray(ultraQualityParticleSystems, false, disableUltraParticleRenderersOnly);
			ToggleGameObjectArray(highQualityParticleSystems, true, disableHighParticleRenderersOnly);
			ToggleGameObjectArray(mediumQualityParticleSystems, true, disableMediumParticleRenderersOnly);		
	
			QualitySettings.masterTextureLimit = highQualityTextureSetting;
			
			SetShadows(highQualityShadowSetting);
			
			QualitySettings.lodBias = highLevelOfDetail;
			QualitySettings.anisotropicFiltering = highAnisotropicFiltering;
			
			/*
			ToggleMonoBehaviourArray(ultraQualityPostEffects, false);
			ToggleMonoBehaviourArray(highQualityPostEffects, true);
			ToggleMonoBehaviourArray(mediumQualityPostEffects, true);
			ToggleMonoBehaviourArray(lowQualityPostEffects, true);
			*/

			overall = 2;
			particleCustom = 2;
			textureCustom = 2;
			lodCustom = 2;
			anisotropicFilteringCustom = 2;
			shadowCustom = 3;
			postEffectsCustom = 3;
			softParticles = false;
			
			break;
		case FluvioQualitySetting.Ultra:
			QualitySettings.SetQualityLevel(5, true);
			ToggleGameObjectArray(ultraQualityParticleSystems, true, disableUltraParticleRenderersOnly);
			ToggleGameObjectArray(highQualityParticleSystems, true, disableHighParticleRenderersOnly);
			ToggleGameObjectArray(mediumQualityParticleSystems, true, disableMediumParticleRenderersOnly);		
			
			QualitySettings.masterTextureLimit = highQualityTextureSetting;
			
			SetShadows(ultraQualityShadowSetting);
			
			QualitySettings.lodBias = ultraLevelOfDetail;
			QualitySettings.anisotropicFiltering = highAnisotropicFiltering;
			
			/*
			ToggleMonoBehaviourArray(ultraQualityPostEffects, true);
			ToggleMonoBehaviourArray(highQualityPostEffects, true);
			ToggleMonoBehaviourArray(mediumQualityPostEffects, true);
			ToggleMonoBehaviourArray(lowQualityPostEffects, true);
			*/

			overall = 3;
			particleCustom = 3;
			textureCustom = 2;
			lodCustom = 3;
			anisotropicFilteringCustom = 2;
			shadowCustom = 4;
			postEffectsCustom = 4;
			softParticles = true;
			
			break;
		}
		
		QualitySettings.vSyncCount = vSync ? 1 : 0;
		
		Save();
	}
	
	void RefreshQuality()
	{
		switch(shadowCustom)
		{
		case 0:
			QualitySettings.SetQualityLevel(softParticles ? 3 : 0, true);
			DisableShadows();
			break;
		case 1:
			QualitySettings.SetQualityLevel(softParticles ? 3 : 0, true);
			SetShadows(lowQualityShadowSetting);
			break;
		case 2:
			QualitySettings.SetQualityLevel(softParticles ? 4 : 1, true);
			SetShadows(mediumQualityShadowSetting);
			break;
		case 3:
			QualitySettings.SetQualityLevel(softParticles ? 5 : 2, true);
			SetShadows(highQualityShadowSetting);
			break;
		case 4:
			QualitySettings.SetQualityLevel(softParticles ? 5 : 2, true);
			SetShadows(ultraQualityShadowSetting);
			break;
		}
	
		switch(particleCustom)
		{
		case 0:
			ToggleGameObjectArray(ultraQualityParticleSystems, false, disableUltraParticleRenderersOnly);
			ToggleGameObjectArray(highQualityParticleSystems, false, disableHighParticleRenderersOnly);
			ToggleGameObjectArray(mediumQualityParticleSystems, false, disableMediumParticleRenderersOnly);
			break;
		case 1:
			ToggleGameObjectArray(ultraQualityParticleSystems, false, disableUltraParticleRenderersOnly);
			ToggleGameObjectArray(highQualityParticleSystems, false, disableHighParticleRenderersOnly);
			ToggleGameObjectArray(mediumQualityParticleSystems, true, disableMediumParticleRenderersOnly);
			break;
		case 2:
			ToggleGameObjectArray(ultraQualityParticleSystems, false, disableUltraParticleRenderersOnly);
			ToggleGameObjectArray(highQualityParticleSystems, true, disableHighParticleRenderersOnly);
			ToggleGameObjectArray(mediumQualityParticleSystems, true, disableMediumParticleRenderersOnly);
			break;
		case 3:
			ToggleGameObjectArray(ultraQualityParticleSystems, true, disableUltraParticleRenderersOnly);
			ToggleGameObjectArray(highQualityParticleSystems, true, disableHighParticleRenderersOnly);
			ToggleGameObjectArray(mediumQualityParticleSystems, true, disableMediumParticleRenderersOnly);
			break;
		}
		
		switch(textureCustom)
		{
		case 0:
			QualitySettings.masterTextureLimit = lowQualityTextureSetting;
			break;
		case 1:
			QualitySettings.masterTextureLimit = mediumQualityTextureSetting;
			break;
		case 2:
			QualitySettings.masterTextureLimit = highQualityTextureSetting;
			break;
		}
		
		switch(lodCustom)
		{
		case 0:
			QualitySettings.lodBias = lowLevelOfDetail;
			break;
		case 1:
			QualitySettings.lodBias = mediumLevelOfDetail;
			break;
		case 2:
			QualitySettings.lodBias = highLevelOfDetail;
			break;
		case 4:
			QualitySettings.lodBias = ultraLevelOfDetail;
			break;
		}
		
		switch(anisotropicFilteringCustom)
		{
		case 0:
			QualitySettings.anisotropicFiltering = lowAnisotropicFiltering;
			break;
		case 1:
			QualitySettings.anisotropicFiltering = mediumAnisotropicFiltering;
			break;
		case 2:
			QualitySettings.anisotropicFiltering = highAnisotropicFiltering;
			break;	
		}
		
		/*
		switch(postEffectsCustom)
		{
		case 0:
			ToggleMonoBehaviourArray(ultraQualityPostEffects, false);
			ToggleMonoBehaviourArray(highQualityPostEffects, false);
			ToggleMonoBehaviourArray(mediumQualityPostEffects, false);
			ToggleMonoBehaviourArray(lowQualityPostEffects, false);
			break;
		case 1:
			ToggleMonoBehaviourArray(ultraQualityPostEffects, false);
			ToggleMonoBehaviourArray(highQualityPostEffects, false);
			ToggleMonoBehaviourArray(mediumQualityPostEffects, false);
			ToggleMonoBehaviourArray(lowQualityPostEffects, true);
			break;
		case 2:
			ToggleMonoBehaviourArray(ultraQualityPostEffects, false);
			ToggleMonoBehaviourArray(highQualityPostEffects, false);
			ToggleMonoBehaviourArray(mediumQualityPostEffects, true);
			ToggleMonoBehaviourArray(lowQualityPostEffects, true);
			break;
		case 3:
			ToggleMonoBehaviourArray(ultraQualityPostEffects, false);
			ToggleMonoBehaviourArray(highQualityPostEffects, true);
			ToggleMonoBehaviourArray(mediumQualityPostEffects, true);
			ToggleMonoBehaviourArray(lowQualityPostEffects, true);
			break;
		case 4:
			ToggleMonoBehaviourArray(ultraQualityPostEffects, true);
			ToggleMonoBehaviourArray(highQualityPostEffects, true);
			ToggleMonoBehaviourArray(mediumQualityPostEffects, true);
			ToggleMonoBehaviourArray(lowQualityPostEffects, true);
			break;	
		}
		*/

		QualitySettings.vSyncCount = vSync ? 1 : 0;
		
		Save();
	}
	
	void Start()
	{	
		Load();
		
		lights = FindObjectsOfType(typeof(Light)) as Light[];
		
		selectionGrid = System.Enum.GetNames(typeof(FluvioQualitySetting));
		
		if (overall < 4)
		{
			SetQuality((FluvioQualitySetting)overall);
		}
		else
		{
			RefreshQuality();
		}
	}
	
	void Save()
	{
		PlayerPrefs.SetInt("FluvioExample.Quality.overall", overall);
		PlayerPrefs.SetInt("FluvioExample.Quality.particleCustom", particleCustom);
		PlayerPrefs.SetInt("FluvioExample.Quality.textureCustom", textureCustom);
		PlayerPrefs.SetInt("FluvioExample.Quality.shadowCustom", shadowCustom);
		PlayerPrefs.SetInt("FluvioExample.Quality.lodCustom", lodCustom);
		PlayerPrefs.SetInt("FluvioExample.Quality.anisotropicFilteringCustom", anisotropicFilteringCustom);
		PlayerPrefs.SetInt("FluvioExample.Quality.postEffectsCustom", postEffectsCustom);
		PlayerPrefs.SetInt("FluvioExample.Quality.softParticles", softParticles ? 1 : 0);
	}
	
	void Load()
	{
		overall = PlayerPrefs.GetInt("FluvioExample.Quality.overall", 1);
		particleCustom = PlayerPrefs.GetInt("FluvioExample.Quality.particleCustom");
		textureCustom = PlayerPrefs.GetInt("FluvioExample.Quality.textureCustom");
		shadowCustom = PlayerPrefs.GetInt("FluvioExample.Quality.shadowCustom");
		lodCustom = PlayerPrefs.GetInt("FluvioExample.Quality.lodCustom");
		anisotropicFilteringCustom = PlayerPrefs.GetInt("FluvioExample.Quality.anisotropicFilteringCustom");
		postEffectsCustom = PlayerPrefs.GetInt("FluvioExample.Quality.postEffectsCustom");
		softParticles = PlayerPrefs.GetInt("FluvioExample.Quality.softParticles") == 1 ? true : false;
	}
	
	void OnGUI()
	{	
		if (lastSync != vSync)
		{
			QualitySettings.vSyncCount = vSync ? 1 : 0;
			lastSync = vSync;
		}
		
		guiRect = new Rect(Screen.width - 410, 10, 400, 230);
		GUILayout.BeginArea(guiRect);
		
		int all = GUILayout.SelectionGrid(overall, selectionGrid4, 5);
		if (all != overall)
		{
			if (all < 4)
			{
				SetQuality((FluvioQualitySetting)all);
			}
			else
			{
				overall = 4;
				Save();
			}
		}
			
		GUILayout.BeginHorizontal();GUILayout.Label("Particles");
		int particles = GUILayout.SelectionGrid(particleCustom, selectionGrid, 5);
		if (particles != particleCustom)
		{
			overall = 4;
			particleCustom = particles;
			RefreshQuality();
		}
		GUILayout.EndHorizontal();
		
		GUILayout.BeginHorizontal();GUILayout.Label("Textures");
		int textures = GUILayout.SelectionGrid(textureCustom, selectionGrid2, 5);
		if (textures != textureCustom)
		{
			overall = 4;
			textureCustom = textures;
			RefreshQuality();
		}
		GUILayout.EndHorizontal();
		
		GUILayout.BeginHorizontal();GUILayout.Label("Level of Detail");
		int lod = GUILayout.SelectionGrid(lodCustom, selectionGrid, 5);
		if (lod != lodCustom)
		{
			overall = 4;
			lodCustom = lod;
			RefreshQuality();
		}
		GUILayout.EndHorizontal();
		
		GUILayout.BeginHorizontal();GUILayout.Label("Anisotropic Filtering");
		int anisotropicFiltering = GUILayout.SelectionGrid(anisotropicFilteringCustom, selectionGrid2, 5);
		if (anisotropicFiltering != anisotropicFilteringCustom)
		{
			overall = 4;
			anisotropicFilteringCustom = anisotropicFiltering;
			RefreshQuality();
		}
		GUILayout.EndHorizontal();
		
		GUILayout.BeginHorizontal();GUILayout.Label("Shadows");
		int shadows = GUILayout.SelectionGrid(shadowCustom, selectionGrid3, 5);
		if (shadows != shadowCustom)
		{
			overall = 4;
			shadowCustom = shadows;
			RefreshQuality();
		}
		GUILayout.EndHorizontal();
		
		/*
		GUILayout.BeginHorizontal();GUILayout.Label("Post Effects");
		int postEffects = GUILayout.SelectionGrid(postEffectsCustom, selectionGrid3, 5);
		if (postEffects != postEffectsCustom)
		{
			overall = 4;
			postEffectsCustom = postEffects;
			RefreshQuality();
		}
		GUILayout.EndHorizontal();
		*/
		
		bool soft = GUILayout.Toggle(softParticles, "Soft Particles", GUILayout.Width(100));
		if (soft != softParticles)
		{
			overall = 4;
			softParticles = soft;
			RefreshQuality();
		}
		
		vSync = GUILayout.Toggle(vSync, "V-Sync", GUILayout.Width(100));
			
		GUILayout.EndArea();
	}
}
			
public enum FluvioQualitySetting
{
	Low = 0,
	Medium,
	High,
	Ultra
}
