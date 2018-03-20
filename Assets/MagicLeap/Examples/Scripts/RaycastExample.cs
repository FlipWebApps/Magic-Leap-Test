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

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Experimental.XR.MagicLeap;

namespace MagicLeap
{
    /// <summary>
    /// This example demonstrates using the magic leap raycast functionality to calculate intersection with the physical space.
    /// It demonstrates casting rays from the users headpose, controller, and eyes position and orientation.
    ///
    /// This example uses several raycast visualizers which represent this intersection with the physical space.
    /// </summary>
    public class RaycastExample : MonoBehaviour
    {
        public enum RaycastMode
        {
            Controller,
            Head,
            Eyes,
            All
        }

        #region Private Variables
        [SerializeField, Tooltip("The headpose canvas for example status text.")]
        private Text _statusLabel;

        [SerializeField, Tooltip("Visualizer for controller raycast result.")]
        private RaycastVisualizer _cursorController;

        [SerializeField, Tooltip("Visualizer for headpose raycast result.")]
        private RaycastVisualizer _cursorHead;

        [SerializeField, Tooltip("Visualizer for eyegaze raycast result.")]
        private RaycastVisualizer _cursorEyes;

        [SerializeField, Tooltip("Raycast from controller.")]
        private WorldRaycastController _raycastController;

        [SerializeField, Tooltip("Raycast from headpose.")]
        private WorldRaycastHead _raycastHead;

        [SerializeField, Tooltip("Raycast from eyegaze.")]
        private WorldRaycastEyes _raycastEyes;

        private RaycastMode _raycastMode = RaycastMode.Controller;
        private int _modeCount = System.Enum.GetNames(typeof(RaycastMode)).Length;

        private float _confidence = 0.0f;
        #endregion

        #region Unity Methods
        /// <summary>
        /// Validate all required components and sets event handlers.
        /// </summary>
        void Awake()
        {
            if (!MLInput.Start())
            {
                Debug.LogError("Error RaycastExample starting MLInput, disabling script.");
                enabled = false;
                return;
            }
            if (_statusLabel == null)
            {
                Debug.LogError("Error RaycastExample._statusLabel is not set, disabling script.");
                enabled = false;
                return;
            }
            if (_cursorController == null)
            {
                Debug.LogError("Error RaycastExample._cursorController is not set, disabling script.");
                enabled = false;
                return;
            }
            if (_cursorHead == null)
            {
                Debug.LogError("Error RaycastExample._cursorHead is not set, disabling script.");
                enabled = false;
                return;
            }
            if (_cursorEyes == null)
            {
                Debug.LogError("Error RaycastExample._cursorEyes is not set, disabling script.");
                enabled = false;
                return;
            }
            if (_raycastController == null)
            {
                Debug.LogError("Error RaycastExample._raycastController is not set, disabling script.");
                enabled = false;
                return;
            }
            if (_raycastHead == null)
            {
                Debug.LogError("Error RaycastExample._raycastHead is not set, disabling script.");
                enabled = false;
                return;
            }
            if (_raycastEyes == null)
            {
                Debug.LogError("Error RaycastExample._raycastEyes is not set, disabling script.");
                enabled = false;
                return;
            }

            MLInput.OnControllerButtonDown += OnButtonDown;
            UpdateRaycastMode();
        }

        /// <summary>
        /// Cleans up the component.
        /// </summary>
        void OnDestroy()
        {
            MLInput.OnControllerButtonDown -= OnButtonDown;
            MLInput.Stop();
        }
        #endregion

        #region Private Methods
        /// <summary>
        /// Updates type of raycast and enables correct cursor.
        /// </summary>
        private void UpdateRaycastMode()
        {
            // Default all objects to inactive and then set active to the appropriate ones.
            _cursorController.gameObject.SetActive(false);
            _cursorHead.gameObject.SetActive(false);
            _cursorEyes.gameObject.SetActive(false);
            _raycastController.enabled = false;
            _raycastHead.enabled = false;
            _raycastEyes.enabled = false;
            _raycastController.CalibratedController.gameObject.SetActive(false);

            switch (_raycastMode)
            {
                case RaycastMode.Controller:
                {
                    _cursorController.gameObject.SetActive(true);
                    _raycastController.enabled = true;
                    _raycastController.CalibratedController.gameObject.SetActive(true);
                    break;
                }
                case RaycastMode.Head:
                {
                    _cursorHead.gameObject.SetActive(true);
                    _raycastHead.enabled = true;
                    break;
                }
                case RaycastMode.Eyes:
                {
                    _cursorEyes.gameObject.SetActive(true);
                    _raycastEyes.enabled = true;
                    break;
                }
                case RaycastMode.All:
                {
                    _cursorController.gameObject.SetActive(true);
                    _cursorHead.gameObject.SetActive(true);
                    _cursorEyes.gameObject.SetActive(true);
                    _raycastController.enabled = true;
                    _raycastHead.enabled = true;
                    _raycastEyes.enabled = true;
                    _raycastController.CalibratedController.gameObject.SetActive(true);
                    break;
                }
            }
        }

        /// <summary>
        /// Updates Status Label with latest data.
        /// </summary>
        private void UpdateStatusText()
        {
            _statusLabel.text = string.Format("Raycast Mode: {0}\nRaycast Hit Confidence: {1}", _raycastMode.ToString(), (_raycastMode == RaycastMode.All) ? "N/A" : _confidence.ToString());
        }
        #endregion

        #region Event Handlers
        /// <summary>
        /// Handles the event for button down and cycles the raycast mode.
        /// </summary>
        /// <param name="controller_id">The id of the controller.</param>
        /// <param name="button">The button that is being pressed.</param>
        private void OnButtonDown(byte controller_id, MLInputControllerButton button)
        {
            if (button == MLInputControllerButton.Bumper)
            {
                _raycastMode = (RaycastMode)((int)(_raycastMode + 1) % _modeCount);
                UpdateRaycastMode();
                UpdateStatusText();
            }
        }

        /// <summary>
        /// Callback handler called when raycast has a result.
        /// Updates the confidence value to the new confidence value.
        /// </summary>
        /// <param name="result">Class containing hit point, normal...</param>
        /// <param name="confidence">Confidence value of hit. 0 no hit, 1 sure hit.</param>
        public void OnRaycastHit(RaycastHit result, float confidence)
        {
            _confidence = confidence;
            UpdateStatusText();
        }
        #endregion
    }
}
