using System.Collections.Generic;
using System.Collections;
using UnityEngine;

/// <summary>
/// This randomly cycles through a list of potential objects the avatar 
/// will attempt to look at. If an object has a 'focusPoint' (or a bunch
/// of focusPoints), it will look at those, otherwise it will look at
/// the pivotPoint.
/// 
/// This will also create a visual frustum collision area that will add or remove objects
/// from the list as they enter or exit the frustum area (see FrustumCollision script).
/// </summary>

namespace com.VR_Robotica.Avatars
{
	[RequireComponent(typeof(Manager_EyeGaze))]
	public class Controller_Interest : MonoBehaviour
	{
		public bool				ShowDebugLog;
		[Space]
		public string			PathToFrustumPrefab	= "Prefabs/Frustum";
		public Vector3			FrustumScale		= new Vector3(75, 50, 100);
		[Space]
		[Header("Objects Available To Look At")]
		public List<GameObject> ObjectsOfInterest;	// Primary
		public List<GameObject> PointsOfInterest;   // Secondary
		[Space]
		[Header("Current Focus")]
		public GameObject		CurrentObject;
		public GameObject		CurrentlyLookingAtThis;

		// script reference
		private Manager_EyeGaze _eyeGaze;
		private Vector3			_centerOfEyes;
		private GameObject		_lastObjectLookedAt;
		private float		_holdFocus;
		private float		_holdMicrosaccade;
		private Vector3		_microsaccadeOffset;
		private bool		_isHoldingMicrosaccade;

		private void Awake()
		{
			ObjectsOfInterest	= new List<GameObject>();
			PointsOfInterest	= new List<GameObject>();

			_eyeGaze			= this.gameObject.GetComponent<Manager_EyeGaze>();
		}

		private void Start()
		{
			createFrustum();
			findCenterOfEyes();

			Start_CycleFocus();
		}

		private void Update()
		{
			trimInactiveObjects();
		}
	
		private void createFrustum()
		{
			loadFrustumPrefab();
			// TODO: dynamically build the frustum geometry
		}

		private void loadFrustumPrefab()
		{
			// check if frustum was added as a child in the editor
			GameObject frustumReference = GameObject.Find("Frustum");
			if ( frustumReference == null)
			{
				// if a frustum has NOT been added...
				// instantiate prefab from resource folder
				frustumReference = Instantiate(Resources.Load(PathToFrustumPrefab) as GameObject);
				
				// if the load failed...
				if (frustumReference == null)
				{
					Debug.LogWarning("Frustum NOT Loaded.");
					return;
				}
			}
			else
			{
				Debug.Log("Frustum was added in the Editor");
			}

			frustumReference.name = "Frustum";
			// Set to Layer[2] = Ignore Ray Cast
			frustumReference.layer = 2; 
			frustumReference.transform.parent = this.transform;
			
			// Align and Scale Frustum
			frustumReference.transform.localPosition = new Vector3(0, _eyeGaze.LeftEye.transform.position.y, 0.1f);
			frustumReference.transform.localScale = FrustumScale;

			frustumReference.AddComponent<Rigidbody>();
			frustumReference.GetComponent<Rigidbody>().useGravity = false;
			frustumReference.GetComponent<Rigidbody>().mass = 0.0f;

			// Add collision triggering script
			if (frustumReference.GetComponent<Object_Frustum>() == null)
			{
				Debug.Log("Adding Frustum Controller");
				frustumReference.AddComponent<Object_Frustum>();
			}
		}

		private void findCenterOfEyes()
		{
			// startPos is midpoint between eyes to simulate line of sight
			_centerOfEyes = _eyeGaze.LeftEye.transform.position + (_eyeGaze.RightEye.transform.position - _eyeGaze.LeftEye.transform.position) / 2;
			//_centerOfEyes = new Vector3(0.0f, 0.9f, 0.1f);
		}

		public void ChangeFocus()
		{
			CurrentlyLookingAtThis = pickObjectFromList();
		}

