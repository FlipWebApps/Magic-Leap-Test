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
using UnityEngine.Experimental.XR.MagicLeap;

namespace MagicLeap
{
    /// <summary>
    /// This class handles GazeInteractable events based on input.
    /// </summary>
    public class GazeInteractionHandler : MonoBehaviour
    {
        #region Private Variables
        // Last object hit by raycast
        private Transform _lastHit;
        // Object trigger was pressed on
        private Transform _lastPressed;
        // Contains if trigger is pressed on object or not
        private bool _isPressed;

        private MLInputController _controller;
        #endregion

        #region Unity Methods
        /// <summary>
        /// Initializes variables.
        /// </summary>
        void OnEnable()
        {
            _lastHit = null;
            _lastPressed = null;
            _isPressed = false;

            if (!MLInput.Start())
            {
                Debug.LogError("Error GazeInteractableHandler starting MLInput, disabling script.");
                enabled = false;
                return;
            }

            _controller = MLInput.GetController(MLInput.Hand.Left);
        }

        /// <summary>
        /// Cleans up the component.
        /// </summary>
        void OnDisable()
        {
            MLInput.Stop();
        }
        #endregion

        #region Private Methods
        /// <summary>
        /// Handles raycast hit collision start on an object
        /// </summary>
        /// <param name="trans"> Transform starting interaction with </param>
        private void InteractionBegin(Transform trans)
        {
            GazeInteractable interactable = trans.GetComponent<GazeInteractable>();

            if (interactable == null)
            {
                return;
            }

            interactable.GazeBegin();
        }

        /// <summary>
        /// Handles raycast hit collision end on an object
        /// </summary>
        /// <param name="trans"> Transform finishing interaction with </param>
        private void InteractionEnd(Transform trans)
        {
            GazeInteractable interactable = trans.GetComponent<GazeInteractable>();

            if (interactable == null)
            {
                return;
            }

            interactable.GazeEnd();
        }

        /// <summary>
        /// Handles controller trigger press behavior on an object
        /// </summary>
        /// <param name="trans"> Transform being pressed </param>
        private void InteractionPress(Transform trans)
        {
            GazeInteractable interactable = trans.GetComponent<GazeInteractable>();

            if (interactable == null)
            {
                return;
            }

            interactable.Press();
        }

        /// <summary>
        /// Handles controller trigger release behavior on an object
        /// </summary>
        /// <param name="trans"> Transform being released </param>
        private void InteractionRelease(Transform trans)
        {
            GazeInteractable interactable = trans.GetComponent<GazeInteractable>();

            if (interactable == null)
            {
                return;
            }

            interactable.Release();
        }

        /// <summary>
        /// Handles controller trigger click behavior on an object
        /// </summary>
        /// <param name="trans"> Transform being clicked </param>
        private void InteractionClick(Transform trans)
        {
            GazeInteractable interactable = trans.GetComponent<GazeInteractable>();

            if (interactable == null)
            {
                return;
            }

            interactable.Click();
        }
        #endregion

        #region Event Handlers
        /// <summary>
        /// Aplies the correct object interactions based on the raycast results.
        /// </summary>
        /// <param name="result"> Contains the info on the result of the raycast</param>
        public void OnRaycastHit(RaycastHit result, float confidence)
        {
            // Detect if raycast hit same object, new object or none
            if (_lastHit != result.transform || _lastHit == null)
            {
                if (_lastHit != null)
                {
                    if (_isPressed)
                    {
                        InteractionRelease(_lastPressed);
                        _isPressed = false;
                        _lastPressed = null;
                    }

                    InteractionEnd(_lastHit);
                }

                _lastHit = result.transform;

                if (_lastHit != null)
                {
                    InteractionBegin(_lastHit);
                }
                else
                {
                    return;
                }
            }

            // Detect the user's press input.
            if (_controller.TriggerValue > 0.2f && !_isPressed)
            {
                InteractionPress(_lastHit);

                _isPressed = true;
                _lastPressed = _lastHit;
            }

            // Detect the user's release input.
            else if (_controller.TriggerValue <= 0.2f && _isPressed)
            {
                InteractionRelease(_lastPressed);

                // Ensure the object was previously pressed and call the click method.
                if (_lastPressed == _lastHit)
                {
                    InteractionClick(_lastHit);
                }

                _isPressed = false;
                _lastPressed = null;
            }
        }
        #endregion
    }
}
