using UnityEngine;

/// <summary>
/// This script simply adds and removes potential ObjectsOfInterest from the FocusOfInterest Script
/// when objects enter or exit the collision area of the Frustum Collider Geometry
/// </summary>

namespace com.VR_Robotica.Avatars
{
	public class Object_Frustum : MonoBehaviour
	{
		[Tooltip("You can place script here, or it will be discovered if left null")]
		// create reference to Control Script to change list values
		public Controller_Interest InterestController;
		[Space]
		public bool ShowDebugLog;

		private void Awake()
		{
			getReferences();
		}
		
		private void getReferences()
		{
			if (InterestController == null)
			{
				InterestController = GameObject.Find("Avatar").GetComponent<Controller_Interest>();

				if (InterestController == null)
				{
					Debug.LogWarning("Controller_Interest Script Not Found!");
				}
			}
		}

		void OnTriggerEnter(Collider col)
		{
			Object_OfInterest ooi = col.gameObject.GetComponent<Object_OfInterest>();

			if ( ooi != null)
			{
				Debug.Log(col.name + " is an object of interest.");

				// add game object to primary list
				InterestController.ObjectsOfInterest.Add(col.gameObject);
				// interupt cycle
				InterestController.InteruptCycles(col.gameObject);
			}
		}

		void OnTriggerExit(Collider col)
		{
			// Remove Exited Object from the Objects Of Interest List in the Parent Script
			Object_OfInterest ooi = col.gameObject.GetComponent<Object_OfInterest>();

			if (ooi != null)
			{
				// remove game object from primary list
				InterestController.ObjectsOfInterest.Remove(col.gameObject);

				// remove points from secondary list
				if (ooi.PointsOfInterest.Length > 0)
				{
					for (int i = 0; i < ooi.PointsOfInterest.Length; i++)
					{
						InterestController.PointsOfInterest.Remove(ooi.PointsOfInterest[i].gameObject);
					}
				}
			}

			// dependancy on InterestController
			InterestController.ChangeFocus();

		}
	}
}