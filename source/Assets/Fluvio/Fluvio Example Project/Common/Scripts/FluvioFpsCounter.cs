// 
// FluvioFpsCounter.cs
//  
// Authors:
//       Josh Montoute <josh@thinksquirrel.com>
//		 Aras Pranckevicius (NeARAZ), Opless <http://www.unifycommunity.com/wiki/index.php?title=FramesPerSecond>
// 
// Copyright (c) 2011-2013 Thinksquirrel Software, LLC
//
//
using UnityEngine;
using System.Collections;

[AddComponentMenu("Fluvio Example Project/FPS Counter")]
public class FluvioFpsCounter : MonoBehaviour 
{

// Attach this to a GUIText to make a frames/second indicator.
//
// It calculates frames/second over each updateInterval,
// so the display does not keep changing wildly.
//
// It is also fairly accurate at very low FPS counts (<10).
// We do this not by simply counting frames per interval, but
// by accumulating FPS for each frame. This way we end up with
// correct overall FPS even if the interval renders something like
// 5.5 frames.
 
public  float updateInterval = 0.5F;
private float accum   = 0; // FPS accumulated over the interval
private int   frames  = 0; // Frames drawn over the interval
private float timeleft; // Left time for current interval

int collectionCount;
	
void Start()
{
    if( !guiText )
    {
        Debug.Log("UtilityFramesPerSecond needs a GUIText component!");
        enabled = false;
        return;
    }
    timeleft = updateInterval;
		
//	collectionCount = System.GC.CollectionCount(0);
		
}
 
void Update()
{
    timeleft -= Time.deltaTime;
    accum += Time.timeScale/Time.deltaTime;
    ++frames;
    
    // Interval ended - update GUI text and start new interval
    if( timeleft <= 0.0 )
    {
        // display two fractional digits (f2 format)
    float fps = accum/frames;
    string format = System.String.Format("{0:F2} FPS"/*\nGC Collection Count: {1}"*/,fps/*, System.GC.CollectionCount(0) - collectionCount*/);
    guiText.text = format;

    if(fps < 30)
        guiText.material.color = Color.yellow;
    else 
        if(fps < 10)
            guiText.material.color = Color.red;
        else
            guiText.material.color = Color.green;
        timeleft = updateInterval;
        accum = 0.0F;
        frames = 0;
    }
}
}