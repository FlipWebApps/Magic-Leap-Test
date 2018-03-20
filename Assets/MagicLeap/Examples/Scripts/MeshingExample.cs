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

#if UNITY_EDITOR || PLATFORM_LUMIN

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Experimental.XR.MagicLeap;

namespace MagicLeap
{
    /// <summary>
    /// This represents all the runtime control over meshing component in order to best visualize the
    /// affect changing parameters has over the meshing API.
    /// </summary>
    public class MeshingExample : MonoBehaviour
    {
        #region Private Variables
        [SerializeField, Tooltip("Prefab to shoot into the scene.")]
        private GameObject _prefab;

        [SerializeField, Tooltip("The headpose canvas for example status text.")]
        private Text _statusLabel;

        [SerializeField, Tooltip("Visualizer for headpose raycast result.")]
        private RaycastVisualizer _cursorHead;

        [SerializeField, Tooltip("Raycast from headpose.")]
        private WorldRaycastHead _raycastHead;

        [SerializeField, Tooltip("Game object transform data that drives the Spatial Mapper parameters.")]
        private SpatialMappingFromObject _spatialMappingFromObject;

        [SerializeField, Tooltip("Visualizer for the meshing results.")]
        private MeshingVisualizer _meshingVisualizer;

        private MeshingVisualizer.RenderMode _renderMode = MeshingVisualizer.RenderMode.Wireframe;
        private int _modeCount = System.Enum.GetNames(typeof(MeshingVisualizer.RenderMode)).Length;

        private MLInputController _controller;

        private readonly Vector3 _scaleIncrement = new Vector3(0.1f, 0.1f, 0.1f);
        private const float MAX_BOUNDS_SIZE = 10.0f;

        bool _triggerPressed = false;

        #endregion

        #region Unity Methods
        /// <summary>
        /// Initializes component data and starts MLInput.
        /// </summary>
        void Awake()
        {
            if (!MLInput.Start())
            {
                Debug.LogError("Error MeshingExample starting MLInput, disabling script.");
                enabled = false;
                return;
            }
            if (_statusLabel == null)
            {
                Debug.LogError("Error MeshingExample._statusLabel is not set, disabling script.");
                enabled = false;
                return;
            }
            if (_prefab == null)
            {
                Debug.LogError("Error MeshingExample._prefab is not set, disabling script.");
                enabled = false;
                return;
            }
            if (_cursorHead == null)
            {
                Debug.LogError("Error MeshingExample._cursorHead is not set, disabling script.");
                enabled = false;
                return;
            }
            if (_raycastHead == null)
            {
                Debug.LogError("Error MeshingExample._raycast is not set, disabling script.");
                enabled = false;
                return;
            }
            if (_meshingVisualizer == null)
            {
                Debug.LogError("Error MeshingExample._meshingVisualizer is not set, disabling script.");
                enabled = false;
                return;
            }
            if (_spatialMappingFromObject == null)
            {
                Debug.LogError("Error MeshingExample._spatialMappingFromObject is not set, disabling script.");
                enabled = false;
                return;
            }
            _meshingVisualizer.SetRenderers(_renderMode);
            _controller = MLInput.GetController(MLInput.Hand.Left);
            MLInput.OnControllerButtonUp += OnButtonUp;
        }

        /// <summary>
        /// Polls input and updates components.
        /// </summary>
        void Update()
        {
            UpdateBalls();
            UpdateMeshingParameters();
        }

        /// <summary>
        /// Cleans up the component.
        /// </summary>
        void OnDestroy()
        {
            MLInput.OnControllerButtonUp -= OnButtonUp;
            MLInput.Stop();
        }
        #endregion

