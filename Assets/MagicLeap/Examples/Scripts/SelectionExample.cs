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

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Experimental.XR.MagicLeap;

namespace MagicLeap
{
    public class SelectionExample : MonoBehaviour
    {
        public enum RaycastMode
        {
            Controller,
            Head,
            Eyes
        }

        #region Private Variables
        [SerializeField, Tooltip("Text to print current raycast mode on.")]
        private Text _statusLabel;

        [SerializeField, Tooltip("Visualizer for controller raycast result.")]
        private RaycastVisualizer _cursorController;

        [SerializeField, Tooltip("Visualizer for head raycast result.")]
        private RaycastVisualizer _cursorHead;

        [SerializeField, Tooltip("Visualizer for eyes raycast result.")]
        private RaycastVisualizer _cursorEyes;

        [SerializeField, Tooltip("ComboRaycast component for controller")]
        private ComboRaycastController _raycastController;

        [SerializeField, Tooltip("ComboRaycast component for head")]
        private ComboRaycastHead _raycastHead;

        [SerializeField, Tooltip("ComboRaycast component for eyes")]
        private ComboRaycastEyes _raycastEyes;

        private RaycastMode _raycastMode = RaycastMode.Controller;
        private int _modeCount = System.Enum.GetNames(typeof(RaycastMode)).Length;
        #endregion

        #region Unity Methods
        /// <summary>
        /// Validate all required components and sets event handlers.
        /// </summary>
        void Awake()
        {
            if (!MLInput.Start())
            {
                Debug.LogError("Error SelectionExample starting MLInput, disabling script.");
                enabled = false;
                return;
            }
            if (_statusLabel == null)
            {
                Debug.LogError("SelectionExample._statusLabel is not set, disabling script.");
                enabled = false;
                return;
            }
            if (_cursorController == null)
            {
                Debug.LogError("SelectionExample._cursorController is not set, disabling script.");
                enabled = false;
                return;
            }
            if (_cursorHead == null)
            {
                Debug.LogError("SelectionExample._cursorHead is not set, disabling script.");
                enabled = false;
                return;
            }
            if (_cursorEyes == null)
            {
                Debug.LogError("SelectionExample._cursorEyes is not set, disabling script.");
                enabled = false;
                return;
            }
            if (_raycastController == null)
            {
                Debug.LogError("SelectionExample._raycastController is not set, disabling script.");
                enabled = false;
                return;
            }
            if (_raycastHead == null)
            {
                Debug.LogError("SelectionExample._raycastHead is not set, disabling script.");
                enabled = false;
                return;
            }
            if (_raycastEyes == null)
            {
                Debug.LogError("SelectionExample._raycastEyes is not set, disabling script.");
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
            }
        }

        /// <summary>
        /// Updates Status Label with latest data.
        /// </summary>
        private void UpdateStatusText()
        {
            _statusLabel.text = string.Format("Raycast Mode: {0}", _raycastMode.ToString());
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
        #endregion
    }
}

