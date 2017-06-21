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
		public Transform ForwardReferencePoint;
		public GameObject[] Eyes;
		public float[] XRotationInOut = { -45.0f, 45.0f };
		public float[] YRotationUpDown = { -30.0f, 50.0f };
		public float[] ZRotationMinMax = {  90.0f, 90.0f };

		// Controls the interest lists of the avatar's potential gaze
		private Controller_Interest _interestController;

		// the object the avatar's eyes are fixated on
		private Controller_Focus	_focusControllerScript;
		private Transform			_focusControlTransform;

		private Controller_EyeBlink _eyeBlink;

		private bool _isReady;

		private void Awake()
		{
			if(ForwardReferencePoint == null)
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
			}
			else
			{
				// if there is NO target, gaze off into the distance...
				targetPosition = ForwardReferencePoint.forward;
			}

			// ...pass the target position to Control_Focus script
			_focusControllerScript.moveTo(targetPosition, 5.0f);
		}

		private void update_EyeRotations()
		{
			for (int i = 0; i < Eyes.Length; i++)
			{
				Eyes[i].transform.LookAt(_focusControlTransform);

				LimitRotations();
				Debug.DrawRay(ForwardReferencePoint.position, ForwardReferencePoint.forward, Color.blue);
				Debug.DrawLine(Eyes[i].transform.position, _focusControlTransform.position, Color.red);
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
					_currentRotationValue = Eyes[0].transform.localEulerAngles.x;

					if (Mathf.Abs(_currentRotationValue - _previousRotationValue) > 5.0f)
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