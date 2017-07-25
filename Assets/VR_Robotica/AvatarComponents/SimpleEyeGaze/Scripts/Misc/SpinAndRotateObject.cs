using System.Collections;
using UnityEngine;

public class SpinAndRotateObject : MonoBehaviour
{
	public float Speed = 10.0f;
	private GameObject childObject;

	private void Awake()
	{
		childObject = this.transform.GetChild(0).gameObject;
	}

	// Use this for initialization
	void Start ()
	{
		StartCoroutine(spinAndRotate());		
	}
	
	// Update is called once per frame
	void Update ()
	{
	}

	private IEnumerator spinAndRotate()
	{
		while (true)
		{
			// main rotation
			this.transform.Rotate(Vector3.up * Time.deltaTime * Speed, Space.World);

			// object rotation
			if (childObject != null)
			{
				childObject.transform.Rotate(Vector3.right  * Time.deltaTime * Speed);
				childObject.transform.Rotate(Vector3.up		* Time.deltaTime * Speed);
			}
			yield return null;
		}
	}
}