		private GameObject pickObjectFromList()
		{
			if (ShowDebugLog) { Debug.Log("Controller_Interest: Picking Object From List"); }
			if (ObjectsOfInterest != null && ObjectsOfInterest.Count > 1)
			{
				int randomNumber = UnityEngine.Random.Range(0, ObjectsOfInterest.Count);

				// Visibility Check
				if (checkLineOfSight(ObjectsOfInterest[randomNumber]))
				{
					_focusHasChanged = true;
					CurrentObject = ObjectsOfInterest[randomNumber];
					// Line of sight is clear, pass the new object
					return ObjectsOfInterest[randomNumber];
				}
				else
				{
					// line of sight is obstructed, continue looking at current/previous object
					if (ShowDebugLog) { Debug.Log("Defaulting to previous object of interest: " + CurrentlyLookingAtThis.name); }
					return CurrentlyLookingAtThis;
				}
			}
			else
			{
				return null;
			}
		}

		private bool checkLineOfSight(GameObject target)
		{
			Ray ray				= new Ray(_centerOfEyes, target.transform.position - _centerOfEyes);
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

		private void trimInactiveObjects()
		{
			ObjectsOfInterest.RemoveAll(elem => !elem.activeInHierarchy);
			PointsOfInterest.RemoveAll(elem => !elem.activeInHierarchy);
		}

#region COROUTINE HANDLERS

		#region EVENT - Focus Change
		private bool _focusHasChanged;
		private void onObjectFocusChange()
		{
			if (ShowDebugLog) { Debug.Log("Object Focus Changed"); }

			// stop coroutine
			Stop_PointsCycle();
			// clear list
			PointsOfInterest = new List<GameObject>();
			// get list reference from new object
			Object_OfInterest ooi = CurrentlyLookingAtThis.GetComponent<Object_OfInterest>();
			// add values to list from object refference
			if (ooi != null && ooi.PointsOfInterest.Length > 0)
			{
				for (int i = 0; i < ooi.PointsOfInterest.Length; i++)
				{
					PointsOfInterest.Add(ooi.PointsOfInterest[i].gameObject);
				}
			}
			// start coroutine again
			Start_PointsCycle();
			// finish off the event
			_focusHasChanged = false;
		}
		#endregion
		
		#region CYCLE OBJECT FOCUS
		// Cycle through the list of objects that are in the area of interest
		public void Start_CycleFocus()
		{
			Stop_CycleFocus();
			cycleObjectFocus_Container = cycleObjectFocus();
			StartCoroutine(cycleObjectFocus_Container);
		}

		public void Stop_CycleFocus()
		{
			if (cycleObjectFocus_Container != null)
			{
				StopCoroutine(cycleObjectFocus_Container);
				cycleObjectFocus_Container = null;
			}
		}

		private IEnumerator cycleObjectFocus_Container;
		private IEnumerator cycleObjectFocus()
		{
			if (ShowDebugLog) { Debug.Log("Controller_Interest: Cycle Focus STARTED"); }

			while (true)
			{
				// check to see if anything is in the list
				if (ObjectsOfInterest.Count > 0)
				{
					ChangeFocus();

					// handle the the focus change event...
					if (_focusHasChanged)
					{
						onObjectFocusChange();
						// checks if there are points of interests on the object
					}
				}
				else
				{
					// else, You're NOT looking at anything
					InteruptCycles(null);
				}

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
			while (true)
			{
				if (ShowDebugLog) { Debug.Log("Control_FocusOfInterest: CycleThroughFaceObjects STARTED"); }

				if (PointsOfInterest != null && PointsOfInterest.Count > 0)
				{
					float randomNumber = UnityEngine.Random.Range(0f, 100f);
					float percentage = 100 / PointsOfInterest.Count;

					for (int i = 0; i < PointsOfInterest.Count; i++)
					{
						if (randomNumber > (i * percentage) && randomNumber < ((i + 1) * percentage))
						{
							CurrentlyLookingAtThis = PointsOfInterest[i];
							yield return new WaitForSeconds(UnityEngine.Random.Range(3.0f, 8.0f));
						}
					}
				}
				else
				{
					CurrentlyLookingAtThis = null;
					yield return null;
				}
			}
		}
		#endregion

		#region INTERUPT CYCLE
		// externally trigger a temporary interuption of the avatar's focus
		public void InteruptCycles(GameObject newObject)
		{
			if (newObject != null)
			{
				// interupt the cycle by stopping them
				Stop_CycleFocus(); 
				Stop_PointsCycle();
				// now look at the new object
				CurrentlyLookingAtThis = newObject;
				// and restart the basic cycle again
				Start_CycleFocus();
			}
		}
		#endregion

#endregion
	}
}