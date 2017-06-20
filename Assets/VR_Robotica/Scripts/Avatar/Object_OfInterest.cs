using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace com.VR_Robotica.Avatars
{
	/// <summary>
	/// Attach this script to any object to Identify it as something
	/// the Avatar is interested in looking at.
	/// </summary>
	public class Object_OfInterest : MonoBehaviour
	{
		[Tooltip("Define Specific Areas of an object that an avatar can cycle through and look at.")]
		public Transform[] PointsOfInterest;

		// TODO: Add weighted priorities for Object and Points
		// public int ObjectPriority;
		// public int[] PointsPriority;

		private void Awake()
		{
			createCollider();
			createPointOfInterest();
		}

		private void createCollider()
		{
			Collider collider = this.gameObject.GetComponent<Collider>();

			if(collider == null)
			{
				Debug.LogWarning("This Object Needs a Custom Collider, creating default.");
				this.gameObject.AddComponent<BoxCollider>();
			}
		}

		private void createPointOfInterest()
		{
			// In case no points of interests are added to the object
			// through the inspector, default to the object's pivot point.
			if (PointsOfInterest == null || PointsOfInterest.Length == 0)
			{
				PointsOfInterest = new Transform[1];
				PointsOfInterest[0] = this.transform;
			}
		}
	}
}