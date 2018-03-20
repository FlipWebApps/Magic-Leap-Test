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
    /// Component used to track a single gesture and display it's confidence
    /// value. It can be configured to track one or both hands.
    /// </summary>
    [RequireComponent(typeof(Gestures))]
    public class GesturesExample : MonoBehaviour
    {
        #region Public Variables
        [SerializeField, Tooltip("Trackers for this example to track.")]
        private GestureTracker[] _gesturesTrackers;
        #endregion

        #region Private Variables
        // Color for confidence values
        private Color _confidenceColor;
        #endregion

        #region Unity Methods
        /// <summary>
        /// Initializes MLHands API.
        /// </summary>
        void OnEnable()
        {
            if (!MLHands.Start())
            {
                Debug.LogError("Error GesturesExample starting MLHands, disabling script.");
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
        ///  Polls the Gestures API for up to date confidence values.
        /// </summary>
        void Update()
        {
            foreach (GestureTracker gesture in _gesturesTrackers)
            {
                float confidenceLeft = gesture.TrackLeft ? GetGestureConfidence(MLHands.Left, gesture.TrackedGesture) : 0.0f;
                float confidenceRight = gesture.TrackRight ? GetGestureConfidence(MLHands.Right, gesture.TrackedGesture) : 0.0f;

                // Calc and set the color value
                float confidenceValue = Mathf.Max(confidenceLeft, confidenceRight);
                _confidenceColor.r = 1 - confidenceValue;
                _confidenceColor.g = 1;
                _confidenceColor.b = 1 - confidenceValue;
                _confidenceColor.a = 1.0f;

                SpriteRenderer sprite = gesture.gameObject.GetComponent<SpriteRenderer>();
                if(sprite == null)
                {
                    Debug.Log("Gesture Tracker object in component GestureVisualizer missing SpriteRenderer");
                    return;
                }

                sprite.material.color = _confidenceColor;
            }


        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Get the device confidence value for the passed in gesture and hand.
        /// </summary>
        /// <param name="type">The specific gesture to check.</param>
        public float GetGestureConfidence(MLHand hand, MLStaticGestureType type)
        {
            if (hand != null)
            {
                if (hand.StaticGesture == type)
                {
                    return hand.GestureConfidence;
                }
            }
            return 0.0f;
        }
        #endregion
    }
}
