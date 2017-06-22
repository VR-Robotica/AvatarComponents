using System.Collections;
using System.Collections.Generic;
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

		private string[] blendshapes;
		private int topLeft		= 0;
		private int topRight	= 0;
		private int botLeft		= 0;
		private int botRight	= 0;



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
			createGazeManager();
			createEyeBlink();
		}

		private void createLogo()
		{
			// Some "official" logo flair for your customized inspector
			GUILayout.Label(logo, GUILayout.MaxHeight(128.0f),GUILayout.ExpandWidth(true));
		}

		private void createTitle()
		{
			// Custom Inspector Title Information
			EditorGUILayout.LabelField("VRR Avatar Manager");
			EditorGUILayout.Space();
		}

		private void createGazeManager()
		{
			EditorGUILayout.LabelField("Eye Gaze Dependencies");
			EditorGUILayout.Space();
		}

		private void createEyeBlink()
		{
			EditorGUILayout.LabelField("Eye Blink Dependencies");
			EditorGUILayout.Space();

			avatarManager.HeadMesh = EditorGUILayout.ObjectField("Head Mesh:", avatarManager.HeadMesh, typeof(GameObject), true) as GameObject;

			if (avatarManager.HeadMesh != null)
			{
				getBlendshapes();
				
				// Start Watching for Changes...
				EditorGUI.BeginChangeCheck();
				//number  = EditorGUILayout.Popup("Blend System", number, blendSystemNames.ToArray(), GUIStyle.none);
				topLeft  = EditorGUILayout.Popup("Eyelid Top-Left: ",  topLeft,  blendshapes);
				topRight = EditorGUILayout.Popup("Eyelid Top-Right: ", topRight, blendshapes);
				botLeft  = EditorGUILayout.Popup("Eyelid Bot-Left: ",  botLeft,  blendshapes);
				botRight = EditorGUILayout.Popup("Eyelid bot_Right: ", botRight, blendshapes);

				EditorGUI.EndChangeCheck();
			}
			else
			{

			}
		}

		private void getBlendshapes()
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