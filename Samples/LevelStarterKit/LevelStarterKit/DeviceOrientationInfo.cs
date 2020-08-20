// Copyright (c) 2010 Microsoft Corporation.  All rights reserved.
//
//
// Use of this source code is subject to the terms of the Microsoft
// license agreement under which you licensed this source code.
// If you did not accept the terms of the license agreement,
// you are not authorized to use this source code.
// For the terms of the license, please see the license agreement
// signed by you and Microsoft.
// THE SOURCE CODE IS PROVIDED "AS IS", WITH NO WARRANTIES OR INDEMNITIES.
//




namespace Microsoft.Phone.Applications.Common
{

    /// <summary>
    /// Orientation related information
    /// </summary>
    public class DeviceOrientationInfo
    {
        public double AngleOnXYPlan { get; set; }
        public int HorizontalAxisPolarity { get; set; }
        public Simple3DVector NormalGravityVector { get; set; }

        public DeviceOrientationInfo(double angle, int horizontalScreenAxisPolarity, Simple3DVector typicalGravityVector)
        {
            AngleOnXYPlan = angle;
            HorizontalAxisPolarity = horizontalScreenAxisPolarity;
            NormalGravityVector = typicalGravityVector;
        }
    }


}