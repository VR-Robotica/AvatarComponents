using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// WIP - WORK IN POROGRESS v0.1
/// Calculates the potential Center Of Gravity (COG) for a full bodied
/// character by triangulating the center of the inputs: HMD, Left & Right
/// Hand Tracked Controllers, and weighting the influence to move the COG 
/// forwards, backwards or to the sides.
/// 
/// The intended use is to initiate a more accurate pose response from the
/// avatar, to determine when the user is leaning and how the avatar 
/// should counter weight its hips and/or curl its back, in an IK setup.
/// </summary>
namespace com.VR_Robotica.Avatar
{
	public class Avatar_CenterOfGravity : MonoBehaviour
	{
		public Transform HeadJoint;
		public Transform LeftHand;
		public Transform RightHand;
		public Transform LeftFoot;
		public Transform RightFoot;

		// how much will the weight of the arm shift the COG
		[Range(0.0f, 1.0f)]
		public float ArmMassInfluence;

		// Use this for initialization
		void Start()
		{

		}

		// Update is called once per frame
		void Update()
		{
			ArmMassInfluence = Mathf.Clamp(ArmMassInfluence, 0.0f, 1.0f);
		}

		private Vector3 LerpByDistance(Vector3 start, Vector3 end, float percent)
		{
			Vector3 P = percent * Vector3.Normalize(end - start) + start;
			return P;
		}

		private void OnDrawGizmos()
		{
			if(HeadJoint == null || LeftFoot == null || RightFoot == null)
			{
				return;
			}

			// Draw triangle connecting inputs
			Gizmos.color = Color.blue;
			Gizmos.DrawLine(LeftHand.position, HeadJoint.position);
			Gizmos.DrawLine(RightHand.position, HeadJoint.position);
			Gizmos.DrawLine(LeftHand.position, RightHand.position);

			// Center of 3 inputs
			Vector3 triangleCenter = (HeadJoint.position + LeftHand.position + RightHand.position) / 3;
			Gizmos.color = Color.blue;
			Gizmos.DrawWireCube(triangleCenter, new Vector3(0.1f,0.1f,0.1f));



			// Adjust plum line:
			// heading = target position - origin position
			Vector3 heading = triangleCenter - HeadJoint.position;
			// distance is also = to the magnitude of the vector we just created
			// *calculating square root functions like magnitude are CPU heavy
			float distance = heading.magnitude; 
			
			// now we normalize the direction
			// Vector3 direction = heading / distance;

			//Vector3 normalizedVectorBetween = Vector3.Normalize(triangleCenter - HeadJoint.position);
			//Vector3 adjustedCenter = HeadJoint.position + ( ArmMassInfluence * direction);
			Vector3 adjustedCenter = LerpByDistance(HeadJoint.position, triangleCenter, ArmMassInfluence);

			Gizmos.color = Color.green;
			Gizmos.DrawWireCube(new Vector3(adjustedCenter.x, triangleCenter.y, adjustedCenter.z), new Vector3(0.05f, 0.05f, 0.05f));

			// Draw plum line to ground
			Gizmos.color = Color.red;
			Gizmos.DrawLine(adjustedCenter, new Vector3(adjustedCenter.x, 0, adjustedCenter.z));

			// Center of Gravity is between Head-to-Toes
			float centerDistance = HeadJoint.position.y - LeftFoot.position.y;
			Gizmos.color = Color.green;
			Gizmos.DrawWireSphere(new Vector3(adjustedCenter.x, HeadJoint.position.y - centerDistance /2, adjustedCenter.z), 0.2f);
		}
	}
}