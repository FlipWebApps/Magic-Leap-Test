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

namespace MagicLeap
{
    /// <summary>
    /// Adds a visual effect to a GazeInteractable component on all actionable events.
    /// </summary>
    [RequireComponent(typeof(GazeInteractable))]
    public class GazeInteractableDemo : MonoBehaviour
    {
        #region Private Variables
        [SerializeField, Tooltip("This is the primitive that should be scaled for the selection effect.")]
        private Primitive _primitive;

        private GazeInteractable _interactable;
        #endregion

        #region Unity Methods
        /// <summary>
        /// Initializes variables and sets callbacks.
        /// </summary>
        void Awake()
        {
            // references:
            _interactable = GetComponent<GazeInteractable>();

            // Register Events
            _interactable.OnGazeBegan += HandleOnGazeBegan;
            _interactable.OnGazeEnded += HandleOnGazeEnded;
            _interactable.OnGazePressed += HandleOnGazePressed;
            _interactable.OnGazeReleased += HandleOnGazeReleased;
        }

        /// <summary>
        /// Unregister callback handlers
        /// </summary>
        void OnDestroy()
        {
            // Unregister Events
            _interactable.OnGazeBegan -= HandleOnGazeBegan;
            _interactable.OnGazeEnded -= HandleOnGazeEnded;
            _interactable.OnGazePressed -= HandleOnGazePressed;
            _interactable.OnGazeReleased -= HandleOnGazeReleased;
        }
        #endregion

        #region Event Handlers
        /// <summary>
        /// When cursor is on the object
        /// </summary>
        private void HandleOnGazeBegan()
        {
            _primitive.ChangeEmission(new Color(0.106f, 0.447f, 0.576f));
        }

        /// <summary>
        /// When cursor is no longer on the object
        /// </summary>
        private void HandleOnGazeEnded()
        {
            _primitive.ResetEmission();
        }

        /// <summary>
        /// When the button in the controller is pressed
        /// </summary>
        private void HandleOnGazePressed()
        {
            _primitive.Expand();
        }

        /// <summary>
        /// When the button in the controller is released
        /// </summary>
        private void HandleOnGazeReleased()
        {
            _primitive.Contract();
        }
        #endregion
    }
}
