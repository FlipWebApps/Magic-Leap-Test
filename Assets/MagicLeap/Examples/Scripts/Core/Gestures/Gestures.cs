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

using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.XR.MagicLeap;

namespace MagicLeap
{
    [Flags]
    public enum GestureTypes
    {
        Finger = (1 << 0),
        Fist = (1 << 1),
        Pinch = (1 << 2),
        Thumb = (1 << 3),
        L = (1 << 4),
        OpenHandBack = (1 << 5),
        Ok = (1 << 6),
        C = (1 << 7),
        NoHand = (1 << 8),
    }

    /// <summary>
    /// Component used to communicate with the MLGestures API and manage
    /// which gestures are currently being tracked by each hand.
    /// Gestures can be added and removed from the tracker during runtime.
    /// </summary>
    public class Gestures : MonoBehaviour
    {
        #region Private Variables
        [Space, SerializeField, BitMask(typeof(GestureTypes)), Tooltip("All gestures to be tracked")]
        private GestureTypes _trackedGestures;

        private GestureTypes _currentGestures;
        #endregion

        #region Public Properties
        public GestureTypes TrackedGestures
        {
            get
            {
                return _currentGestures;
            }
        }
        #endregion

        #region Unity Methods
        /// <summary>
        /// Initializes and finds references to all relevant components in the
        /// scene and registers required events.
        /// </summary>
        void OnEnable()
        {
            if (!MLHands.Start())
            {
                Debug.LogError("Error Gestures starting MLHands, disabling script.");
                enabled = false;
                return;
            }

            UpdateGestureStates(true);
        }

        /// <summary>
        /// Stops the communication to the Gestures API and unregisters required events.
        /// </summary>
        void OnDisable()
        {
            if (MLHands.IsStarted)
            {
                // Disable all gestures if MLHands was started
                // and is about to stop
                UpdateGestureStates(false);
            }
            MLHands.Stop();
        }

        /// <summary>
        /// Update gestures tracked if enum value changed.
        /// </summary>
        void Update()
        {
            if((_trackedGestures ^ _currentGestures) != 0)
            {
               UpdateGestureStates(true);
            }
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Adds gesture if it's not there already.
        /// </summary>
        /// <param name="gesture"> Gesture to add. </param>
        public void AddGesture(GestureTypes gesture)
        {
            if ((gesture & _trackedGestures) != gesture)
            {
                _trackedGestures |= gesture;
                UpdateGestureStates(true);
            }
        }

        /// <summary>
        /// Removes gesture if it's there.
        /// </summary>
        /// <param name="gesture"> Gesture to remove. </param>
        public void RemoveGesture(GestureTypes gesture)
        {
            if ((gesture & _trackedGestures) == gesture)
            {
                _trackedGestures ^= gesture;
                UpdateGestureStates(true);
            }
        }
        #endregion

        #region Private Methods
        /// <summary>
        /// Get the gestures enabled in an MLStaticGestureType array.
        /// </summary>
        /// <returns> The array of gestures being tracked.</returns>
        private MLStaticGestureType[] GetGestureTypes()
        {
            int[] enumValues = (int[])Enum.GetValues(typeof(GestureTypes));
            List<MLStaticGestureType> gestures = new List<MLStaticGestureType>();

            _currentGestures = 0;
            GestureTypes current;
            for(int i = 0; i < enumValues.Length; ++i)
            {
                current = (GestureTypes)enumValues[i];
                if ((_trackedGestures & current) == current)
                {
                    _currentGestures |= current;
                    gestures.Add((MLStaticGestureType)i);
                }
            }

            return gestures.ToArray();
        }

        /// <summary>
        /// Updates the list of gestures internal to the magic leap device,
        /// enabling or disabling gestures that should be tracked.
        /// </summary>
        private void UpdateGestureStates(bool enableState = true)
        {
            MLStaticGestureType[] gestureTypes = GetGestureTypes();

            // Early out in case there are no gestures to enable.
            if (gestureTypes.Length == 0)
            {
                MLHands.GestureManager.DisableAllGestures();
                return;
            }

            bool status = MLHands.GestureManager.EnableGestures(gestureTypes, enableState, true);
            if (!status)
            {
                Debug.LogError("EnableGestures failed during a call to enable tracked gestures.\n"
                    + "Disabling Gestures component.");
                enabled = false;
                return;
            }
        }
        #endregion
    }
}
