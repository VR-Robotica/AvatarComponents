using UnityEngine;

/// <summary>
/// This script simply adds and removes potential ObjectsOfInterest from the FocusOfInterest Script
/// when objects enter or exit the collision area of the Frustum Collider Geometry
/// </summary>

namespace com.VR_Robotica.Avatars
{
	public class Controller_Frustum : MonoBehaviour
	{
		[Tooltip("You can place script here, or it will be discovered if left null")]
		// create reference to Control Script to change list values
		public Controller_Interest ParentScript;
		[Tooltip("These will be set by the parent script")]
		public string[] FocusOfInterestObjectTags;

		[Space]
		public bool ShowDebugLog;

		// *** May need to create a loading coroutine to accommodate objects of interest already 
		// being inside of collision area when collider is creted.  in the meantime, the Awake()
		// function seems to get things done early enough to not need it.

		private void Awake()
		{
			if (ParentScript == null)
			{
				ParentScript = GameObject.Find("Avatar").GetComponent<Controller_Interest>();

				if(ParentScript == null)
				{
					Debug.LogWarning("Avatar_FocusOfInterest Script Not Found!");
				}
			}
		}
		
		void OnTriggerEnter(Collider col)
		{
			if(col.gameObject.GetComponent<Object_Interest>() != null)
			{

			}
			if (FocusOfInterestObjectTags.Length > 0)
			{
				for (int i = 0; i < FocusOfInterestObjectTags.Length; i++)
				{
					if (col.tag == FocusOfInterestObjectTags[i])
					{
						ParentScript.ObjectsOfInterest.Add(col.gameObject);
						ParentScript.InteruptCycles(col.gameObject);

						if (ShowDebugLog)
							Debug.Log("FrustumCollision: Triggered by " + col);
					}
				}
			}
		}

		void OnTriggerExit(Collider col)
		{
			// Remove Exited Object from the Objects Of Interest List in the Parent Script

			// Check TAG array to make sure there are TAGS to be watching
			if (FocusOfInterestObjectTags.Length > 0)
			{
				for (int i = 0; i < FocusOfInterestObjectTags.Length; i++)
				{
					// Check to make sure object that is exiting is TAGGED as something of interest
					if (col.tag == FocusOfInterestObjectTags[i])
					{
						// Remove object from list
						ParentScript.ObjectsOfInterest.Remove(col.gameObject);

						// Check if object is also currently being looked at
						if(col.gameObject == ParentScript.CurrentlyLookingAtThis)
						{
							// remove that too
							ParentScript.CurrentlyLookingAtThis = null;
						}

						if (ShowDebugLog)
							Debug.Log("FrustumCollision: Exited by " + col);
					}
				}
			}
		}
	}
}