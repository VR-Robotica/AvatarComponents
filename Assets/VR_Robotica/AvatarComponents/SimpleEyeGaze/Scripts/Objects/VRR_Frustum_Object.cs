using System.Collections;
using UnityEngine;
using com.VR_Robotica.AvatarComponents.Controllers;

/// <summary>
/// This script simply adds and removes potential ObjectsOfInterest from the Controller_Interest Script
/// when objects enter or exit the collision area of the Frustum Collider Geometry
/// </summary>

namespace com.VR_Robotica.AvatarComponents.Objects
{
    public class VRR_Frustum_Object : MonoBehaviour
    {
        [Tooltip("You can place script here, or it will be discovered if left null")]
        // create reference to Control Script to change list values
        public VRR_Interest_Controller InterestController;
        [Space]
        public bool ShowDebugLog;

        [Tooltip("Width, Height, Focus Distance")]
        public Vector3 FrustumSize = new Vector3(1.0f, 0.5f, 2.5f);
        public Vector3 FrustumOffSet;
        [HideInInspector]
        public Vector3 FrustumScale = new Vector3(1, 1, 1);//(150, 100, 200);

        private bool _isReady;

       

        #region CREATE FRUSTUM
        public IEnumerator Create()
        {
            if (!_isReady)
            {
                createFrustum(FrustumSize.x, FrustumSize.y, FrustumSize.z);
                setupFrustum();
                getReferences();
                _isReady = true;
            }

            yield return null;
        }

        private void createFrustum(float width, float height, float distance)
        {
            MeshCollider collider = this.gameObject.AddComponent<MeshCollider>();
            collider.convex = true;
            collider.isTrigger = true;

            Mesh colliderMesh;
            colliderMesh = new Mesh();
            colliderMesh.name = "Frustum Mesh";

            Vector3[] frustumOriginPlane = new Vector3[4];
            frustumOriginPlane[0] = new Vector3(-width * 0.1f, height * 0.1f, 0);
            frustumOriginPlane[1] = new Vector3(width * 0.1f, height * 0.1f, 0);
            frustumOriginPlane[2] = new Vector3(-width * 0.1f, -height * 0.1f, 0);
            frustumOriginPlane[3] = new Vector3(width * 0.1f, -height * 0.1f, 0);

            Vector3[] frustumDistantPlane = new Vector3[4];
            frustumDistantPlane[0] = new Vector3(-width, height, distance);
            frustumDistantPlane[1] = new Vector3(width, height, distance);
            frustumDistantPlane[2] = new Vector3(-width, -height, distance);
            frustumDistantPlane[3] = new Vector3(width, -height, distance);

            colliderMesh.vertices = new Vector3[]
            {	
				// bottom plane
				frustumDistantPlane[2], frustumDistantPlane[3], frustumOriginPlane[3], frustumOriginPlane[2],
				// left plane
				frustumOriginPlane[0], frustumDistantPlane[0], frustumDistantPlane[2], frustumOriginPlane[2],
				// front  plane - Distant
				frustumDistantPlane[0], frustumDistantPlane[1], frustumDistantPlane[3], frustumDistantPlane[2],
				// back  plane - Origin
				frustumOriginPlane[1], frustumOriginPlane[0], frustumOriginPlane[2], frustumOriginPlane[3],
				// right plane
				frustumDistantPlane[1], frustumOriginPlane[1], frustumOriginPlane[3], frustumDistantPlane[3],
				// top plane
				frustumOriginPlane[0], frustumOriginPlane[1], frustumDistantPlane[1], frustumDistantPlane[0]
            };

            colliderMesh.triangles = new int[]
            {
				// Bottom
				3, 1, 0,
                3, 2, 1,			
 
				// Left
				3 + 4 * 1, 1 + 4 * 1, 0 + 4 * 1,
                3 + 4 * 1, 2 + 4 * 1, 1 + 4 * 1,
 
				// Front
				3 + 4 * 2, 1 + 4 * 2, 0 + 4 * 2,
                3 + 4 * 2, 2 + 4 * 2, 1 + 4 * 2,
 
				// Back
				3 + 4 * 3, 1 + 4 * 3, 0 + 4 * 3,
                3 + 4 * 3, 2 + 4 * 3, 1 + 4 * 3,
 
				// Right
				3 + 4 * 4, 1 + 4 * 4, 0 + 4 * 4,
                3 + 4 * 4, 2 + 4 * 4, 1 + 4 * 4,
 
				// Top
				3 + 4 * 5, 1 + 4 * 5, 0 + 4 * 5,
                3 + 4 * 5, 2 + 4 * 5, 1 + 4 * 5,
            };

            collider.sharedMesh = colliderMesh;
        }

