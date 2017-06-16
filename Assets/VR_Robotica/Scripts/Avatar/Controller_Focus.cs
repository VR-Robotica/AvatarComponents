using System.Collections;
using UnityEngine;


/// <summary>
/// This is a simple gameObject that the avatar's eyes will be oriented to.
/// </summary>
namespace com.VR_Robotica.Avatars
{
	public class Controller_Focus : MonoBehaviour
	{
		[HideInInspector]
		public GameObject	Controller;

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

		/// <summary>
		/// Move the FocusController gameObject to this Target Transform 
		/// POSITION at this rate of SPEED.
		/// </summary>
		/// <param name="target"></param>
		/// <param name="speed"></param>
		public void moveTo(Vector3 target, float speed)
		{
			_start = Controller.transform.position;
			_target = target;
			_speed = speed;
		}

		private IEnumerator moving()
		{
			while (true)
			{
				Controller.transform.position = Vector3.Lerp(_start, _target, Time.deltaTime * _speed);
				yield return null;
			}
		}

		private void create()
		{
			Controller = new GameObject();
			Controller.name = "Object Of Focus";
			// make sure it does not interfere with any ray casting
			// Layer[2] = Ignore Raycast
			Controller.layer = 2;

			Controller.AddComponent<Rigidbody>();
			Controller.GetComponent<Rigidbody>().useGravity = false;
			Controller.GetComponent<Rigidbody>().mass = 0.0f;

			//add collider
			Controller.AddComponent<SphereCollider>();
			Controller.GetComponent<SphereCollider>().radius = 0.01f;
			Controller.GetComponent<SphereCollider>().isTrigger = true;
		}
	}
}