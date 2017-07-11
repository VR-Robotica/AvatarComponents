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
	public enum EyeStyle		{ _2D, _3D};
	public enum ImageSource		{ SequenceOfImages, TextureAtlas };
	public enum AtlasAlignment	{ Horizontal, Vertical, Square };

	public class VRR_EyeGaze_Manager : MonoBehaviour
	{
		public bool ShowDebugLog;
		[Tooltip("See The Frustum Position")]
		public bool ShowGizmos;
		public Transform FacingForwardReference;
		public Transform[] Eyes;
		[Tooltip("Adjust when object is reset, without rotations")]
		public Vector3 FrustumPosition;
		[Space]
		public EyeStyle AnimatedEye = EyeStyle._3D;
		[Space]
		[Header("3D Eye Attributes:")]
		[Space]
		public GameObject MeshWithEyeLidShapes;
		public int eyelidTopLeft;
		public int eyelidTopRight;
		public int eyelidBotLeft;
		public int eyelidBotRight;
		[Space]
		[Header("2D Eye Attributes:")]
		[Space]
		public GameObject LeftEyeObject;
		public GameObject RightEyeObject;
		private Material Material_LeftEye;
		private Material Material_RightEye;
		[Header("Blink Animation Properties:")]
		public ImageSource ImageSourceFormat;
		public bool ReverseAnimation;
		[Range(0.01f, 1.0f)]
		public float BetweenFrameRate	= 0.01f;
		[Range(0.01f, 2.0f)]
		public float EyeClosedPause		= 0.1f;
		// if using a Sequence Of Images
		public Texture2D[] BlinkAnimationFrames;
		// if using a Texture Atlas / Sprite Sheet
		public Texture2D BlinkAnimationAtlas;
		// if using a square atlas, define division of rows and columns
		[Range(1, 16)]
		public int SquareAtlasDivision	= 1;
		[Space]
		[Header("Gazing Properties: ")]
		public bool AddEyeJitter;
		[Header("Pivot Angle Rotation Limits")]
		public float EyeRotLimitUp		= 0;
		public float EyeRotLimitDown	= 0;
		public float EyeRotLimitOut		= 0;
		public float EyeRotLimitIn		= 0;
		[Header("Texture Offset Limits")]
		public float LimitUp			= 0;
		public float LimitDown			= 0;
		public float LimitOut			= 0;
		public float LimitIn			= 0;
		[Space]

		// Controls the blink animations
		private VRR_EyeBlink_Controller _eyeBlink;
		// Controls the interest lists of the avatar's potential gaze
		private VRR_Interest_Controller _interestController;
		// the object the avatar's eyes are fixated on
		private VRR_Focus_Controller	_focusControllerScript;
		private Transform				_focusControlTransform;


		private float _offsetLeftXValue		= 0;
		private float _offsetLeftYValue		= 0;
		private float _offsetRightXValue	= 0;
		private float _offsetRightYValue	= 0;


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

				// float	mag		= Vector3.Magnitude(origin + FrustumPosition);
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

			if(AnimatedEye == EyeStyle._2D)
			{
				rotationAngleToOffset();
				update_EyeTextureOffset();
			}
		}

		private void update_EyeTextureOffset()
		{
			// add some random eye jitter (aka a poor implementation of microsaccadic motion)
			float eyeJitter_X = 0;
			float eyeJitter_Y = 0;

			if (AddEyeJitter)
			{
				eyeJitter_X = Mathf.Lerp(0, Random.Range(0.001f, 0.1f), 0.05f);
				eyeJitter_Y = Mathf.Lerp(0, Random.Range(0.001f, 0.1f), 0.05f);
			}

			Material_LeftEye.SetTextureOffset("_BotTex", new Vector2(_offsetLeftXValue + eyeJitter_X, _offsetLeftYValue + eyeJitter_Y));
			Material_RightEye.SetTextureOffset("_BotTex", new Vector2(_offsetRightXValue + eyeJitter_X, _offsetRightYValue + eyeJitter_Y));

			Material_LeftEye.SetTextureOffset("_IrisMask", new Vector2(_offsetLeftXValue + eyeJitter_X, _offsetLeftYValue + eyeJitter_Y));
			Material_RightEye.SetTextureOffset("_IrisMask", new Vector2(_offsetRightXValue + eyeJitter_X, _offsetRightYValue + eyeJitter_Y));
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
			get_AvatarEyes();

			create_BlinkController();
			// wait for the eye blink controller to finish
			yield return _eyeBlink.Create();

			create_FocusController();
			// wait for the focus controller to finish
			yield return _focusControllerScript.Create();
			// get reference to focus control object
			_focusControlTransform = _focusControllerScript.controller.transform;

			create_InterestController();
			// set variable(s)
			_interestController.FrustumOffSet = FrustumPosition;
			// wait for interest controller to finish
			yield return _interestController.Create();

			_isReady = true;
		}

		private void get_AvatarEyes()
		{
			if (Eyes == null || Eyes.Length == 0)
			{
				Debug.LogWarning("You need to define the Eyes in the Inspector");
				return;
			}

			if (AnimatedEye == EyeStyle._2D)
			{
				Material_LeftEye = LeftEyeObject.GetComponent<Renderer>().material;
				Material_RightEye = RightEyeObject.GetComponent<Renderer>().material;

				// Check for proper Shader...
				if (Material_LeftEye.shader.name == "VR Robotica/Unlit_Eye" && Material_RightEye.shader.name == "VR Robotica/Unlit_Eye")
				{
					if (ShowDebugLog) { Debug.Log("Correct Shaders FOUND!"); }
				}
				else
				{
					Debug.LogWarning("For best results, please use VR Robotica's 'Unlit_Eye' ");
					return;
				}
			}
		}

		private void create_BlinkController()
		{
			_eyeBlink = this.gameObject.AddComponent<VRR_EyeBlink_Controller>();
			_eyeBlink.AnimatedEye = AnimatedEye;

			if (AnimatedEye == EyeStyle._3D)
			{
				_eyeBlink.MeshWithEyeLidShapes = MeshWithEyeLidShapes;
				_eyeBlink.eyelidTopLeft = eyelidTopLeft;
				_eyeBlink.eyelidBotLeft = eyelidBotLeft;
				_eyeBlink.eyelidTopRight = eyelidTopRight;
				_eyeBlink.eyelidBotRight = eyelidBotRight;
			}
			else
			if(AnimatedEye == EyeStyle._2D)
			{
				// passing values over to BlinkController (a little redundant right now)
				_eyeBlink.Material_LeftEye = Material_LeftEye;
				_eyeBlink.Material_RightEye = Material_RightEye;
				_eyeBlink.ImageSourceFormat = ImageSourceFormat;
				_eyeBlink.ReverseAnimation = ReverseAnimation;
				_eyeBlink.BetweenFrameRate = BetweenFrameRate;
				_eyeBlink.EyeClosedPause = EyeClosedPause;
				_eyeBlink.BlinkAnimationFrames = BlinkAnimationFrames;
				_eyeBlink.BlinkAnimationAtlas = BlinkAnimationAtlas;
				_eyeBlink.SquareAtlasDivision = SquareAtlasDivision;
			}
		}

		private void create_FocusController()
		{
			_focusControllerScript = this.gameObject.AddComponent<VRR_Focus_Controller>();
		}

		private void create_InterestController()
		{
			_interestController = this.gameObject.AddComponent<VRR_Interest_Controller>();
		}

		#endregion

		private void rotationAngleToOffset()
		{
			// convert the rotation angle of the Eye Pivots and 
			// translate that value to a texture offset value 
			// using a simple percentage value conversion
			if (ShowDebugLog) { Debug.Log("Converting Rotation Angle to Offset Value"); }

			float Percent_LeftLookingOut, Percent_LeftLookingIn, Percent_LeftLookingUp, Percent_LeftLookingDown;
			float Percent_RightLookingOut, Percent_RightLookingIn, Percent_RightLookingUp, Percent_RightLookingDown;

			// get current rotation values, then divide by max limit values to get relative percentages
			// Left Eye Rotations - Eyes[0]
			Percent_LeftLookingOut = ((360 - Eyes[0].transform.localEulerAngles.x) / EyeRotLimitOut);
			Percent_LeftLookingIn = (Eyes[0].transform.localEulerAngles.x / EyeRotLimitIn);
			Percent_LeftLookingUp = ((360 - Eyes[0].transform.localEulerAngles.y) / EyeRotLimitUp);
			Percent_LeftLookingDown = (Eyes[0].transform.localEulerAngles.y / EyeRotLimitDown);
			// Right Eye Rotations - Eyes[1]
			Percent_RightLookingOut = (Eyes[1].transform.localEulerAngles.x / EyeRotLimitOut);
			Percent_RightLookingIn = ((360 - Eyes[1].transform.localEulerAngles.x) / EyeRotLimitIn);
			Percent_RightLookingUp = ((360 - Eyes[1].transform.localEulerAngles.y) / EyeRotLimitUp);
			Percent_RightLookingDown = (Eyes[1].transform.localEulerAngles.y / EyeRotLimitDown);

			if (ShowDebugLog) { Debug.Log("Left X Rot: " + Mathf.Round(Eyes[0].transform.localEulerAngles.x) + ", Y Rot: " + Mathf.Round(Eyes[0].transform.localEulerAngles.y)); }

			// Euler Angles use a 0-360 value set, so negative angle values (-x) are converted to (360 - x)
			// HORIZONTAL
			// LEFT EYE
			if (Eyes[0].transform.localEulerAngles.x <= 360 && Eyes[0].transform.localEulerAngles.x >= 360 - EyeRotLimitOut)
			{
				if (ShowDebugLog) { Debug.Log("Left Out Percentage: " + Percent_LeftLookingOut); }

				_offsetLeftXValue = (LimitOut * Percent_LeftLookingOut);
			}
			else
			if (Eyes[0].transform.localEulerAngles.x >= 0 && Eyes[0].transform.localEulerAngles.x <= EyeRotLimitIn)
			{
				if (ShowDebugLog) { Debug.Log("Left In Percentage: " + Percent_LeftLookingIn); }

				_offsetLeftXValue = -1 * (LimitIn * Percent_LeftLookingIn);
			}

			// RIGHT EYE
			if (Eyes[1].transform.localEulerAngles.x <= 360 && Eyes[1].transform.localEulerAngles.x >= 360 - EyeRotLimitIn)
			{
				if (ShowDebugLog) { Debug.Log("Right In Percentage: " + Percent_RightLookingIn); }

				_offsetRightXValue = -1 * (LimitIn * Percent_RightLookingIn);
			}
			else
			if (Eyes[1].transform.localEulerAngles.x >= 0 && Eyes[1].transform.localEulerAngles.x <= EyeRotLimitOut)
			{
				if (ShowDebugLog) { Debug.Log("Right Out Percentage: " + Percent_RightLookingOut); }

				_offsetRightXValue = (LimitOut * Percent_RightLookingOut);
			}

			// VERTICAL
			// LEFT EYE
			if (Eyes[0].transform.localEulerAngles.y >= 0 && Eyes[0].transform.localEulerAngles.y <= EyeRotLimitDown)
			{
				_offsetLeftYValue = (LimitDown * Percent_LeftLookingDown);
			}
			else
			if (Eyes[0].transform.localEulerAngles.y <= 360 && Eyes[0].transform.localEulerAngles.y >= 360 - EyeRotLimitUp)
			{
				_offsetLeftYValue = -1 * (LimitUp * Percent_LeftLookingUp);
			}

			// RIGHT EYE
			if (Eyes[1].transform.localEulerAngles.y >= 0 && Eyes[1].transform.localEulerAngles.y <= EyeRotLimitDown)
			{
				_offsetRightYValue = (LimitDown * Percent_RightLookingDown);
			}
			else
			if (Eyes[1].transform.localEulerAngles.y <= 360 && Eyes[1].transform.localEulerAngles.y >= 360 - EyeRotLimitUp)
			{
				_offsetRightYValue = -1 * (LimitUp * Percent_RightLookingUp);
			}
		}
	}
}