using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace com.VR_Robotica.AvatarComponents
{
	/// <summary>
	/// Attach this script to any object to Identify it as something the Avatar is 
	/// interested in looking at. A designer can specify which points on the object 
	/// are of interest by adding them to the PointsOfInterest Array in the editor.
	/// </summary>
	public class VRR_ObjectOfInterest : MonoBehaviour
	{
		[Tooltip("Toggle to AUTOMATICALLY populate PointsOfInterest with all the children of this object")]
		public bool UseAllChildren;
		[Tooltip("Define Specific Areas of an object that an avatar can cycle through and look at.")]
		public Transform[] PointsOfInterest;

		// TODO: Add weighted priorities for Object and Points
		// public int ObjectPriority;
		// public int[] PointsPriority;

		private void Awake()
		{
			createCollider();
			createPointsOfInterest();
		}

		private void createCollider()
		{
			Collider collider = this.gameObject.GetComponent<Collider>();

			if(collider == null)
			{
				Debug.LogWarning("This Object Needs a Custom Collider, creating default.");
				SphereCollider sc = this.gameObject.AddComponent<SphereCollider>();
				sc.isTrigger = true;
				sc.radius = 0.1f;
			}
		}

		private void createPointsOfInterest()
		{
			// If there are no points of interests manually added in the inspector...
			if (PointsOfInterest == null || PointsOfInterest.Length == 0)
			{
				// check the boolean toggle to automatically populate with the children
				if (UseAllChildren)
				{
					PointsOfInterest = new Transform[this.transform.childCount];
					for (int i = 0; i < PointsOfInterest.Length; i++)
					{
						PointsOfInterest[i] = this.transform.GetChild(i);
					}

					if(PointsOfInterest.Length == 0)
					{
						Debug.LogWarning(this.gameObject.name + ": ERROR : No Children were found.");
						return;
					}
				}
				else
				{
					// or default to using the object's pivot point
					PointsOfInterest = new Transform[1];
					PointsOfInterest[0] = this.transform;
				}
			}
		}
	}
}