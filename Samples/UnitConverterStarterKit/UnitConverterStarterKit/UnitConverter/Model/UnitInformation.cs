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
using System.Xml.Serialization;

namespace Microsoft.Phone.Applications.UnitConverter.Model
{
    /// <summary>
    /// 
    /// </summary>
    public class UnitInformation
    {
        /// <summary>
        /// The Unit String Resource  identifier
        /// </summary>
        public string ResourceName { get; set; }

        /// <summary>
        /// Localized name from ResourceName
        /// </summary>
        [XmlIgnore]
        public string NameLocalized { get; set; }

        /// <summary>
        /// Conversion multiplier
        /// </summary>
        public double Multiplier { get; set; }

        /// <summary>
        /// Conversion offset
        /// </summary>
        public double Offset { get; set; }

        /// <summary>
        /// When true, we invert the formula in the conversion logic 
        /// </summary>
        public bool FormulaInvert { get; set; }

        /// <summary>
        /// Default constructor
        /// </summary>
        public UnitInformation()
        {
        }

        /// <summary>
        /// Initialize All members
        /// </summary>
        /// <param resourceName="resourceName">The name of the string resource.</param>
        /// <param name="multiplier">The multiplier.</param>
        /// <param name="offset">The offset.</param>
        /// <param name="formulaInvert">If true, we need to invert the conversion formula for
        /// this unit. The standard formula would be not in Y = mx + b, but in x = (Y-b)/m</param>
        public UnitInformation(
            string resourceName,
            double multiplier,
            double offset ,
            bool formulaInvert)
        {
            this.ResourceName = resourceName;
            this.Multiplier = multiplier;
            this.Offset = offset;
            this.FormulaInvert = formulaInvert;
        }

    }
}
