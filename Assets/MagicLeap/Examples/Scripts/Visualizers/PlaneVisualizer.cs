// %BANNER_BEGIN%
// ---------------------------------------------------------------------
// %COPYRIGHT_BEGIN%
//
// Copyright (c) 2017 Magic Leap, Inc. (COMPANY) All Rights Reserved.
// Magic Leap, Inc. Confidential and Proprietary
//
//  NOTICE:  All information contained herein is, and remains the property
//  of COMPANY. The intellectual and technical concepts contained herein
//  are proprietary to COMPANY and may be covered by U.S. and Foreign
//  Patents, patents in process, and are protected by trade secret or
//  copyright law.  Dissemination of this information or reproduction of
//  this material is strictly forbidden unless prior written permission is
//  obtained from COMPANY.  Access to the source code contained herein is
//  hereby forbidden to anyone except current COMPANY employees, managers
//  or contractors who have executed Confidentiality and Non-disclosure
//  agreements explicitly covering such access.
//
//  The copyright notice above does not evidence any actual or intended
//  publication or disclosure  of  this source code, which includes
//  information that is confidential and/or proprietary, and is a trade
//  secret, of  COMPANY.   ANY REPRODUCTION, MODIFICATION, DISTRIBUTION,
//  PUBLIC  PERFORMANCE, OR PUBLIC DISPLAY OF OR THROUGH USE  OF THIS
//  SOURCE CODE  WITHOUT THE EXPRESS WRITTEN CONSENT OF COMPANY IS
//  STRICTLY PROHIBITED, AND IN VIOLATION OF APPLICABLE LAWS AND
//  INTERNATIONAL TREATIES.  THE RECEIPT OR POSSESSION OF  THIS SOURCE
//  CODE AND/OR RELATED INFORMATION DOES NOT CONVEY OR IMPLY ANY RIGHTS
//  TO REPRODUCE, DISCLOSE OR DISTRIBUTE ITS CONTENTS, OR TO MANUFACTURE,
//  USE, OR SELL ANYTHING THAT IT  MAY DESCRIBE, IN WHOLE OR IN PART.
//
// %COPYRIGHT_END%
// --------------------------------------------------------------------*/

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.Experimental.XR.MagicLeap;

namespace MagicLeap
{
    /// <summary>
    /// Manages plane rendering based on plane detection from Planes component.
    /// </summary>
    [RequireComponent(typeof(Planes))]
    public class PlaneVisualizer : MonoBehaviour
    {
        #region Public Variables
        [Tooltip("Object prefab to use for plane visualization.")]
        public GameObject PlaneVisualPrefab;

        [Header("Materials")]
        [Tooltip("Material used for wall planes.")]
        public Material WallMaterial;
        [Tooltip("Material used for floor planes.")]
        public Material FloorMaterial;
        [Tooltip("Material used for ceiling planes.")]
        public Material CeilingMaterial;
        [Tooltip("Material used for other types of planes.")]
        public Material DefaultMaterial;
        #endregion

        #region Private Members
        // List of all the planes being rendered
        private List<GameObject> _planeCache;
        #endregion

        #region Unity Methods
        /// <summary>
        /// Initializes all variables and makes sure needed components exist
        /// </summary>
        void Awake()
        {
            if (PlaneVisualPrefab == null)
            {
                Debug.LogError("Error PlanesVisualizer.PlaneVisualPrefab is not set, disabling script.");
                enabled = false;
                return;
            }

            if (WallMaterial == null || FloorMaterial == null || CeilingMaterial == null || DefaultMaterial == null)
            {
                Debug.LogError("Error PlanesVisualizer.Materials is not set, disabling script.");
                enabled = false;
                return;
            }

            MeshRenderer planeRenderer = PlaneVisualPrefab.GetComponent<MeshRenderer>();
            if (planeRenderer == null)
            {
                Debug.LogError("Error PlanesVisualizer MeshRenderer component not found, disabling script.");
                enabled = false;
                return;
            }

            _planeCache = new List<GameObject>();
        }

        /// <summary>
        /// Destroys all planes instances created
        /// </summary>
        void OnDestroy()
        {
            _planeCache.ForEach((GameObject go) => GameObject.Destroy(go));
            _planeCache.Clear();
        }
        #endregion

        #region Public Functions
        /// <summary>
        /// Updates planes and creates new planes based on detected planes.
        ///
        /// This function reuses previously allocated memory to convert all planes
        /// to the new ones by changing their transforms, it allocates new objects
        /// if the current result ammount is bigger than the ones already stored.
        /// </summary>
        /// <param name="p">The planes component</param>
        public void OnPlanesUpdate(MLWorldPlane[] planes)
        {
            int index = planes.Length > 0 ? planes.Length - 1 : 0;
            for (int i = index; i < _planeCache.Count; ++i)
            {
                _planeCache[i].SetActive(false);
            }

            for (int i = 0; i < planes.Length; ++i)
            {
                GameObject planeVisual;
                if (i < _planeCache.Count)
                {
                    planeVisual = _planeCache[i];
                    planeVisual.SetActive(true);
                }
                else
                {
                    planeVisual = Instantiate(PlaneVisualPrefab);
                    _planeCache.Add(planeVisual);
                }

                planeVisual.transform.position = planes[i].Center;
                planeVisual.transform.rotation = planes[i].Rotation;
                planeVisual.transform.localScale = new Vector3(planes[i].Width, planes[i].Height, 1f);

                Renderer planeRenderer = planeVisual.GetComponent<Renderer>();
                SetRenderTexture(planeRenderer, planes[i].Flags);

                float xScale = planeVisual.transform.localScale.x;
                float yScale = planeVisual.transform.localScale.y;

                float xOffset = xScale - Mathf.Floor(xScale);
                float yOffset = yScale - Mathf.Floor(yScale);

                // Apply tiling and offsets to the texture
                planeRenderer.material.SetTextureScale("_MainTex", new Vector2(xScale, yScale));
                planeRenderer.material.SetTextureOffset("_MainTex", new Vector2(-xOffset * 0.5f, -yOffset * 0.5f));
            }
        }
        #endregion

        #region Private Functions
        /// <summary>
        /// Sets correct texture to plane based on surface type
        /// </summary>
        /// <param name="renderer">The renderer component</param>
        /// <param name="flags">The flags of the plane containing the surface type</param>
        private void SetRenderTexture(Renderer renderer, uint flags)
        {
            //Set Renderer texture to proper visual
            if ((flags & (uint)SemanticFlags.Wall) != 0)
            {
                renderer.material = WallMaterial;
            }
            else if ((flags & (uint)SemanticFlags.Floor) != 0)
            {
                renderer.material = FloorMaterial;
            }
            else if ((flags & (uint)SemanticFlags.Ceiling) != 0)
            {
                renderer.material = CeilingMaterial;
            }
            else
            {
                renderer.material = DefaultMaterial;
            }
        }
        #endregion
    }
}
