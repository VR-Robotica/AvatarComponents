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
	public class Manager_EyeGaze : MonoBehaviour
	{
		public GameObject LeftEye;
		public GameObject RightEye;
		[Space]
		public ArrayList focusTargets;

		private Controller_Focus	_focusController;
		private Controller_Interest _interestController;

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
			if(LeftEye == null || RightEye == null)
			{
				Debug.LogWarning("You need to define the Eyes in the Inspector");
			}
		}

		private void createFocusController()
		{
			_focusController = this.gameObject.AddComponent<Controller_Focus>();
		//	_focusController.Controller.transform.parent = this.transform;
		}

		private void createInterestController()
		{
			_interestController = this.gameObject.AddComponent<Controller_Interest>();
		}
	}
}