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
		public GameObject LeftEyePivot;
		public GameObject RightEyePivot;
		[HideInInspector]
		public Vector3 CenterOfEyes;

		// Controls the interest lists of the avatar's potential gaze
		private Controller_Interest _interestController;

		// the object the avatar's eyes are fixated on
		private Controller_Focus _focusController;

		private Controller_EyeBlink _eyeBlink;

		private bool _isReady;

		// Use this for initialization
		void Start()
		{
			StartCoroutine(setup());
			StartCoroutine(triggerBlink());
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
			LeftEyePivot.transform.LookAt(_focusController.controller.transform);
			RightEyePivot.transform.LookAt(_focusController.controller.transform);

			// Draw lines from eyes to target
			Debug.DrawLine(LeftEyePivot.transform.position, _focusController.controller.transform.position, Color.red);
			Debug.DrawLine(RightEyePivot.transform.position, _focusController.controller.transform.position, Color.red);
		}

		// if rotation of eyes changes enough, trigger eye blink
		private float _currentRotationValue;
		private float _previousRotationValue;
		private IEnumerator triggerBlink()
		{
			if (_eyeBlink != null)
			{
				while (true)
				{
					_currentRotationValue = LeftEyePivot.transform.localEulerAngles.y;

					if (Mathf.Abs(_currentRotationValue - _previousRotationValue) > 0.5f)
					{
						StartCoroutine(_eyeBlink.SingleBlink());
					}

					yield return new WaitForEndOfFrame();

					_previousRotationValue = _currentRotationValue;
				}
			}

			yield return null;
		}

		#region SETUP
		private IEnumerator setup()
		{
			getAvatarEyes();
			_eyeBlink = this.gameObject.GetComponent<Controller_EyeBlink>();
			createFocusController();
			createInterestController();

			// wait for the focus controller to finish
			yield return _focusController.Create();
			
			_isReady = true;
		}

		private void getAvatarEyes()
		{
			if (LeftEyePivot == null || RightEyePivot == null)
			{
				Debug.LogWarning("You need to define the Eyes in the Inspector");
				return;
			}

			CenterOfEyes = LeftEyePivot.transform.position + (RightEyePivot.transform.position - LeftEyePivot.transform.position) / 2;
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