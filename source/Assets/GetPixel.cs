using UnityEngine;
using System.Collections;
using System;

public class GetPixel : MonoBehaviour {

	// Use this for initialization
	public Camera myCam;
	public Rect r;
	private Texture2D tex;
	public String pix;

	void Start () {
		r = myCam.pixelRect;
		tex = new Texture2D ((int)r.width, (int)r.height, TextureFormat.RGB24, false);
	}
	
	public Color32 GetVisionPixel(double x, double y) {
		//x, y are decimal values from [0, 1]
		//Bottom left corner of camera is (0, 0)
		if (x > 1 || x < 0 || y > 1 || y < 0) {
			Debug.Log ("Invalid Pixel Position");
			return Color.white;
		}
		tex.ReadPixels (r, 0, 0);
		tex.Apply ();
		//var bytes = tex.EncodeToPNG();
		//File.WriteAllBytes("C:/Users/TurdBurbler/Desktop/testing.png", bytes);
		return tex.GetPixel ((int)(x * r.width), (int)(y * r.height));
		//Debug.Log (r.xMin);
		//Debug.Log(r.xMax);
		//Debug.Log (r.yMin);
		
	}
	// Update is called once per frame
	void Update () {
	
	}
	void OnPostRender() {
		pix = GetVisionPixel (0, 0).ToString ();
		//Debug.Log (pix);
	}
}
