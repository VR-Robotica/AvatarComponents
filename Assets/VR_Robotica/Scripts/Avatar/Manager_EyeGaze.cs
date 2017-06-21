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
		public GameObject[] Eyes;
		public float[] XRotationInOut = { -45.0f, 45.0f };
		public float[] YRotationUpDown = { -30.0f, 50.0f };
		public float[] ZRotationMinMax = {  90.0f, 90.0f };

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
					targetPosition = new Vector3(	_focusController.transform.localPosition.x,
													_focusController.transform.localPosition.y,
													_focusController.transform.localPosition.z + 10.0f
												);
				}

				// ...pass to focus controller
				_focusController.moveTo(targetPosition, 10.0f);

				moveEyes();
			}
		}

		private void moveEyes()
		{
			for (int i = 0; i < Eyes.Length; i++)
			{
				Eyes[i].transform.LookAt(_focusController.controller.transform);

				LimitRotations();

				Debug.DrawLine(Eyes[i].transform.position, _focusController.controller.transform.position, Color.red);
			}
		}

		private void LimitRotations()
		{

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
					_currentRotationValue = Eyes[0].transform.localEulerAngles.y;

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
			if (Eyes == null || Eyes.Length == 0)
			{
				Debug.LogWarning("You need to define the Eyes in the Inspector");
				return;
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