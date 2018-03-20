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

namespace MagicLeap
{
    /// <summary>
    /// Simple interface to Magic Leap's primitive to ensure all visual augmentations are consistant to the visual assets
    /// </summary>
    public class Primitive : MonoBehaviour
    {
        #region Private Variables
        private const float _expansionAmount = 1.3f;
        private Material _innerGlass;
        private Transform _expansionPiece;
        private Color _initialColor;
        private Color _initialEmission;

        private const string _emissionProperty = "_EmissionColor";
        #endregion

        #region Unity Methods
        void Awake()
        {
            //get pieces and hook them to references:
            foreach (Transform item in transform)
            {
                //find inner glass:
                if (item.name.Contains("InnerGlass"))
                {
                    _innerGlass = item.GetComponent<Renderer>().material;
                    _initialColor = _innerGlass.color;
                    _initialEmission = _innerGlass.GetColor(_emissionProperty);
                }

                //find expansion piece:
                if (item.name.Contains("Plastic"))
                {
                    _expansionPiece = item;
                }
            }
        }
        #endregion

        #region Public Methods
        public void Expand()
        {
            _expansionPiece.localScale = Vector3.one * _expansionAmount;
        }

        public void Contract()
        {
            _expansionPiece.localScale = Vector3.one;
        }

        public void ChangeColor(Color color)
        {
            _innerGlass.color = color;
        }

        public void ChangeEmission(Color color)
        {
            _innerGlass.SetColor(_emissionProperty, color);
            _innerGlass.EnableKeyword("_EMISSION");
        }

        public void ResetColor()
        {
            _innerGlass.color = _initialColor;
        }

        public void ResetEmission()
        {
            _innerGlass.SetColor(_emissionProperty, _initialEmission);
        }
        #endregion
    }
}
