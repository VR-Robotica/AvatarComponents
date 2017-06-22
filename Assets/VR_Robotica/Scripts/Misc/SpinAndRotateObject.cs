using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpinAndRotateObject : MonoBehaviour
{
	public float Speed = 10.0f;
	private GameObject childObject;

	private void Awake()
	{
		childObject = this.transform.GetChild(0).gameObject;
		Debug.Log(childObject.name);
	}

	// Use this for initialization
	void Start ()
	{
		
	}
	
	// Update is called once per frame
	void Update ()
	{
		// main rotation
//		this.transform.Rotate(Vector3.right * Time.deltaTime * Speed);
		this.transform.Rotate(Vector3.up * Time.deltaTime * Speed, Space.World);

		// object rotation
		if (childObject != null)
		{
			childObject.transform.Rotate(Vector3.right * Time.deltaTime * Speed);
			childObject.transform.Rotate(Vector3.up * Time.deltaTime * Speed);
		}
	}
}
