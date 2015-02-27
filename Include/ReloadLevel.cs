using UnityEngine;
using System.Collections;

public class ReloadLevel : MonoBehaviour
{
		
	void Update ()
	{
		if (Input.GetKeyDown (KeyCode.R)) {
			Reload ();
		}
	}

	void Reload ()
	{
		Application.LoadLevel (Application.loadedLevel);
	}
}
