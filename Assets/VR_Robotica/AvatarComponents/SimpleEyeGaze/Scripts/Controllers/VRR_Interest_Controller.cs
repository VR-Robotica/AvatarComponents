using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using com.VR_Robotica.AvatarComponents.Objects;

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

namespace com.VR_Robotica.AvatarComponents.Controllers
{
	[RequireComponent(typeof(VRR_SimpleEyeGaze))]
	public class VRR_Interest_Controller : MonoBehaviour
	{
		public bool				ShowDebugLog;
        public Vector3 FrustumOffSet;
        [Space]
		[Header("Objects Available To Look At")]
		public List<GameObject> ObjectsOfInterest;  // Primary
		[Space]
		public GameObject CurrentObject;
		[Space]
		public List<GameObject> PointsOfInterest;   // Secondary
		[Space]
		public GameObject		CurrentlyLookingAt;
	
		// script reference
		private VRR_SimpleEyeGaze _eyeGaze;
		private GameObject		_frustum;

		private bool _isReady;

		private void Awake()
		{
			ObjectsOfInterest	= new List<GameObject>();
			PointsOfInterest	= new List<GameObject>();

			_eyeGaze			= this.gameObject.GetComponent<VRR_SimpleEyeGaze>();			
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
				VRR_ObjectOfInterest ooi = CurrentObject.GetComponent<VRR_ObjectOfInterest>();
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
			Ray ray				= new Ray(FrustumOffSet, target.transform.position - FrustumOffSet);
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

		public IEnumerator Create()
		{
            if (!_isReady)
            {

                _frustum = new GameObject();
                _frustum.transform.SetParent(_eyeGaze.FacingForwardReference);

                // use a quick reference to pass the offset value
                VRR_Frustum_Object frustumComponent = _frustum.AddComponent<VRR_Frustum_Object>();
                frustumComponent.InterestController = this.gameObject.GetComponent<VRR_Interest_Controller>();
                frustumComponent.FrustumOffSet = FrustumOffSet;
                

                yield return frustumComponent.Create();

                Start_ObjectsCycle();

                _isReady = true;
            }

            yield return null;
		}
	}
}