        #region Private Methods
        /// <summary>
        /// Checks too see if trigger was pressed and shoot a ball into the scene when block meshing.
        /// </summary>
        void UpdateBalls()
        {
            if (!_triggerPressed)
            {
                if (_controller.TriggerValue > 0.8f)
                {
                    _triggerPressed = true;

                    // Create the ball and necessary components and shoot it along raycast.
                    GameObject ball = Instantiate(_prefab);
                    SphereCollider colliderComponent = ball.AddComponent<SphereCollider>();
                    colliderComponent.radius = 0.15f;
                    Rigidbody rigidBody = ball.GetComponent<Rigidbody>();
                    if (rigidBody == null)
                    {
                        rigidBody = ball.AddComponent<Rigidbody>();
                    }

                    ball.transform.SetParent(ball.transform);
                    ball.transform.position = _raycastHead.RayOrigin;
                    rigidBody.AddForce(_raycastHead.RayDirection * 500);

                    Destroy(ball, 10);
                }
            }
            else if (_controller.TriggerValue < 0.2f)
            {
                _triggerPressed = false;
            }
        }

        /// <summary>
        /// Updates meshing parameters based on input.
        /// </summary>
        void UpdateMeshingParameters()
        {
            if (_controller.TouchpadGesture.Type == MLInputControllerTouchpadGestureType.RadialScroll && _controller.TouchpadGestureState != MLInputControllerTouchpadGestureState.End)
            {
                _spatialMappingFromObject.transform.rotation = Quaternion.AngleAxis(_controller.TouchpadGesture.Angle * Mathf.Rad2Deg, _spatialMappingFromObject.transform.up);
            }
            else if (_controller.TouchpadGesture.Type == MLInputControllerTouchpadGestureType.Swipe && _controller.TouchpadGestureState != MLInputControllerTouchpadGestureState.End)
            {
                if (_controller.TouchpadGesture.Direction == MLInputControllerTouchpadGestureDirection.Up)
                {
                    _spatialMappingFromObject.transform.localScale += _scaleIncrement;
                }
                else if (_controller.TouchpadGesture.Direction == MLInputControllerTouchpadGestureDirection.Down)
                {
                    _spatialMappingFromObject.transform.localScale -= _scaleIncrement;
                }
                else
                {
                    return;
                }

                _spatialMappingFromObject.transform.localScale = new Vector3(Mathf.Clamp(_spatialMappingFromObject.transform.localScale.x, 0.0f, MAX_BOUNDS_SIZE),
                                                                             Mathf.Clamp(_spatialMappingFromObject.transform.localScale.y, 0.0f, MAX_BOUNDS_SIZE),
                                                                             Mathf.Clamp(_spatialMappingFromObject.transform.localScale.z, 0.0f, MAX_BOUNDS_SIZE));
            }
            else if (_controller.TriggerValue > 0.8f && _controller.State.ButtonState[(int)MLInputControllerButton.Bumper] != 0)
            {
                _spatialMappingFromObject.transform.position = _cursorHead.transform.position;
            }
        }

        /// <summary>
        /// Updates examples status text.
        /// </summary>
        private void UpdateStatusText()
        {
            _statusLabel.text = string.Format("Render Mode: {0}\nMesh Type: {1}", _renderMode.ToString(), _meshingVisualizer.SpatialMapper.meshType.ToString());
        }
        #endregion

        #region Event Handlers
        /// <summary>
        /// Handles the event for button up.
        /// </summary>
        /// <param name="controller_id">The id of the controller.</param>
        /// <param name="button">The button that is being released.</param>
        private void OnButtonUp(byte controller_id, MLInputControllerButton button)
        {
            MLInputController controller = MLInput.GetController(controller_id);
            if (button == MLInputControllerButton.Bumper && controller.TriggerValue < 0.8f)
            {
                _renderMode = (MeshingVisualizer.RenderMode)((int)(_renderMode + 1) % _modeCount);
                _meshingVisualizer.SetRenderers(_renderMode);
            }
            else if (button == MLInputControllerButton.HomeTap)
            {
                _meshingVisualizer.SpatialMapper.meshType = (_meshingVisualizer.SpatialMapper.meshType == MLSpatialMapper.MeshType.Blocks) ? MLSpatialMapper.MeshType.Full : MLSpatialMapper.MeshType.Blocks;
            }
            UpdateStatusText();
        }
        #endregion
    }
}

#endif
