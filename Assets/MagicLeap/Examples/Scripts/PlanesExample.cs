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

using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.Experimental.XR.MagicLeap;

namespace MagicLeap
{
    /// <summary>
    /// This class handles the functionality of updating the bounding box
    /// for the planes query params through input. This class also updates
    /// the UI text containing the latest useful info on the planes queries.
    /// </summary>
    [RequireComponent(typeof(Planes))]
    public class PlanesExample : MonoBehaviour
    {
        #region Private Variables
        [SerializeField, Tooltip("Initial cube size for plane quering.")]
        private Vector3 _boundsExtents = new Vector3(0, 0, 0);

        [Space, SerializeField, Tooltip("Cursor to use for changing bounds location.")]
        private GameObject _cursorHead;

        [Space, SerializeField, Tooltip("Text to display planes info on.")]
        private Text _statusText;

        private readonly Vector3 _scaleIncrement = new Vector3(0.1f, 0.1f, 0.1f);
        private const float MAX_BOUNDS_SIZE = 10.0f;

        private MLInputController _controller;

        private Planes _planesComponent;

        #endregion

        #region Unity Methods
        /// <summary>
        /// Check editor set variables for null references.
        /// </summary>
        void Awake()
        {
            if (_statusText == null)
            {
                Debug.LogError("Error PlanesExample._statusText is not set, disabling script.");
                enabled = false;
                return;
            }

            transform.localScale = _boundsExtents;
        }

        /// <summary>
        /// Initializes variables.
        /// </summary>
        void Start()
        {
            if (!MLInput.Start())
            {
                Debug.LogError("Error PlanesExample starting MLInput, disabling script.");
                enabled = false;
                return;
            }
            _controller = MLInput.GetController(MLInput.Hand.Left);

            _planesComponent = GetComponent<Planes>();
        }

        /// <summary>
        /// Update transform based on input.
        /// </summary>
        void Update()
        {
            if (_controller.TouchpadGesture.Type == MLInputControllerTouchpadGestureType.RadialScroll && _controller.TouchpadGestureState != MLInputControllerTouchpadGestureState.End)
            {
                transform.rotation = Quaternion.AngleAxis(_controller.TouchpadGesture.Angle * Mathf.Rad2Deg, transform.up);
            }
            else if (_controller.TouchpadGesture.Type == MLInputControllerTouchpadGestureType.Swipe && _controller.TouchpadGestureState != MLInputControllerTouchpadGestureState.End)
            {
                if (_controller.TouchpadGesture.Direction == MLInputControllerTouchpadGestureDirection.Up)
                {
                    transform.localScale += _scaleIncrement;
                }
                else if (_controller.TouchpadGesture.Direction == MLInputControllerTouchpadGestureDirection.Down)
                {
                    transform.localScale -= _scaleIncrement;
                }
                else
                {
                    return;
                }

                transform.localScale = new Vector3(Mathf.Clamp(transform.localScale.x, 0.0f, MAX_BOUNDS_SIZE),
                                                   Mathf.Clamp(transform.localScale.y, 0.0f, MAX_BOUNDS_SIZE),
                                                   Mathf.Clamp(transform.localScale.z, 0.0f, MAX_BOUNDS_SIZE));
            }
            else if (_controller.TriggerValue > 0.8f && _controller.State.ButtonState[(int)MLInputControllerButton.Bumper] != 0)
            {
                transform.position = _cursorHead.transform.position;
            }
        }

        /// <summary>
        /// Stop the input API.
        /// </summary>
        void OnDestroy()
        {
            MLInput.Stop();
        }
        #endregion

        #region Event Handlers
        /// <summary>
        /// Callback handler, changes text when new planes are received.
        /// </summary>
        /// <param name="planes"> Array of new planes. </param>
        public void OnPlanesUpdate(MLWorldPlane[] planes)
        {
            _statusText.text = string.Format("Number of Planes = {0}/{1}", planes.Length, _planesComponent.MaxPlaneCount);
        }
        #endregion
    }
}
