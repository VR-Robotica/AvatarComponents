using System.Collections;
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
		[Tooltip("See The Frustum Position")]
		public bool ShowGizmos;
		public GameObject FacingForwardReference;
		public Transform[] Eyes;
		[Tooltip("Adjust when object is reset, without rotations")]
		public Vector3 FrustumPosition;

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
				update_DrawDebugRays();
			}
		}

		private void LateUpdate()
		{
			if (_isReady)
			{
				update_EyeRotations();
			}
		}

		private void OnDrawGizmosSelected()
		{
			#region CLAMP
			FrustumPosition = new Vector3(
				Mathf.Clamp(FrustumPosition.x, 0, 2),
				Mathf.Clamp(FrustumPosition.y, 0, 2),
				Mathf.Clamp(FrustumPosition.z, 0, 2)
				);
			#endregion

			if (ShowGizmos)
			{
				Vector3 origin = FacingForwardReference.transform.position;
				Vector3 xDirection = FacingForwardReference.transform.right;
				Vector3 yDirection = FacingForwardReference.transform.up;
				Vector3 zDirection = FacingForwardReference.transform.forward;

				Ray xRay = new Ray(origin, xDirection);
				Ray yRay = new Ray(origin, yDirection);
				Ray zRay = new Ray(origin, zDirection);
				
				Vector3 xPos = xRay.GetPoint(FrustumPosition.x);
				Vector3 yPos = yRay.GetPoint(FrustumPosition.y);
				Vector3 zPos = zRay.GetPoint(FrustumPosition.z);
				
				Vector3 vec1 = yPos + zPos - origin;
				Vector3 vec2 = xPos + yPos - origin;
				Vector3 vec3 = zPos + xPos - origin;

				xRay = new Ray(vec3, yDirection);
				Vector3 newxPos = xRay.GetPoint(FrustumPosition.y);
				yRay = new Ray(vec2, zDirection);
				Vector3 newYPos = yRay.GetPoint(FrustumPosition.z);
				zRay = new Ray(vec1, xDirection);
				Vector3 newZPos = zRay.GetPoint(FrustumPosition.x);

				float	mag		= Vector3.Magnitude(origin + FrustumPosition);
				Vector3 newPos  = new Vector3(newxPos.x, newYPos.y, newZPos.z); 
				// newRay.GetPoint(mag);

				

				#region DEBUG LINES
				/*
				Vector3 gizmoMini = new Vector3(0.01f, 0.01f, 0.01f);
				Vector3 gizmoSize = new Vector3(0.025f, 0.025f, 0.025f);

				Gizmos.color = Color.cyan;
				// directions
				Gizmos.DrawRay(origin, xDirection);
				Gizmos.DrawRay(origin, yDirection);
				Gizmos.DrawRay(origin, zDirection);

				// forward - Z Axis
				Gizmos.color = Color.blue;
				Gizmos.DrawCube(vec1, gizmoMini);
				Gizmos.DrawRay(vec1, xDirection);
				// Up - Y Axis
				Gizmos.color = Color.red;
				Gizmos.DrawCube(vec2, gizmoMini);
				Gizmos.DrawRay(vec2, zDirection);
				// Right - X Axis
				Gizmos.color = Color.green;
				Gizmos.DrawCube(vec3, gizmoMini);
				Gizmos.DrawRay(vec3, yDirection);
				*/

				Vector3 gizmoposition = newPos;

				Gizmos.color = Color.white;
				Gizmos.DrawRay(origin, gizmoposition - origin);
				Gizmos.DrawSphere(gizmoposition, 0.01f);

				#endregion
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
				targetPosition = FacingForwardReference.transform.TransformPoint(new Vector3(0, 0, 10));
				_focusControllerScript.moveTo(targetPosition, 5.0f);
			}
		}

		private void update_EyeRotations()
		{
			for (int i = 0; i < Eyes.Length; i++)
			{
				Eyes[i].LookAt(_focusControlTransform);
			}
		}

		private void update_DrawDebugRays()
		{
			for (int i = 0; i < Eyes.Length; i++)
			{
				Debug.DrawRay(FacingForwardReference.transform.position,
							FacingForwardReference.transform.forward,
							Color.blue
							);
				Debug.DrawLine(Eyes[i].position,
							_focusControlTransform.position,
							Color.red
							);

			//	Debug.Log("Ref: " + FacingForwardReference.transform.position + " Eye: " + Eyes[0].position);
			}
		}

		// if rotation of eyes changes enough, trigger eye blink
		private float _currentRotationValue;
		private float _previousRotationValue;
		private IEnumerator triggerBlink()
		{
			if (_eyeBlink != null)
			{
				// coroutine runs a continuous check...
				while (true)
				{
					_currentRotationValue = Eyes[0].localEulerAngles.x;
					// checking if the eye rotates a significant amount
					if (Mathf.Abs(_currentRotationValue - _previousRotationValue) > 20.0f)
					{
						// then trigger blink
						yield return _eyeBlink.SingleBlink();
						// delay the next trigger attempt
						yield return new WaitForSeconds(2.0f);
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
			// wait for the focus controller to finish
			yield return _focusControllerScript.Create();
			// get reference to focus control object
			_focusControlTransform = _focusControllerScript.controller.transform;

			createInterestController();
			// set variable(s)
			_interestController.FrustumOffSet = FrustumPosition;
			// wait for intialization
			yield return _interestController.Create();

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