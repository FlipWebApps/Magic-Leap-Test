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
    /// This represents a virtual controller visualization that mimics the current state of the
    /// physical controller. Button presses, touch pad, sensitivy are all represented along with
    /// the orientation of the controller.  6dof positional behavior isn't replicated.
    /// </summary>
    public class ControllerExample : MonoBehaviour
    {
        #region Private Variables
        [SerializeField, Tooltip("The controller's bumper button.")]
        private GameObject _bumper;

        [SerializeField, Tooltip("The controller's trigger.")]
        private GameObject _trigger;

        [SerializeField, Tooltip("The controller's trigger mask.")]
        private MeshRenderer _triggerMask;

        [SerializeField, Tooltip("The controller's track pad.")]
        private GameObject _pad;

        [SerializeField, Tooltip("The controller's track pad mask.")]
        private MeshRenderer _padMask;

        private MLInputController _controller;

        /// <summary>
        /// Constants used in UpdateLED.
        /// </summary>
        private const float HALF_HOUR_IN_DEGREES = 15.0f;
        private const float DEGREES_PER_HOUR = 12.0f / 360.0f;

        private const int MIN_LED_INDEX = (int)(MLInputControllerFeedbackPatternLED.Clock12);
        private const int MAX_LED_INDEX = (int)(MLInputControllerFeedbackPatternLED.Clock6And12);
        private const int LED_INDEX_DELTA = MAX_LED_INDEX - MIN_LED_INDEX;

        private const float LED_TIMER = 0.1f;
        private float timer = 0.0f;
        #endregion

        #region Unity Methods
        /// <summary>
        /// Initializes component data and starts MLInput.
        /// </summary>
        void Awake()
        {
            if (!MLInput.Start())
            {
                Debug.LogError("Error ControllerExample starting MLInput, disabling script.");
                enabled = false;
                return;
            }
            if (_bumper == null)
            {
                Debug.LogError("Error ControllerExample._bumper is not set, disabling script.");
                enabled = false;
                return;
            }
            if (_trigger == null)
            {
                Debug.LogError("Error ControllerExample._trigger is not set, disabling script.");
                enabled = false;
                return;
            }
            if (_triggerMask == null)
            {
                Debug.LogError("Error ControllerExample._triggerMask is not set, disabling script.");
                enabled = false;
                return;
            }
            if (_pad == null)
            {
                Debug.LogError("Error ControllerExample._pad is not set, disabling script.");
                enabled = false;
                return;
            }
            if (_padMask == null)
            {
                Debug.LogError("Error ControllerExample._padMask is not set, disabling script.");
                enabled = false;
                return;
            }

            _bumper.SetActive(false);
            _trigger.SetActive(false);
            _pad.SetActive(false);

            MLInput.OnControllerButtonDown += OnButtonDown;
            MLInput.OnControllerButtonUp += OnButtonUp;
            _controller = MLInput.GetController(MLInput.Hand.Left);
        }

        /// <summary>
        /// Updates effects on different input responses via input polling mechanism.
        /// </summary>
        void Update()
        {
            UpdateController();
            UpdateLED();
            UpdateHaptics();
        }

        /// <summary>
        /// Cleans up the component.
        /// </summary>
        void OnDestroy()
        {
            MLInput.OnControllerButtonUp -= OnButtonUp;
            MLInput.OnControllerButtonDown -= OnButtonDown;
            MLInput.Stop();
        }
        #endregion

        #region Private Methods
        /// <summary>
        /// Updates controller status and buttons.
        /// </summary>
        private void UpdateController()
        {
            // Set state for bumper press if its a controller (6dof).
            if (_controller.Type == MLInputControllerType.Device)
            {
                _bumper.SetActive(_controller.State.ButtonState[(int)MLInputControllerButton.Bumper] != 0);

                // Handle pressure sensitivity feedback for trigger.
                _trigger.SetActive(_controller.TriggerValue > 0.2f); // Adding in some deadzone checking.
                SetPressure(_triggerMask, _controller.TriggerValue);
            }

            // The touch pad position is stored in the x and y component of the Vector3 and force is stored in z component.
            _pad.SetActive(_controller.Touch1Active);
            _pad.transform.localPosition = new Vector3(_controller.Touch1PosAndForce.x / -50.0f, 0.0f, _controller.Touch1PosAndForce.y / -50.0f);
            SetPressure(_padMask, _controller.Touch1PosAndForce.z);

            /* TODO: Uncomment, expose code below and remove ControllerCalibration when 6dof registration is finished.
            transform.position = _controller.Position;
            transform.rotation = _controller.Orientation;*/
        }

        /// <summary>
        /// Updates LED on the physical controller based on touch pad input.
        /// </summary>
        private void UpdateLED()
        {
            if (_controller.Type == MLInputControllerType.Device)
            {
                timer -= Time.deltaTime;

                if (_controller.Touch1Active && timer <= 0.0f)
                {
                    // Get angle of touchpad position.
                    float angle = -Vector2.SignedAngle(Vector2.up, _controller.Touch1PosAndForce);
                    if (angle < 0.0f)
                    {
                        angle += 360.0f;
                    }

                    // Get the correct hour and map it to [0,6]
                    int index = (int)((angle + HALF_HOUR_IN_DEGREES) * DEGREES_PER_HOUR) % LED_INDEX_DELTA;

                    // Pass from hour to MLInputControllerFeedbackPatternLED index  [0,6] -> [MIN_LED_INDEX + 1, MAX_LED_INDEX]
                    // 0 -> MAX_LED_INDEX , 1 -> MIN_LED_INDEX + 1, 2 -> MIN_LED_INDEX + 2 ...
                    index = (MAX_LED_INDEX + index > MAX_LED_INDEX) ? MIN_LED_INDEX + index : MAX_LED_INDEX;

                    _controller.StartFeedbackPatternLED((MLInputControllerFeedbackPatternLED)index, MLInputControllerFeedbackColorLED.Pink2, LED_TIMER);
                    timer = LED_TIMER;
                }
            }
        }

        /// <summary>
        /// Updates controller vibration haptics.
        /// </summary>
        private void UpdateHaptics()
        {
            if (_controller.Type == MLInputControllerType.Device)
            {
                // For trigger issue a buzz vibe with appropriate pressure sensitivity to the body.
                if (_controller.TriggerValue > 0.2f) // Adding in some deadzone checking.
                {
                    MLInputControllerFeedbackIntensity intensity = (MLInputControllerFeedbackIntensity)((int)(_controller.TriggerValue * 2.0f));
                    _controller.StartFeedbackPatternVibe(MLInputControllerFeedbackPatternVibe.Buzz, intensity);
                }
            }
        }

        /// <summary>
        /// Sets the visual pressure indicator for the appropriate button MeshRenderers.
        /// <param name="renderer">The meshrenderer to modify.</param>
        /// <param name="pressure">The pressure sensitivy interpolant for the meshrendere.r</param>
        /// </summary>
        private void SetPressure(MeshRenderer renderer, float pressure)
        {
            if (renderer.material.HasProperty("_Cutoff"))
            {
                renderer.material.SetFloat("_Cutoff", pressure);
            }
        }
        #endregion

        #region Event Handlers
        /// <summary>
        /// Handles the event for button down.
        /// </summary>
        /// <param name="controller_id">The id of the controller.</param>
        /// <param name="button">The button that is being pressed.</param>
        private void OnButtonDown(byte controller_id, MLInputControllerButton button)
        {
            if (button == MLInputControllerButton.Bumper)
            {
                // Demonstrate haptics using callbacks.
                _controller.StartFeedbackPatternVibe(MLInputControllerFeedbackPatternVibe.ForceDown, MLInputControllerFeedbackIntensity.Medium);
            }
        }

        /// <summary>
        /// Handles the event for button up.
        /// </summary>
        /// <param name="controller_id">The id of the controller.</param>
        /// <param name="button">The button that is being released.</param>
        private void OnButtonUp(byte controller_id, MLInputControllerButton button)
        {
            if (button == MLInputControllerButton.Bumper)
            {
                // Demonstrate haptics using callbacks.
                _controller.StartFeedbackPatternVibe(MLInputControllerFeedbackPatternVibe.ForceUp, MLInputControllerFeedbackIntensity.Medium);
            }
        }
        #endregion
    }
}
