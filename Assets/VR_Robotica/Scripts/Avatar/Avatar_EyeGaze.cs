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
		public GameObject LeftEye;
		public GameObject RightEye;
		[Space]
		public ArrayList focusTargets;

		private Avatar_FocusController _focusController;
		private Avatar_FocusOfInterest _focusOfInterest;

		// Use this for initialization
		void Start()
		{
			getAvatarEyes();
			createFocusController();

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
				_focusController.moveTo( (Vector3)focusTargets[0], 10.0f );
			}
		}

		private void getAvatarEyes()
		{
			if(LeftEye == null)
			{
				LeftEye = this.gameObject.transform.Find("LeftEye").gameObject;
				if(LeftEye == null)
				{
					LeftEye = this.gameObject.transform.Find("eye_l").gameObject;
				}

				if (LeftEye == null)
				{
					Debug.LogWarning("Left Eye NOT Found!");
				}

			}

			if (RightEye == null)
			{
				RightEye = this.gameObject.transform.Find("RightEye").gameObject;
				if (LeftEye == null)
				{
					RightEye = this.gameObject.transform.Find("eye_r").gameObject;
				}

				if(RightEye == null)
				{
					Debug.LogWarning("Right Eye NOT Found!");
				}
			}
		}

		private void createFocusController()
		{
			_focusController = this.gameObject.AddComponent<Avatar_FocusController>();
			_focusController.Controller.transform.parent = this.transform;
		}

		private void createFocusOfInterest()
		{
			_focusOfInterest = this.gameObject.AddComponent<Avatar_FocusOfInterest>();
		}
	}
}