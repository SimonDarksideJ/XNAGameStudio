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

using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Diagnostics;

namespace Microsoft.Phone.Applications.UnitConverter.Converters
{

    /// <summary>
    /// 
    /// </summary>
    public class DecimalSeparatorMarginConverter : IValueConverter
    {
        /// <summary>
        /// Modifies the source data before passing it to the target for display in the UI.
        /// </summary>
        /// <param name="value">The source data being passed to the target.</param>
        /// <param name="targetType">The type of data expected by 
        /// the target dependency property.</param>
        /// <param name="parameter">An optional parameter to be used in the converter logic.</param>
        /// <param name="culture">The culture of the conversion.</param>
        /// <returns>
        /// The value to be passed to the target dependency property.
        /// </returns>
        public object Convert(
            object value,
            Type targetType,
            object parameter,
            System.Globalization.CultureInfo culture)
        {
            string separator = value as string;
            Thickness margin;
            if (separator == null)
            {
                ApplicationState.ErrorLog.Add(new Helpers.ErrorLog("Decimal Separator ", "Value not a string"));

                // margin offest for all other '.' number format decimal separator
                margin = new Thickness(47, -39, 0, 0);
                return margin;
            }

            // these margins are for a font size of 80 with the default font

            if (separator.CompareTo(",") == 0)
            {
                // margin offest for the french ',' number format decimal separator
                margin = new Thickness(47, -40, 0, 0);
            }
            else
            {
                // margin offest for all other '.' number format decimal separator
                margin = new Thickness(47, -39, 0, 0);
            }

            return margin;
        }

        /// <summary>
        /// Modifies the target data before passing it to the source object.  
        /// This method is called only in 
        /// <see cref="F:System.Windows.Data.BindingMode.TwoWay"/> bindings.
        /// </summary>
        /// <param name="value">The target data being passed to the source.</param>
        /// <param name="targetType">The type of data expected by 
        /// the target dependency property.</param>
        /// <param name="parameter">An optional parameter to be used in the converter logic.</param>
        /// <param name="culture">The culture of the conversion.</param>
        /// <returns>
        /// The value to be passed to the source object.
        /// </returns>
        public object ConvertBack(
            object value,
            Type targetType,
            object parameter,
            CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

}
