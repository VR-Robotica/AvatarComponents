using System.Collections.Generic;
using System.Collections;
using UnityEngine;

/// <summary>
/// This randomly cycles through a list of potential objects and their points of interest, 
/// that the avatar will attempt to look at. (see Object_OfInterest)
/// 
/// It uses two coroutines: one to cycle through objects, and the second to cycle through an
/// object's points of interest (in case there are more than one)
/// 
/// This will also create a visual frustum collision area that will add or remove objects
/// from the list as they enter or exit the frustum area (see Object_Frustum script).
/// </summary>

namespace com.VR_Robotica.Avatars
{
	[RequireComponent(typeof(Manager_EyeGaze))]
	public class Controller_Interest : MonoBehaviour
	{
		public bool				ShowDebugLog;
		[Space]
		[Header("Objects Available To Look At")]
		public List<GameObject> ObjectsOfInterest;  // Primary
		[Space]
		public GameObject CurrentObject;
		[Space]
		public List<GameObject> PointsOfInterest;   // Secondary
		[Space]
		public GameObject		CurrentlyLookingAt;

		[HideInInspector]
		public string PathToFrustumPrefab = "Prefabs/Frustum";
		[HideInInspector]
		public Vector3 FrustumScale = new Vector3(75, 50, 100);

		// script reference
		private Manager_EyeGaze _eyeGaze;
		private Vector3			_eyeHeight;
		private GameObject		_frustum;
		

		private void Awake()
		{
			ObjectsOfInterest	= new List<GameObject>();
			PointsOfInterest	= new List<GameObject>();

			_eyeGaze			= this.gameObject.GetComponent<Manager_EyeGaze>();
			_eyeHeight			= new Vector3(0, _eyeGaze.Eyes[0].transform.position.y, 0.1f);
		}

		private void Start()
		{
			createFrustum();
			setupFrustum();
			Start_ObjectsCycle();
		}

		private void Update()
		{
			trimInactiveObjects();
		}

		private void LateUpdate()
		{
			if (CurrentObject != null)
			{
				if (!checkLineOfSight(CurrentObject))
				{
					ChangeObjectOfFocus();
				}
			}
		}

		private void trimInactiveObjects()
		{
			// this is a back up function to clean up any inactive elements in the lists
			ObjectsOfInterest.RemoveAll(elem => !elem.activeInHierarchy);
			PointsOfInterest.RemoveAll(elem => !elem.activeInHierarchy);
		}

		public void ChangeObjectOfFocus()
		{
			if (ShowDebugLog) { Debug.Log("Changing Objects Of Focus"); }

			if (ObjectsOfInterest != null && ObjectsOfInterest.Count > 0)
			{
				CurrentObject = pickObjectFromList();
			}
			else
			{
				CurrentObject = null;
			}
			
			// each object change will initiate a reset of points list
			// clearing out its values
			resetPointsOfInterest();
		}

		private GameObject pickObjectFromList()
		{
			if (ShowDebugLog) { Debug.Log("Picking Object From List"); }

			if (ObjectsOfInterest != null && ObjectsOfInterest.Count > 0)
			{
				int randomNumber = UnityEngine.Random.Range(0, ObjectsOfInterest.Count);

				// Visibility Check
				if (checkLineOfSight(ObjectsOfInterest[randomNumber]))
				{
					CurrentObject = ObjectsOfInterest[randomNumber];
					// Line of sight is clear, pass the new object
					return ObjectsOfInterest[randomNumber];
				}
				else
				{
					// line of sight is obstructed, continue looking at current/previous object
					if (ShowDebugLog) { Debug.Log("Defaulting to previous object of interest: " + CurrentObject.name); }
					return CurrentObject;
				}
			}
			else
			{
				return null;
			}
		}

		public void ChangePointOfFocus()
		{
			if (ShowDebugLog) { Debug.Log("Changing Points Of Focus"); }
			if (PointsOfInterest != null && PointsOfInterest.Count > 0)
			{
				float randomNumber = UnityEngine.Random.Range(0f, 100f);
				// define equal chance percentages for each point of interest
				float percentage = 100 / PointsOfInterest.Count;

				for (int i = 0; i < PointsOfInterest.Count; i++)
				{
					// pick points within each percentage range 
					// example: if 10 points exist, 
					// to choose [0] the random range must fall between 0% & 10% 
					// aka (0 * percentage) = 0 AND (1 * percentage) = 10;
					if (randomNumber > (i * percentage) && randomNumber < ((i + 1) * percentage))
					{
						CurrentlyLookingAt = PointsOfInterest[i];
					}
				}
			}
		}

		private void resetPointsOfInterest()
		{
			if (ShowDebugLog) { Debug.Log("Reseting Points Of Interest"); }

			// stop coroutine
			Stop_PointsCycle();
			// clear list
			PointsOfInterest.Clear();
			PointsOfInterest = new List<GameObject>();

			if (CurrentObject != null)
			{
				// get list reference from new object
				Object_OfInterest ooi = CurrentObject.GetComponent<Object_OfInterest>();
				// IF there is an object of interest, add its points values to list 
				if (ooi != null && ooi.PointsOfInterest.Length > 0)
				{
					for (int i = 0; i < ooi.PointsOfInterest.Length; i++)
					{
						PointsOfInterest.Add(ooi.PointsOfInterest[i].gameObject);
					}
				}
				// start coroutine again
				Start_PointsCycle();
			}
		}

