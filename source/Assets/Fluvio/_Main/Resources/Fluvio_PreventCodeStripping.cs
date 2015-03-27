using UnityEngine;
using System.Collections;

namespace ThinksquirrelSoftware.Fluvio.Internal
{
	internal class Fluvio_PreventCodeStripping
	{
		/// <summary>
		/// A workaround to prevent bytecode stripping of certain types that are needed by Fluvio's assemblies.
		/// </summary>
		void PreventCodeStripping()
		{
#if UNITY_IPHONE
			Debug.Log(iPhone.generation);
#endif
		}
	}
}