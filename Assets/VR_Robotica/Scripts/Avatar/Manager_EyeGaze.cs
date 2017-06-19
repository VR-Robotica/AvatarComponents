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

		// Controls the interest lists of the avatar's potential gaze
		private Controller_Interest _interestController;

		// the object the avatar's eyes are fixated on
		private Controller_Focus _focusController;

		public bool _isReady;

		// Use this for initialization
		void Start()
		{
			StartCoroutine(setup());
		}

		// Update is called once per frame
		void Update()
		{
			if(_isReady)
			{
				Vector3 targetPosition;
				// taking input from interest controller... 
				if (_interestController.CurrentlyLookingAtThis != null)
				{
					targetPosition = _interestController.CurrentlyLookingAtThis.transform.position;
				}
				else
				{
					// gaze off in the distance
					targetPosition = new Vector3(0, 0, 10);
				}

				// ...pass to focus controller
				_focusController.moveTo(targetPosition, 10.0f);

				moveEyes();
			}
		}

		private void moveEyes()
		{
			// eyes follow the control object
			LeftEye.transform.LookAt(_focusController.controller.transform);
			RightEye.transform.LookAt(_focusController.controller.transform);

			// Draw lines from eyes to target
			Debug.DrawLine(LeftEye.transform.position, _focusController.controller.transform.position, Color.red);
			Debug.DrawLine(RightEye.transform.position, _focusController.controller.transform.position, Color.red);
		}

		#region SETUP
		private IEnumerator setup()
		{
			getAvatarEyes();

			createFocusController();
			createInterestController();

			// wait for the focus controller to finish
			yield return _focusController.Create();
			
			_isReady = true;
		}

		private void getAvatarEyes()
		{
			if (LeftEye == null || RightEye == null)
			{
				Debug.LogWarning("You need to define the Eyes in the Inspector");
			}
		}

		private void createFocusController()
		{
			_focusController = this.gameObject.AddComponent<Controller_Focus>();
		}

		private void createInterestController()
		{
			_interestController = this.gameObject.AddComponent<Controller_Interest>();
		}

		#endregion
	}
}