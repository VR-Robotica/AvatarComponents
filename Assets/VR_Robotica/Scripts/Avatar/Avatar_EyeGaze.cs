using com.VR_Robotica;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// WIP - WORK IN PROGRESS v0.1
/// 
/// </summary>
namespace com.VR_Robotica.Avatars
{
	public class Avatar_EyeGaze : MonoBehaviour
	{
		public Avatar_FocusController ObjectOfFocus;
		public ArrayList focusTargets;

		// Use this for initialization
		void Start()
		{
			createObjectOfFocus();

			Start_Focus();
		}

		// Update is called once per frame
		void Update()
		{

		}

		public void Start_Focus()
		{
			if(focusTargets != null && focusTargets.Count > 0)
			{
				ObjectOfFocus.moveTo( (Transform)focusTargets[0], 10.0f );
			}
		}

		private void createObjectOfFocus()
		{
			// if a gameObject has not been created/defined in the Editor...
			if (ObjectOfFocus == null)
			{
				// create a new object
				ObjectOfFocus = new Avatar_FocusController();
				// parent object to this avatar
				ObjectOfFocus.FocusController.transform.parent = this.transform;
			}
		}
	}
}