		// externally trigger a temporary interuption of the avatar's focus
		public void InteruptCycle(GameObject newObject)
		{
			if (ShowDebugLog) { Debug.Log("Interupting Cycle"); }
			CurrentObject = newObject;
			resetPointsOfInterest();
		}

		private bool checkLineOfSight(GameObject target)
		{
			Ray ray				= new Ray(_eyeHeight, target.transform.position - _eyeHeight);
			RaycastHit hit		= new RaycastHit();

			if (Physics.Raycast(ray, out hit, 100))
			{
				if (hit.transform == target.transform)
				{
					if (ShowDebugLog) { Debug.Log("Raycast SEEs: " + target.name); }
					return true;
				}
				else
				{
					if (ShowDebugLog) { Debug.Log("Raycast CAN'T SEE: " + target.name + " Collided with: " + hit.collider.name); }
					return false;
				}
			}
			else
			{
				if (ShowDebugLog) { Debug.Log("Raycast MISSED"); }
				return false;
			}
		}

		

	#region COROUTINE HANDLERS
		#region CYCLE OBJECT FOCUS
		// Cycle through the list of objects that are in the area of interest
		public void Start_ObjectsCycle()
		{
			Stop_ObjectsCycle();
			objectsCycle_container = objectsCycle();
			StartCoroutine(objectsCycle_container);
		}

		public void Stop_ObjectsCycle()
		{
			if (objectsCycle_container != null)
			{
				StopCoroutine(objectsCycle_container);
				objectsCycle_container = null;
			}
		}

		private IEnumerator objectsCycle_container;
		private IEnumerator objectsCycle()
		{
			if (ShowDebugLog) { Debug.Log("Controller_Interest: Cycle Focus STARTED"); }

			while (true)
			{
				ChangeObjectOfFocus();

				// wait a 'natural' random amount of time...
				yield return new WaitForSeconds(UnityEngine.Random.Range(5.0f, 12.0f));
			}
		}
		#endregion

		#region CYCLE POINTS FOCUS
		// Looking at specific points on the object
		public void Start_PointsCycle()
		{
			Stop_PointsCycle();
			cyclePointsFocus_container = cyclePointsFocus();
			StartCoroutine(cyclePointsFocus_container);
		}

		public void Stop_PointsCycle()
		{
			if (cyclePointsFocus_container != null)
			{
				StopCoroutine(cyclePointsFocus_container);
				cyclePointsFocus_container = null;
			}
		}

		private IEnumerator cyclePointsFocus_container;
		private IEnumerator cyclePointsFocus()
		{
			if (ShowDebugLog) { Debug.Log("Control_FocusOfInterest: CycleThroughFaceObjects STARTED"); }

			while (true)
			{
				ChangePointOfFocus();
				yield return new WaitForSeconds(UnityEngine.Random.Range(1.0f, 4.0f));
			}
		}
		#endregion
	#endregion

		#region CREATE FRUSTUM
		private void createFrustum()
		{
			loadFrustumPrefab();
			// TODO: dynamically build the frustum geometry
		}

		private void loadFrustumPrefab()
		{
			// check if frustum was added as a child in the editor
			if (this.transform.childCount > 0)
			{
				// get child object(s) with the Object_Frustum component (should only be one)
				Object_Frustum[] children = this.transform.GetComponentsInChildren<Object_Frustum>();
				foreach (Object_Frustum frustum in children)
				{
					_frustum = frustum.gameObject;
				}
			}

			if (_frustum == null)
			{
				// if a frustum has NOT been added...
				// instantiate prefab from resource folder
				_frustum = Instantiate(Resources.Load(PathToFrustumPrefab) as GameObject);

				// if the load failed...
				if (_frustum == null)
				{
					Debug.LogWarning("Frustum NOT Loaded.");
					return;
				}
			}
		}

		private void setupFrustum()
		{
			if (_frustum != null)
			{
				_frustum.name = "Frustum";
				// Set to Layer[2] = Ignore Ray Cast
				_frustum.layer = 2;
				_frustum.transform.parent = _eyeGaze.FacingForwardReference;

				// Align and Scale Frustum
				_frustum.transform.position = _eyeHeight;
				_frustum.transform.localScale = FrustumScale;

				_frustum.AddComponent<Rigidbody>();
				_frustum.GetComponent<Rigidbody>().useGravity = false;
				_frustum.GetComponent<Rigidbody>().mass = 0.0f;

				// Add collision triggering script
				Object_Frustum of = _frustum.GetComponent<Object_Frustum>();
				if (of == null)
				{
					Debug.Log("Adding Frustum Controller");
					of = _frustum.AddComponent<Object_Frustum>();
				}

				// setting reference
				of.InterestController = this.gameObject.GetComponent<Controller_Interest>();
			}
		}
		#endregion
	}
}