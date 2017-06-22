using com.VR_Robotica;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// WIP - WORK IN PROGRESS v0.1
/// This Manages all the basic elements for simualting the Eye Gaze for your Avatar or NPC.
/// It will create all the controllers and their corresponding control objects automatically,
/// and will gaze at objects that have the Object_OfInterest Component.
/// </summary>
namespace com.VR_Robotica.Avatars
{
	public class Manager_EyeGaze : MonoBehaviour
	{
		public Transform FacingForwardReference;
		public Transform[] Eyes;

		// Controls the interest lists of the avatar's potential gaze
		private Controller_Interest _interestController;

		// the object the avatar's eyes are fixated on
		private Controller_Focus	_focusControllerScript;
		private Transform			_focusControlTransform;

		private Controller_EyeBlink _eyeBlink;

		private bool _isReady;

		private void Awake()
		{
			if(FacingForwardReference == null)
			{
				Debug.LogError("Forward Reference Point needs to be set");
				return;
			}
		}

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
				update_TargetPosition();
			//	update_EyeRotations();
			}
		}

		private void LateUpdate()
		{
			if (_isReady)
			{
				update_EyeRotations();
			}
		}

		private void update_TargetPosition()
		{
			Vector3 targetPosition;
			// taking input from interest controller... 
			// check if there is a current target from Control_Interest Script
			if (_interestController.CurrentlyLookingAt != null)
			{
				targetPosition = _interestController.CurrentlyLookingAt.transform.position;
				// ...pass the target position to Control_Focus script
				_focusControllerScript.moveTo(targetPosition, Random.Range(10.0f, 20.0f));
			}
			else
			{
				// use a default position
				targetPosition = FacingForwardReference.TransformPoint(new Vector3(0, 0, 10));
				_focusControllerScript.moveTo(targetPosition, 5.0f);

				//_focusControllerScript.GotoDefaultPosition();
			}
		}

		private void update_EyeRotations()
		{
			for (int i = 0; i < Eyes.Length; i++)
			{
				Eyes[i].LookAt(_focusControlTransform);

				Debug.DrawRay(FacingForwardReference.position, FacingForwardReference.forward, Color.blue);
				Debug.DrawLine(Eyes[i].position, _focusControlTransform.position, Color.red);
			}
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
					_currentRotationValue = Eyes[0].localEulerAngles.x;

					if (Mathf.Abs(_currentRotationValue - _previousRotationValue) > 50.0f)
					{
						yield return _eyeBlink.SingleBlink();
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
			yield return _focusControllerScript.Create();
			// get reference to focus control object
			_focusControlTransform = _focusControllerScript.controller.transform;

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
			_focusControllerScript = this.gameObject.AddComponent<Controller_Focus>();
		}

		private void createInterestController()
		{
			_interestController = this.gameObject.AddComponent<Controller_Interest>();
		}

		#endregion
	}
}