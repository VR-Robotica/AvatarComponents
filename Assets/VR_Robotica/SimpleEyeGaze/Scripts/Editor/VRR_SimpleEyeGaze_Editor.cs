using UnityEditor;
using UnityEngine;

namespace com.VR_Robotica.AvatarComponents
{
	[CustomEditor(typeof(VRR_SimpleEyeGaze))]
	public class VRR_SimpleEyeGaze_Editor : Editor
	{
		private float				_versionNumber = 1.0f;
		private GUISkin				_customSkin;
		private Texture2D			_logo;
		private SerializedObject	_serializedTarget;
		private VRR_SimpleEyeGaze	_eyeGazeManager;
		// GUI Hack
		private string separator = "_______________________________________________________________________________________";

//		private Transform[] Eyes = new Transform[2];

		private string[] blendshapes;

		private void OnSceneGUI()
		{
			
		}

		private void OnEnable()
		{
			_serializedTarget = new SerializedObject(target);
			_logo = (Texture2D)EditorGUIUtility.Load("VR_Robotica/LOGO.png");
		}

		public override void OnInspectorGUI()
		{
			if (_serializedTarget == null)
			{
				OnEnable();
			}

			_serializedTarget.Update();
			// Script Refference
			_eyeGazeManager = (VRR_SimpleEyeGaze)target;

			createLogo();
			createTitle();

			// set DEFAULT ARRAY SIZES...
			if (_eyeGazeManager.Eyes == null)
			{
				_eyeGazeManager.Eyes = new Transform[2];
			}
			if (_eyeGazeManager.BlinkAnimationFrames == null || _eyeGazeManager.BlinkAnimationFrames.Length != 4)
			{
				_eyeGazeManager.BlinkAnimationFrames = new Texture2D[4];
			}



			// CREATE UI MENU HERE
			EditorGUI.BeginChangeCheck();
			
			EditorGUILayout.LabelField(separator);
			EditorGUILayout.Space();
			
			_eyeGazeManager.FacingForwardReference = EditorGUILayout.ObjectField("Head Pivot:", _eyeGazeManager.FacingForwardReference, typeof(Transform), true) as Transform;

			if (_eyeGazeManager.FacingForwardReference != null)
			{
				if (_eyeGazeManager.Eyes != null)
				{
					_eyeGazeManager.Eyes[0] = EditorGUILayout.ObjectField("Left Eye Pivot:", _eyeGazeManager.Eyes[0], typeof(Transform), true) as Transform;
					_eyeGazeManager.Eyes[1] = EditorGUILayout.ObjectField("Right Eye Pivot:", _eyeGazeManager.Eyes[1], typeof(Transform), true) as Transform;
				}
			}

			if (_eyeGazeManager.Eyes != null && _eyeGazeManager.Eyes[0] != null && _eyeGazeManager.Eyes[1] != null)
			{
				EditorGUILayout.Space();
				_eyeGazeManager.ShowGizmos = EditorGUILayout.Toggle("Show Frustum Placement:", _eyeGazeManager.ShowGizmos);
				_eyeGazeManager.FrustumPosition = EditorGUILayout.Vector3Field("Frustum Position:", _eyeGazeManager.FrustumPosition);
			}

			EditorGUILayout.LabelField(separator);
			EditorGUILayout.Space();
			_eyeGazeManager.AnimatedEye = (EyeStyle)EditorGUILayout.EnumPopup("Are We Animating 3D or 2D Eyes?", _eyeGazeManager.AnimatedEye);
			EditorGUILayout.Space();
			EditorGUILayout.LabelField(separator);


			switch (_eyeGazeManager.AnimatedEye)
			{
				case EyeStyle._2D:
					EditorGUILayout.LabelField("Animated 2D Eyes");

					_eyeGazeManager.LeftEyeObject = EditorGUILayout.ObjectField("Left Eye Object:", _eyeGazeManager.LeftEyeObject, typeof(GameObject), true) as GameObject;
					_eyeGazeManager.RightEyeObject = EditorGUILayout.ObjectField("Right Eye Object:", _eyeGazeManager.RightEyeObject, typeof(GameObject), true) as GameObject;
					EditorGUILayout.Space();
					if (_eyeGazeManager.LeftEyeObject != null && _eyeGazeManager.RightEyeObject != null)
					{
						EditorGUILayout.LabelField("Blink Animation Properties");
						EditorGUILayout.Space();

						_eyeGazeManager.ImageSourceFormat = (ImageSource)EditorGUILayout.EnumPopup("Eye Animation Source Format:", _eyeGazeManager.ImageSourceFormat);

						switch(_eyeGazeManager.ImageSourceFormat)
						{
							case ImageSource.SequenceOfImages:
								EditorGUILayout.Space();
								EditorGUILayout.LabelField("4 Image Sequence:");

								_eyeGazeManager.BlinkAnimationFrames[0] = EditorGUILayout.ObjectField("01 - Open  :", _eyeGazeManager.BlinkAnimationFrames[0], typeof(Texture2D), true) as Texture2D;
								_eyeGazeManager.BlinkAnimationFrames[1] = EditorGUILayout.ObjectField("02   90%   :", _eyeGazeManager.BlinkAnimationFrames[1], typeof(Texture2D), true) as Texture2D;
								_eyeGazeManager.BlinkAnimationFrames[2] = EditorGUILayout.ObjectField("03   10%   :", _eyeGazeManager.BlinkAnimationFrames[2], typeof(Texture2D), true) as Texture2D;
								_eyeGazeManager.BlinkAnimationFrames[3] = EditorGUILayout.ObjectField("04 - Closed:", _eyeGazeManager.BlinkAnimationFrames[3], typeof(Texture2D), true) as Texture2D;

								break;

							case ImageSource.TextureAtlas:
								_eyeGazeManager.BlinkAnimationAtlas = EditorGUILayout.ObjectField("Animation Atlas:", _eyeGazeManager.BlinkAnimationAtlas, typeof(Texture2D), true) as Texture2D;
								_eyeGazeManager.ReverseAnimation = EditorGUILayout.Toggle("Reverse Animation?", _eyeGazeManager.ReverseAnimation);
								// _eyeGazeManager.SquareAtlasDivision = EditorGUILayout.IntSlider("Square Atlas Division:", _eyeGazeManager.SquareAtlasDivision, 1, 16);
								break;
						}

						EditorGUILayout.Space();
						_eyeGazeManager.BetweenFrameRate = EditorGUILayout.Slider("Tween Time:", _eyeGazeManager.BetweenFrameRate, 0.01f, 0.1f);
						_eyeGazeManager.EyeClosedPause = EditorGUILayout.Slider("Eye Closed Time:", _eyeGazeManager.EyeClosedPause, 0.01f, 1.0f);
						_eyeGazeManager.AddEyeJitter = EditorGUILayout.Toggle("Add Eye Jitter?", _eyeGazeManager.AddEyeJitter);

						EditorGUILayout.Space();
						EditorGUILayout.LabelField("Pivot Rotation Limits:");
						_eyeGazeManager.EyeRotLimitUp	= EditorGUILayout.Slider("Rotate Up Limit:",   _eyeGazeManager.EyeRotLimitUp,   0.0f, 50.0f);
						_eyeGazeManager.EyeRotLimitDown = EditorGUILayout.Slider("Rotate Down Limit:", _eyeGazeManager.EyeRotLimitDown, 0.0f, 50.0f);
						_eyeGazeManager.EyeRotLimitIn	= EditorGUILayout.Slider("Rotate In Limit:",   _eyeGazeManager.EyeRotLimitIn,   0.0f, 50.0f);
						_eyeGazeManager.EyeRotLimitOut	= EditorGUILayout.Slider("Rotate Out Limit:",  _eyeGazeManager.EyeRotLimitOut,  0.0f, 50.0f);

						EditorGUILayout.Space();
						EditorGUILayout.LabelField("Texture Offset Limits:");
						_eyeGazeManager.LimitUp   = EditorGUILayout.Slider("Up Limit:",   _eyeGazeManager.LimitUp,   0.0f, 0.5f);
						_eyeGazeManager.LimitDown = EditorGUILayout.Slider("Down Limit:", _eyeGazeManager.LimitDown, 0.0f, 0.5f);
						_eyeGazeManager.LimitIn   = EditorGUILayout.Slider("In Limit:",   _eyeGazeManager.LimitIn,   0.0f, 0.5f);
						_eyeGazeManager.LimitOut  = EditorGUILayout.Slider("Out Limit:",  _eyeGazeManager.LimitOut,  0.0f, 0.5f);
					}

					break;

				case EyeStyle._3D:
					EditorGUILayout.LabelField("3D Eye Geometry");

					_eyeGazeManager.MeshWithEyeLidShapes = EditorGUILayout.ObjectField("Mesh With Blendshapes:", _eyeGazeManager.MeshWithEyeLidShapes, typeof(GameObject), true) as GameObject;

					EditorGUILayout.Space();

					if (_eyeGazeManager.MeshWithEyeLidShapes != null)
					{
						getBlendshapeNames();

						EditorGUILayout.LabelField("Eyelid Blendshapes:");
						_eyeGazeManager.eyelidTopLeft = EditorGUILayout.Popup("Top-Left: ", _eyeGazeManager.eyelidTopLeft, blendshapes);
						_eyeGazeManager.eyelidBotLeft = EditorGUILayout.Popup("Bot-Left: ", _eyeGazeManager.eyelidBotLeft, blendshapes);
						EditorGUILayout.Space();
						_eyeGazeManager.eyelidTopRight = EditorGUILayout.Popup("Top-Right: ", _eyeGazeManager.eyelidTopRight, blendshapes);
						_eyeGazeManager.eyelidBotRight = EditorGUILayout.Popup("Bot_Right: ", _eyeGazeManager.eyelidBotRight, blendshapes);
					}
					break;
			}
			_serializedTarget.ApplyModifiedProperties();
			// END UI MENU
			EditorGUI.EndChangeCheck();
			EditorGUILayout.LabelField(separator);
			EditorGUILayout.Space();
		}

		private void createLogo()
		{
			// Some "official" logo flair for your customized inspector
			GUILayout.Label(_logo, GUILayout.MaxHeight(128.0f), GUILayout.ExpandWidth(true));
		}

		private void createTitle()
		{
			// Custom Inspector Title Information
			EditorGUILayout.LabelField("VRR Avatar Components ver " + _versionNumber.ToString());

			EditorGUILayout.Space();
		}

		private void getBlendshapeNames()
		{
			SkinnedMeshRenderer headMesh = _eyeGazeManager.MeshWithEyeLidShapes.GetComponent<SkinnedMeshRenderer>();
			blendshapes = new string[headMesh.sharedMesh.blendShapeCount];
			for (int i = 0; i < blendshapes.Length; i++)
			{
				blendshapes[i] = headMesh.sharedMesh.GetBlendShapeName(i);
			}
		}
	}
}