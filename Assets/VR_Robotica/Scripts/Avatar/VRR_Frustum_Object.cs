using UnityEngine;

/// <summary>
/// This script simply adds and removes potential ObjectsOfInterest from the Controller_Interest Script
/// when objects enter or exit the collision area of the Frustum Collider Geometry
/// </summary>

namespace com.VR_Robotica.Avatars
{
	public class VRR_Frustum_Object : MonoBehaviour
	{
		[Tooltip("You can place script here, or it will be discovered if left null")]
		// create reference to Control Script to change list values
		public VRR_Interest_Controller InterestController;
		[Space]
		public bool ShowDebugLog;

		private bool  _isReady;

		private void Start()
		{
			getReferences();
		}

		private void getReferences()
		{
			if (InterestController == null)
			{
				Debug.Log("Frustum: interest Controller Not Set, finding Avatar");
				InterestController = GameObject.Find("Avatar").GetComponent<VRR_Interest_Controller>();

				if (InterestController == null)
				{
					Debug.LogWarning("Controller_Interest Script Not Found!");
					return;
				}
			}

			_isReady = true;
		}

		void OnTriggerEnter(Collider col)
		{
			if (_isReady)
			{
				if (col.gameObject != InterestController.gameObject)
				{
					VRR_ObjectOfInterest ooi = col.gameObject.GetComponent<VRR_ObjectOfInterest>();

					if (ooi != null)
					{
						//Debug.Log(col.name + " is an object of interest.");
						// add game object to primary list
						InterestController.ObjectsOfInterest.Add(col.gameObject);
						// interupt cycle
						InterestController.InteruptCycle(col.gameObject);
					}
				}
			}
		}

		/// <summary>
		/// Remove Exited Object from the Objects Of Interest List in the Controller_Interest script
		/// If the exited object was being looked at, refresh the focus (clear points of interest and
		/// the currently looked at objects)
		/// </summary>
		/// <param name="col"></param>
		void OnTriggerExit(Collider col)
		{
			if (_isReady)
			{
				
				VRR_ObjectOfInterest ooi = col.gameObject.GetComponent<VRR_ObjectOfInterest>();

				if (ooi != null)
				{
					// IF the object exiting is the CURRENT OBJECT being looked at
					// clear everything...
					if (ooi.gameObject == InterestController.CurrentObject)
					{
						InterestController.CurrentlyLookingAt	= null;
						InterestController.CurrentObject		= null;
					}
					// remove object from list
					InterestController.ObjectsOfInterest.Remove(col.gameObject);
					InterestController.ChangeObjectOfFocus();
				}
			}
		}
	}
}