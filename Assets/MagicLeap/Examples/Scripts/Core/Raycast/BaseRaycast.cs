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

using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Experimental.XR.MagicLeap;

namespace MagicLeap
{
    /// <summary>
    /// Base raycast class containing the some common variables and functionality.
    /// </summary>
    public abstract class BaseRaycast : MonoBehaviour
    {
        #region Public Variables
        [System.Serializable]
        public class RaycastResultEvent : UnityEvent<RaycastHit, float> { }

        [Space]
        [Tooltip("The callback handler for raycast result.")]
        public RaycastResultEvent OnRaycastResult;

        [Tooltip("The number of horizontal rays to cast. Single raycasts set to 1.")]
        public uint Width = 1;

        [Tooltip("The number of vertical rays to cast. Single raycasts set to 1.")]
        public uint Height = 1;

        [Tooltip("The horizontal field of view in degrees to determine density of Width/Height raycasts.")]
        public float HorizontalFovDegrees;

        [Tooltip("If true the ray will terminate when encountering an unobserved area. Useful for determining unmapped areas.")]
        public bool CollideWithUnobserved = false;
        #endregion

        #region Protected Variables
        protected MLWorldRays.QueryParams _raycastParams = new MLWorldRays.QueryParams();

        // Stores if previous raycast called finished (true) or not (false).
        protected bool _isReady = true;
        #endregion

        #region Public Properties
        /// <summary>
        /// Returns the last raycasts origination position.
        /// </summary>
        public Vector3 RayOrigin
        {
            get
            {
                return _raycastParams.Position;
            }
        }

        /// <summary>
        /// Returns the last raycasts origination direction.
        /// </summary>
        public Vector3 RayDirection
        {
            get
            {
                return _raycastParams.Direction;
            }
        }
        #endregion

        #region Protected Properties
        /// <summary>
        /// Returns raycast position.
        /// </summary>
        protected abstract Vector3 Position
        {
            get;
        }

        /// <summary>
        /// Returns raycast direction.
        /// </summary>
        protected abstract Vector3 Direction
        {
            get;
        }

        /// <summary>
        /// Returns raycast up.
        /// </summary>
        protected abstract Vector3 Up
        {
            get;
        }
        #endregion

        #region Unity Methods
        /// <summary>
        /// Initializes component.
        /// </summary>
        virtual protected void OnEnable()
        {
            _isReady = true;
            if(!MLWorldRays.Start())
            {
                Debug.LogError("Error BaseRaycast starting MLWorldRays, disabling script.");
                enabled = false;
                return;
            }
        }

        /// <summary>
        /// Cleans up component.
        /// </summary>
        virtual protected void OnDisable()
        {
            MLWorldRays.Stop();
        }

        /// <summary>
        /// Continuously casts rays using _raycastParams.
        /// </summary>
        private void Update()
        {
            if (_isReady)
            {
                _isReady = false;

                _raycastParams.Position = Position;
                _raycastParams.Direction = Direction;
                _raycastParams.UpVector = Up;
                _raycastParams.Width = Width;
                _raycastParams.Height = Height;
                _raycastParams.HorizontalFovDegrees = HorizontalFovDegrees;
                _raycastParams.CollideWithUnobserved = CollideWithUnobserved;

                MLWorldRays.GetWorldRays(_raycastParams, HandleOnReceiveRaycast);
            }
        }
        #endregion

        #region Private Methods
        /// <summary>
        /// Returns a RaycastHit based on results from callback function HandleOnReceiveRaycast.
        /// </summary>
        /// <param name="hit"> Stores if there is a hit.</param>
        /// <param name="point"> Position of the hit.</param>
        /// <param name="normal"> Normal of the surface hit.</param>
        /// <param name="confidence"> Confidence value on hit.</param>
        /// <returns></returns>
        protected RaycastHit GetWorldRaycastResult(bool hit, Vector3 point, Vector3 normal, float confidence)
        {
            RaycastHit result = new RaycastHit();

            if (hit == true && confidence > 0.0f)
            {
                result.point = point;
                result.normal = normal;
                result.distance = Vector3.Distance(_raycastParams.Position, point);
            }

            return result;
        }
        #endregion

        #region Event Handlers
        /// <summary>
        /// Callback handler called when raycast call has a result.
        /// </summary>
        /// <param name="hit"> Stores if there is a hit.</param>
        /// <param name="point"> Position of the hit.</param>
        /// <param name="normal"> Normal of the surface hit.</param>
        /// <param name="confidence"> Confidence value on hit.</param>
        virtual protected void HandleOnReceiveRaycast(bool hit, Vector3 point, Vector3 normal, float confidence)
        {
            RaycastHit result = GetWorldRaycastResult(hit, point, normal, confidence);

            OnRaycastResult.Invoke(result, confidence);

            _isReady = true;
        }
        #endregion
    }
}
