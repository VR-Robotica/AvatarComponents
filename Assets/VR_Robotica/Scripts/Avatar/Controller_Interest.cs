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
	public class Controller_Interest : MonoBehaviour
	{
		public Collider		PlayerFrustum;
		public Vector3		FrustumScale;

		[Header("List of objects as they enter the Frustum")]
		public List<GameObject> ObjectsOfInterest = new List<GameObject>();
		public GameObject CurrentlyLookingAtThis;

		public float FocusOnFaceDistance = 1.0f;

		// script reference
		private Manager_EyeGaze _eyeGaze;

		// for microsaccade
		private GameObject	_lastObjectLookedAt;
		private float		_holdFocus;
		private float		_holdMicrosaccade;
		private Vector3		_microsaccadeOffset;
		private bool		_isHoldingMicrosaccade;

		[Space]
		public bool ShowDebugLog;
		
		void Start()
		{
			loadFrustumPrefab();

			StartAvatarCoroutines();
		}

		void Update()
		{
			TrimInactiveObjects();
			
			if (ObjectsOfInterest.Count == 0)
			{
				CurrentlyLookingAtThis = null;
			}

			// if there is something to look at...
			// ...MOVE the FocusTarget to its position
			if (CurrentlyLookingAtThis != null)
			{

			}
		}
		 
		private void StartAvatarCoroutines()
		{
			Start_CycleFocus();
		}

		private void createFrustum()
		{
			// i would like to dynamically create the frustum geometry,
		}

		// but for now we'll simply load a frustum prefab...
		private void loadFrustumPrefab()
		{
			GameObject frustumReference = Instantiate(Resources.Load("Avatars/Frustum") as GameObject);
			frustumReference.name = "Frustum";
			frustumReference.layer = 2; // Layer[2] = Ignore Ray Cast
			//frustumReference.transform.parent = GameObject.Find("Player").transform;
			frustumReference.transform.parent = GameObject.Find("Avatar").transform;
			//frustumReference.transform.localPosition = new Vector3(0f, 0f, 0.415f);

			frustumReference.AddComponent<Rigidbody>();
			frustumReference.GetComponent<Rigidbody>().useGravity = false;
			frustumReference.GetComponent<Rigidbody>().mass = 0.0f;

			// add collision triggering script
			frustumReference.AddComponent<Controller_Frustum>();

			// get reference to collider for trigger events
			PlayerFrustum = frustumReference.GetComponent<Collider>();
		}

		private float CastRayFromEyes()
		{
			Vector3 startPos = this.transform.position;
			Ray ray = new Ray(startPos, Vector3.forward);
			RaycastHit hit = new RaycastHit();

			return hit.distance;
		}

		private GameObject pickObjectFromArray()
		{
			if (ShowDebugLog)
				Debug.Log("FocusOfInterest: Picking Object From Array");

			int randomNumber = UnityEngine.Random.Range(0, ObjectsOfInterest.Count);

			// Visibility Check
			// check to see if user has clear line-of-sight to the new target 
			// by sending raycast to check for any collisions between user and 
			// the object of interest
			if (checkLineOfSight(ObjectsOfInterest[randomNumber]))
			{
				// Line of sight is clear, pass the new object
				return ObjectsOfInterest[randomNumber];
			}
			else
			{
				// line of sight is obstructed, continue looking at current/previous object
			//	Debug.Log("Defaulting to previous object of interest: " + CurrentlyLookingAtThis.name);
				return CurrentlyLookingAtThis;
			}

			// TO BE ADDED... Weighted Priorities
			// *** need to add weighted values of priority for relative interest
			// *** ie. AVATARS that are FRIENDS, or TALKING get higher priority; 
			// *** things that are CLOSER get priority over things FAR AWAY, etc;
			// *** May use an Avatar Class to hold relationship priority data
		}

		// When new objects enter frustum, briefly stop and look at the new object

		// triggered by frustum collision script as it adds new array values
		private bool checkLineOfSight(GameObject target)
		{
			Vector3 startPos = this.transform.position;
			Ray ray = new Ray(startPos, target.transform.position - startPos);
			RaycastHit hit = new RaycastHit();

			if (Physics.Raycast(ray, out hit, 100))
			{
				if (hit.transform == target.transform)
				{
				//	Debug.Log("Raycast SEEs: " + target.name);
					return true;
				}
				else
				{
				//	Debug.Log("Raycast CAN'T SEE: " + target.name + " Collided with: " + hit.collider.name);
					return false;
				}
			}
			else
			{
			//	Debug.Log("Raycast MISSED");
				return false;
			}

			
		}

		void TrimInactiveObjects()
		{
			ObjectsOfInterest.RemoveAll(elem => !elem.activeInHierarchy);
		}

#region COROUTINE HANDLERS

		#region FRUSTUM SCALER
		// Use raycast (to a limited distance) to dynamically adjust the frustum size so objects can 
		// enter the field of view to be added to the list of objects of interest.
		private IEnumerator scaleFrustum_Container;
		private IEnumerator ScaleFrustum()
		{
			// * LOW PRIORITY
			// Frustum size is scaled in relation to distance of ray cast from center of eyes
			// The idea being, the field for attention grabbing objects grows bigger as you look
			// out farther away.  So, when looking at things near by, your simulated attention 
			// is less distracted

			float distanceScale = CastRayFromEyes();

			PlayerFrustum.transform.localScale = new Vector3(FrustumScale.x * distanceScale, FrustumScale.y * distanceScale, FrustumScale.z * distanceScale);

			yield return new WaitForEndOfFrame();
		}

		void Start_ScaleFrustum()
		{
			scaleFrustum_Container = ScaleFrustum();
			StartCoroutine(scaleFrustum_Container);
		}

		void Stop_ScaleFrustum()
		{
			StopCoroutine(scaleFrustum_Container);
			scaleFrustum_Container = null;
		}
		#endregion

		#region CYCLE FOCUS
		// Cycle through the list of objects that are in the area of interest
		public void Start_CycleFocus()
		{
			Stop_CycleFocus();
			cycleFocus_Container = cycleFocus();
			StartCoroutine(cycleFocus_Container);
		}

		public void Stop_CycleFocus()
		{
			if (cycleFocus_Container != null)
			{
				StopCoroutine(cycleFocus_Container);
				cycleFocus_Container = null;
			}
		}

		private IEnumerator cycleFocus_Container;
		private IEnumerator cycleFocus()  // aka Saccade
		{
			if (ShowDebugLog) { Debug.Log("FocusOfInterest: Cycle Focus STARTED"); }

			while (true)
			{
				// check to see if anything is in the list
				if (ObjectsOfInterest.Count > 0)
				{
					// Pick an object from the list...
					GameObject arrayObject = pickObjectFromArray();

					// Check the TAGS of the chosen object...
					if (arrayObject.tag == "User")
					{
						// check the distance between this avatar and the avatar being looked at
						float dist = Vector3.Distance(arrayObject.transform.position, transform.position);

						// if they are close (within 10 units), look at their facial features...
						if (dist < FocusOnFaceDistance)
						{
							// get facial feature references...
							GameObject user_leftEye = arrayObject.transform.Find("Eyes").transform.Find("Eye_L").gameObject;
							GameObject user_rightEye = arrayObject.transform.Find("Eyes").transform.Find("Eye_R").gameObject;
							GameObject user_mouth = arrayObject.transform.Find("Mouth").gameObject;

							if (ShowDebugLog)
								Debug.Log(user_leftEye.name + ", " + user_rightEye + ", " + user_mouth);

							// Cycle through facial features with another coroutine...
							Start_FacialSaccade(user_leftEye, user_rightEye, user_mouth); //StartCoroutine(CycleThroughFaceObjects(user_leftEye, user_rightEye, user_mouth));

							// and DELAY this main coroutine for a decent amount of time (close faces are captivating)
							yield return new WaitForSeconds(UnityEngine.Random.Range(7.0f, 15.0f));
						}
						else
						{
							// else if the other avatar is farther away,
							// just look at center of eyes
							Stop_FacialSaccade(); // StopCoroutine("CycleThroughFaceObjects");
							CurrentlyLookingAtThis = arrayObject.transform.Find("Eyes").gameObject;
							yield return new WaitForSeconds(UnityEngine.Random.Range(5.0f, 10.0f));
						}
					}
					else
					if (arrayObject.tag == "Screen")
					{
						// if you're looking at a screen, 
						Stop_FacialSaccade(); // StopCoroutine("CycleThroughFaceObjects");

						// simply look at origin of object
						CurrentlyLookingAtThis = arrayObject;
						// TO BE ADDED... Mouse Cusor Gaze
						// CurrentlyLookingAtThis = mouseCursorObject;
					}
				}
				else
				{
					// else, You're NOT looking at anything
					InteruptCycles(null);
				}

				yield return new WaitForSeconds(UnityEngine.Random.Range(5.0f, 12.0f));
			}
		}
		#endregion

		#region FACIAL SACCADIC MOTION
		/* Saccade movements are very fast jumps from one eye 
		 * position to another. 
		 *
		 * Humans and many animals do not look at a scene in fixed steadiness; 
		 * instead, the eyes move around, locating interesting parts of the scene 
		 * and building up a mental, three-dimensional 'map' corresponding to the 
		 * scene. When scanning immediate surroundings or reading, human eyes make 
		 * jerky saccadic movements and stop several times, moving very quickly 
		 * between each stop. The speed of movement during each saccade cannot be 
		 * controlled; the eyes move as fast as they are able. One reason for the 
		 * saccadic movement of the human eye is that the central part of the retina
		 * — known as the fovea — which provides the high-resolution portion of vision 
		 * is very small in humans, only about 1–2 degrees of vision, but it plays a 
		 * critical role in resolving objects. By moving the eye so that small parts 
		 * of a scene can be sensed with greater resolution, body resources can be 
		 * used more efficiently. <Wikipedia>
		 *	 
		 */

		/* When looking at faces, the saccadic motion function cycles through the primary 
		 * facial landmarks: Left Eye, Right Eye, and Mouth. 
		 *
		 * This is a simple implementation, as a more accurate version would also move to the nose 
		 * and around the perimeter of the face. I compensate with an additional simulation in the 
		 * Microsaccade function which plays on top of this coroutine.
		 */

		private IEnumerator facialSaccade_Container;
		private IEnumerator facialSaccade(GameObject leftEye, GameObject rightEye, GameObject mouth)
		{
			while (true)
			{
				if (ShowDebugLog)
					Debug.Log("Control_FocusOfInterest: CycleThroughFaceObjects STARTED");

				// when looking at faces, change between each eye (~80%)
				// and the mouth (~20%) ...or immediately when avatar starts talking
				int randomNumber = UnityEngine.Random.Range(0, 100);

				if (randomNumber >= 0 && randomNumber < 40)
				{
					CurrentlyLookingAtThis = leftEye;
					yield return new WaitForSeconds(UnityEngine.Random.Range(3.0f, 8.0f));
				}
				else
				if (randomNumber >= 40 && randomNumber < 80)
				{
					CurrentlyLookingAtThis = rightEye;
					yield return new WaitForSeconds(UnityEngine.Random.Range(3.0f, 8.0f));
				}
				else
				{
					CurrentlyLookingAtThis = mouth;
					yield return new WaitForSeconds(UnityEngine.Random.Range(1.0f, 3.0f));
				}
			}
		}

		public void Start_FacialSaccade(GameObject leftEye, GameObject rightEye, GameObject mouth)
		{
			Stop_FacialSaccade();
			facialSaccade_Container = facialSaccade(leftEye, rightEye, mouth);
			StartCoroutine(facialSaccade_Container);
		}

		public void Stop_FacialSaccade()
		{
			if (facialSaccade_Container != null)
			{
				StopCoroutine(facialSaccade_Container);
				facialSaccade_Container = null;
			}
		}
		#endregion

		#region INTERUPT CYCLE
		// externally trigger a temporary interuption of the avatar's focus
		public void InteruptCycles(GameObject newObject)
		{
			if (newObject != null)
			{
				if ((newObject.tag == "User") || (newObject.tag == "Screen"))
				{
					// interupt the cycle by stopping them
					Stop_CycleFocus(); //StopCoroutine("CycleFocus");
					Stop_FacialSaccade(); //StopCoroutine("CycleThroughFaceObjects");

					// now look at the new object
					CurrentlyLookingAtThis = newObject;

					// and restart the basic cycle again
					Start_CycleFocus(); //StartCoroutine(CycleFocus());
				}
			}
		}
		#endregion

		#region MICROSACCADE
		/* Microsaccades are a kind of fixational eye movement. 
		 * They are small, jerk-like, involuntary eye movements, 
		 * similar to miniature versions of voluntary saccades. 
		 * They typically occur during prolonged visual fixation 
		 * (of at least several seconds), not only in humans, but 
		 * also in animals with foveal vision (primates, cats, etc.). 
		 * Microsaccade amplitudes vary from 2 arcminutes (0.0333 degrees) 
		 * to 120arcminutes (2 degrees). <Wikipedia>
		 */

		// *** NOTE: Microsaccade should probably activate for the duration of the overall focus length (>3sec)

		private IEnumerator microsaccade_Container;
		private IEnumerator microsaccade()
		{
			float minDistance = -0.1f; // 2 arcminutes or 0.03333 degrees
			float maxDistance = 0.1f; // 120 arcminutes or 2 degrees
			Vector3 originPosition;
			Vector3 targetPostion;
			Vector3 velocity = Vector3.zero;
			float smoothTime = 0.3f;

			while(true)
			{
				_holdFocus = Random.Range(3.0f, 6.0f);
				
				// if we're looking at a new object... reset microsaccade
				if (CurrentlyLookingAtThis != _lastObjectLookedAt)
				{
					if (ShowDebugLog)
						Debug.Log("Stopped Microsaccade");

					_isHoldingMicrosaccade = false;

					_lastObjectLookedAt = CurrentlyLookingAtThis;
					// zero out offset and duration values
					_holdMicrosaccade = 0.0f;
					_microsaccadeOffset = Vector3.zero;

					// wait (holdFocus)
					yield return new WaitForSeconds(_holdFocus);
				}
				else //it's still the same object, start microsaccade
				{
					if (CurrentlyLookingAtThis != null)
					{
						_isHoldingMicrosaccade = true;

						if(ShowDebugLog)
							Debug.Log("Holding Microsaccade");

						// create random offset distance and duration values
                        _microsaccadeOffset	= new Vector3(Random.Range(minDistance, maxDistance), Random.Range(minDistance, maxDistance), Random.Range(minDistance, maxDistance));
						originPosition		= CurrentlyLookingAtThis.transform.localPosition;
						targetPostion		= originPosition + _microsaccadeOffset;
						_holdMicrosaccade	= Random.Range(0.5f, 2.0f);

						// move to random position (microsaccadeOffset)
						// *** NOTE: I should add some tweening...
			//			FocusTarget.transform.localPosition = Vector3.Lerp(originPosition, targetPostion, FocusSpeed);
						// FocusTarget.transform.localPosition = Vector3.SmoothDamp(originPosition, targetPostion, ref velocity, smoothTime, 1.0f, Time.deltaTime);

						// wait for short duration (holdMicrosaccade)
						yield return new WaitForSeconds(_holdMicrosaccade);

						// choose to do another microsaccade or return...

						// then return to original position (or current object of interest position)
						_isHoldingMicrosaccade = false;
						if (ShowDebugLog)
							Debug.Log("Microsaccade Ended");

						// wait again (holdFocus)
						yield return new WaitForSeconds(_holdFocus);
					}
					else
					{
						yield return new WaitForEndOfFrame();
					}
				}
			}
		}

		public void Start_microsaccade()
		{
			microsaccade_Container = microsaccade();
			StartCoroutine(microsaccade_Container);
		}

		public void Stop_microsaccade()
		{
			if(microsaccade_Container != null)
			{
				StopCoroutine(microsaccade_Container);
				microsaccade_Container = null;
			}
		}

		private void offsetFocusPosition()
		{

		}
		#endregion

#endregion
	}
}