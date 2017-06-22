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
		[Header("Avatar Anatomy")]
		[Space]
		[Header("Eye Blink")]
		public GameObject HeadMesh;
		public int[] eyelidBlendshapeTop;
		public int[] eyelidBlendshapeBot;
		[Header("Eye Gaze")]
		public Transform[] Eyes;
		[Header("IK Refs")]
		public Transform LeftHand;
		public Transform RightHand;
		public Transform LeftFoot;
		public Transform RightFoot;

		private AudioListener	_audioListener;
		private AudioSource		_audioSource;

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
			checkForRequiredComponents();
			yield return null;
		}

		private void checkForRequiredComponents()
		{
			#region AUDIO LISTENER

			if (LocalPlayer)
			{
				_audioListener = this.gameObject.GetComponent<AudioListener>();

				if (_audioListener == null)
				{
					_audioListener = this.gameObject.AddComponent<AudioListener>();
				}
			}

			#endregion

			#region AUDIO SOURCE

			_audioSource = this.gameObject.GetComponent<AudioSource>();

			if(_audioSource == null)
			{
				_audioSource = this.gameObject.AddComponent<AudioSource>();
			}

			#endregion
		}
		
	}
}