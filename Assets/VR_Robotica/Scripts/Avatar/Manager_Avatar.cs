using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace com.VR_Robotica.Avatars
{
	/// <summary>
	/// This is the Main Class for Avatars which manages all the contributing components and
	/// their dependencies.
	/// </summary>
	public class Manager_Avatar : MonoBehaviour
	{
		public bool LocalPlayer;
		[Space]

		[Header("Eye Gaze")]
		public bool AddEyeGaze;
		public GameObject HeadJoint;
		public Transform[] Eyes;

		[Header("Eye Blink")]
		public bool			AddEyeBlink;

		public enum EyeType { Three_Dimensions };
		public EyeType		AvatarEyeType = EyeType.Three_Dimensions;

		public GameObject	HeadMesh;
		public int[]		eyelidBlendshapeTop;
		public int[]		eyelidBlendshapeBot;

		

		// Use this for initialization
		void Start()
		{
			StartCoroutine(setup());
		}

		// Update is called once per frame
		void Update()
		{

		}

		private IEnumerator setup()
		{
			/*
			checkForRequiredComponents();
			if(AddEyeBlink)
			{
				addEyeBlink();
			}

			if(AddSimulatedEyeGaze)
			{
				addEyeGaze();
			}
			*/
			yield return null;
		}

		private void checkForRequiredComponents()
		{
			
		}

		private void addEyeBlink()
		{

		}

		private void addEyeGaze()
		{

		}

	}
}