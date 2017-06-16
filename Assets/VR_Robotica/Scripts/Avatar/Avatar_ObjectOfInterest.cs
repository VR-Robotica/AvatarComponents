using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace com.VR_Robotica.Avatars
{
	/// <summary>
	/// Used to Identify Objects that an Avatar can look at.
	/// </summary>
	public class Avatar_ObjectOfInterest : MonoBehaviour
	{
		/// <summary>
		/// Define Specific Areas of an object that an avatar can cycle
		/// through and look at.
		/// </summary>
		public Transform[] PointsOfInterest;

		private void Start()
		{
			createCollider();
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
	}
}