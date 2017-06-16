using System.Collections;
using UnityEngine;


/// <summary>
/// This is a simple gameObject that the avatar's eyes will be oriented to.
/// </summary>
namespace com.VR_Robotica.Avatars
{
	public class Avatar_FocusController : MonoBehaviour
	{
		public GameObject	FocusController;

		private Vector3		_start;
		private Vector3		_target;
		private float		_speed;

		private void Start()
		{ 
			create();

			// Initialize first position...
			moveTo(new Vector3(0, 0, 10), 10.0f);

			StartCoroutine(moving());
		}

		private void create()
		{
			FocusController = new GameObject();
			FocusController.name = "Object Of Focus";
		}

		/// <summary>
		/// Move the FocusController gameObject to this Target Transform 
		/// POSITION at this rate of SPEED.
		/// </summary>
		/// <param name="target"></param>
		/// <param name="speed"></param>
		public void moveTo(Vector3 target, float speed)
		{
			_start	= FocusController.transform.position;
			_target = target;
			_speed	= speed;
		}

		private IEnumerator moving()
		{
			while (true)
			{
				FocusController.transform.position = Vector3.Lerp(_start, _target, Time.deltaTime * _speed);
				yield return null;
			}
		}
	}
}