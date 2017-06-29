using UnityEngine;
using UnityEditor;

namespace com.VR_Robotica.Avatars
{
	[CustomEditor(typeof(Manager_Avatar))]
	public class Editor_Avatar : Editor
	{
		private SerializedObject serializedTarget;
		private GUISkin customSkin;
		private Texture2D logo;
		private Manager_Avatar avatarManager;

		// Eye Gaze
		public bool AddEyeGaze;
		private GameObject forwardReference;
		private Transform LeftEye;
		private Transform RightEye;

		// Eye Blink
		public bool AddEyeBlink;
		public enum EyeType { Three_Dimensions };
		public EyeType AvatarEyeType = EyeType.Three_Dimensions;

		private string[] blendshapes;
		private int topLeft		= 0;
		private int topRight	= 0;
		private int botLeft		= 0;
		private int botRight	= 0;

		// GUI HAck
		private string separator = "_______________________________________________________________________________________";

		private void OnEnable()
		{
			serializedTarget = new SerializedObject(target);
			logo = (Texture2D)EditorGUIUtility.Load("VR_Robotica/LOGO.png");
		}

		public override void OnInspectorGUI()
		{
			if (serializedTarget == null)
			{
				OnEnable();
			}
			serializedTarget.Update();

			// Script Refference
			avatarManager = (Manager_Avatar)target;

			createLogo();
			createTitle();

			AddEyeGaze = EditorGUILayout.Toggle("Add Simulated Eye Gaze:", AddEyeGaze);
			AddEyeBlink = EditorGUILayout.Toggle("Add Eye Blink Control:", AddEyeBlink);

			EditorGUI.BeginChangeCheck();
			createGazeManager();
			createEyeBlink();
			EditorGUI.EndChangeCheck();
		}

		private void createLogo()
		{
			// Some "official" logo flair for your customized inspector
			GUILayout.Label(logo, GUILayout.MaxHeight(128.0f),GUILayout.ExpandWidth(true));
		}

		private void createTitle()
		{
			// Custom Inspector Title Information
			EditorGUILayout.LabelField("VRR Avatar Components ver 0.1");

			EditorGUILayout.Space();
		}

		private void createGazeManager()
		{
			EditorGUILayout.Space();

			

			if (AddEyeGaze)
			{
				avatarManager.AddEyeGaze = AddEyeGaze;

				EditorGUILayout.Space();
				avatarManager.HeadJoint = EditorGUILayout.ObjectField("Head:", avatarManager.HeadJoint, typeof(GameObject), true) as GameObject;

				EditorGUILayout.Space();
				avatarManager.Eyes = new Transform[2];
				avatarManager.Eyes[0] = EditorGUILayout.ObjectField("Left Eye:", avatarManager.Eyes[0],  typeof(Transform), true) as Transform;
				avatarManager.Eyes[1] = EditorGUILayout.ObjectField("Right Eye:", avatarManager.Eyes[1], typeof(Transform), true) as Transform;
			}

			EditorGUILayout.LabelField(separator);
		}

		private void createEyeBlink()
		{
			EditorGUILayout.Space();

			if (AddEyeBlink)
			{
				EditorGUILayout.Space();

				avatarManager.AddEyeBlink	= AddEyeBlink;
				avatarManager.HeadMesh		= EditorGUILayout.ObjectField("Animate This Mesh:", avatarManager.HeadMesh, typeof(GameObject), true) as GameObject;

				EditorGUILayout.Space();

				if (avatarManager.HeadMesh != null)
				{
					EditorGUILayout.LabelField("Blendshapes:");
					getBlendshapeNames();

					//number  = EditorGUILayout.Popup("Blend System", number, blendSystemNames.ToArray(), GUIStyle.none);
					topLeft  = EditorGUILayout.Popup("- Eyelid Top-Left: ",  topLeft,  blendshapes);
					topRight = EditorGUILayout.Popup("- Eyelid Top-Right: ", topRight, blendshapes);
					botLeft  = EditorGUILayout.Popup("- Eyelid Bot-Left: ",  botLeft,  blendshapes);
					botRight = EditorGUILayout.Popup("- Eyelid bot_Right: ", botRight, blendshapes);
				}
			}

			EditorGUILayout.LabelField(separator);
		}

		private void getBlendshapeNames()
		{
			SkinnedMeshRenderer headMesh = avatarManager.HeadMesh.GetComponent<SkinnedMeshRenderer>();
			blendshapes = new string[headMesh.sharedMesh.blendShapeCount];
			for (int i = 0; i < blendshapes.Length; i++)
			{
				blendshapes[i] = headMesh.sharedMesh.GetBlendShapeName(i);
			}
		}
	}
}