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
// %BANNER_END%

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.Experimental.XR.MagicLeap;

namespace MagicLeap
{
    /// <summary>
    /// Component used to hook into the HandGestures script and attach
    /// primitive game objects to it's detected keypoint positions for
    /// each hand.
    /// </summary>
    [RequireComponent(typeof(Gestures))]
    public class GesturesKeypointVisualizer : MonoBehaviour
    {
        #region Private Variables
        [SerializeField, Tooltip("Type of primitive for the gesture keypoints objects.")]
        private PrimitiveType _keypointType;

        [SerializeField, Tooltip("Scale for the gesture keypoints objects.")]
        private Vector3 _keypointScale = new Vector3(0.02f, 0.02f, 0.02f);

        [SerializeField, Tooltip("Material to use on the gesture keypoint objects.")]
        private Material _keypointMaterial;

        // KeyPoints
        // Key points for current gestures for each hand
        private Transform[] _leftHandKeyPoints;
        private Transform[] _rightHandKeyPoints;

        // Maximum number of key points for each gesture
        private const uint _maxKeypoints = 3;
        #endregion

        #region Unity Methods
        /// <summary>
        /// Initializes MLHands API.
        /// </summary>
        void OnEnable()
        {
            if (!MLHands.Start())
            {
                Debug.LogError("Error GesturesKeypointVisualizer starting MLHands, disabling script.");
                enabled = false;
                return;
            }
        }

        /// <summary>
        /// Stops the communication to the MLHands API and unregisters required events.
        /// </summary>
        void OnDisable()
        {
            MLHands.Stop();
        }

        /// <summary>
        /// Initializes and finds references to all relevant components in the
        /// scene and creates the GameObject pool to be used as the keypoint
        /// visuals.
        /// </summary>
        void Awake()
        {
            _leftHandKeyPoints = new Transform[_maxKeypoints];
            _rightHandKeyPoints = new Transform[_maxKeypoints];
            for (int i = 0; i < _maxKeypoints; ++i)
            {
                // Left hand
                _leftHandKeyPoints[i] = CreateObject(string.Format("HandGestures_Keypoint(L{0})", i)).transform;

                // Right hand
                _rightHandKeyPoints[i] = CreateObject(string.Format("HandGestures_Keypoint(R{0})", i)).transform;
            }
        }

        /// <summary>
        /// Polls the Gestures API each frame and gets relevant information about
        /// the currently tracked gesture's keypoints.
        /// </summary>
        void Update()
        {
            if (MLHands.IsStarted)
            {
                if (MLHands.Left.StaticGesture != MLStaticGestureType.NoHand && MLHands.Left.KeyPoints.Length > 0)
                {
                    System.Array.ForEach(_leftHandKeyPoints, (x) => x.gameObject.SetActive(true));
                    UpdateKeypointObjects(_leftHandKeyPoints, MLHands.Left);
                }
                else
                {
                    System.Array.ForEach(_leftHandKeyPoints, (x) => x.gameObject.SetActive(false));
                }

                if (MLHands.Right.StaticGesture != MLStaticGestureType.NoHand && MLHands.Right.KeyPoints.Length > 0)
                {
                    System.Array.ForEach(_rightHandKeyPoints, (x) => x.gameObject.SetActive(true));
                    UpdateKeypointObjects(_rightHandKeyPoints, MLHands.Right);
                }
                else
                {
                    System.Array.ForEach(_rightHandKeyPoints, (x) => x.gameObject.SetActive(false));
                }
            }
        }
        #endregion

        #region Private Methods
        /// <summary>
        /// Update the positions of the keypoints to the latest data from the
        /// ML device.
        /// </summary>
        /// <param name="keypoints">The array of transforms to set.</param>
        /// <param name="hand">The hand to poll for the keypoint information.</param>
        private void UpdateKeypointObjects(Transform[] keypoints, MLHand hand)
        {
            keypoints[0].position = hand.KeyPoints[0];
            keypoints[1].position = hand.KeyPoints[1];
            keypoints[2].position = hand.Center;
        }

        /// <summary>
        /// Creates object based on script type... input and specified color and name
        /// </summary>
        /// <param name="name"> Name for new object </param>
        /// <returns> The new GameObject </returns>
        private GameObject CreateObject(string name)
        {
            GameObject newObject = GameObject.CreatePrimitive(_keypointType);
            newObject.transform.localScale = _keypointScale;
            newObject.name = name;
            newObject.GetComponent<MeshRenderer>().material = _keypointMaterial;
            newObject.SetActive(false);

            return newObject;
        }
        #endregion
    }
}
