using System.Collections;
using UnityEngine;


/// <summary>
/// This is a simple gameObject that the avatar's eyes will be oriented to.
/// </summary>
namespace com.VR_Robotica.Avatars
{
	public class Avatar_FocusController : MonoBehaviour
	{
		public GameObject FocusController;

		private void Start()
		{ 
			create();
		}

		private void create()
		{
			FocusController = new GameObject();
			FocusController.name = "Object Of Focus";
		}

		/// <summary>
		/// Move the FocusController gameObject to this TARGET Transform 
		/// Position at this rate of SPEED.
		/// </summary>
		/// <param name="target"></param>
		/// <param name="speed"></param>
		public void moveTo(Transform target, float speed)
		{
			StartCoroutine(moving(target, speed));
		}

		private IEnumerator moving(Transform target, float speed)
		{
			Vector3 startPos = FocusController.transform.position;

			while (true)
			{
				FocusController.transform.position = Vector3.Lerp(startPos, target.position, Time.deltaTime * speed);
				yield return null;
			}
		}
	}
}