        private void setupFrustum()
        {
            this.gameObject.name = "Frustum";
            // Set to Layer[2] = Ignore Ray Cast
            this.gameObject.layer = 2;

            // Align and Scale Frustum
            this.transform.localEulerAngles = Vector3.zero;
            this.transform.localScale = FrustumScale;
            this.transform.localPosition = FrustumOffSet;
            Debug.Log("Frustum Offset: " + FrustumOffSet);

            this.gameObject.AddComponent<Rigidbody>();
            this.gameObject.GetComponent<Rigidbody>().useGravity = false;
            this.gameObject.GetComponent<Rigidbody>().mass = 0.0f;

            // Add collision triggering script
            VRR_Frustum_Object of = this.gameObject.GetComponent<VRR_Frustum_Object>();
            if (of == null)
            {
                Debug.Log("Adding Frustum Controller");
                of = this.gameObject.AddComponent<VRR_Frustum_Object>();
            }

            // setting reference
            of.InterestController = this.gameObject.GetComponent<VRR_Interest_Controller>();
        }
        #endregion

        private void getReferences()
        {
            if (InterestController == null)
            {
                if (ShowDebugLog) { Debug.Log("Frustum: interest Controller Not Set, searching Parent Object for component"); }

                InterestController = transform.parent.gameObject.GetComponent<VRR_Interest_Controller>();

                if (InterestController == null)
                {
                    if (ShowDebugLog) { Debug.Log("Frustum: interest Controller Not Set, searching Grand Parent Object for component"); }

                    InterestController = transform.parent.parent.gameObject.GetComponent<VRR_Interest_Controller>();

                    if (InterestController == null)
                    {
                        Debug.LogWarning("Controller_Interest Component Not Found in Parent or GrandParent Object!");
                        return;
                    }
                }
            }
        }

        void OnTriggerEnter(Collider col)
        {
            if (_isReady)
            {
                if (col.gameObject != InterestController.gameObject)
                {
                    VRR_ObjectOfInterest ooi = col.gameObject.GetComponent<VRR_ObjectOfInterest>();

                    if (ooi != null)
                    {
                        //Debug.Log(col.name + " is an object of interest.");
                        // add game object to primary list
                        InterestController.ObjectsOfInterest.Add(col.gameObject);
                        // interupt cycle
                        InterestController.InteruptCycle(col.gameObject);
                    }
                }
            }
        }

        /// <summary>
        /// Remove Exited Object from the Objects Of Interest List in the Controller_Interest script
        /// If the exited object was being looked at, refresh the focus (clear points of interest and
        /// the currently looked at objects)
        /// </summary>
        /// <param name="col"></param>
        void OnTriggerExit(Collider col)
        {
            if (_isReady)
            {

                VRR_ObjectOfInterest ooi = col.gameObject.GetComponent<VRR_ObjectOfInterest>();

                if (ooi != null)
                {
                    // IF the object exiting is the CURRENT OBJECT being looked at
                    // clear everything...
                    if (ooi.gameObject == InterestController.CurrentObject)
                    {
                        InterestController.CurrentlyLookingAt = null;
                        InterestController.CurrentObject = null;
                    }
                    // remove object from list
                    InterestController.ObjectsOfInterest.Remove(col.gameObject);
                    InterestController.ChangeObjectOfFocus();
                }
            }
        }
    }
}