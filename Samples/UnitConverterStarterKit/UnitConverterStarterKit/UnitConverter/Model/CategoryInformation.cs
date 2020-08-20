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
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using Microsoft.Phone.Applications.UnitConverter.View;


namespace Microsoft.Phone.Applications.UnitConverter.Model
{
    /// <summary>
    /// Container for the conversion information for units for a particular category
    /// </summary>
    public class CategoryInformation
    {
        /// <summary>
        /// Gets or sets the category resource string name
        /// </summary>
        public string Category { get; set; }

        /// <summary>
        /// Gets or sets the category name localized. Read from the string resources
        /// per the locale
        /// </summary>
        [XmlIgnore]
        public string CategoryLocalized { get; set; }

        /// <summary>
        /// Gets or sets the units for this category
        /// </summary>
        public UnitInformation[] Units { get; set; }


        /// <summary>
        /// Gets or sets the pivot list box item for this category.
        /// </summary>
        [XmlIgnore]
        [IgnoreDataMember]
        public TwoListBoxes PivotUnitSelect { get; set; }

        /// <summary>
        /// For faster access to unit information, we provide dictionary access 
        /// to find the unit object for the name
        /// </summary>
        [XmlIgnore]
        [IgnoreDataMember]
        public Dictionary<string, UnitInformation> UnitAccess { get; private set; }


        /// <summary>
        /// Initializes a new instance of the <see cref="CategoryInformation"/> class.
        /// </summary>
        public CategoryInformation()
        {
            this.UnitAccess = new Dictionary<string, UnitInformation>();
        }

        /// <summary>
        /// Initializes the dictionary.
        /// </summary>
        internal void InitializeDictionary()
        {
            this.UnitAccess = new Dictionary<string, UnitInformation>();
            foreach ( UnitInformation u in this.Units )
            {
                this.UnitAccess.Add(u.NameLocalized, u);
            }
        }

        /// <summary>
        /// Reads the localized names from string resources
        /// </summary>
        internal void ReadLocalizedNames()
        {
            foreach (UnitInformation u in this.Units)
            {
                u.NameLocalized =
                    Resources.Strings.ResourceManager.GetString(u.ResourceName);
            }
        }

        /// <summary>
        /// Determine if the unit name is part of the unit collection
        /// </summary>
        /// <param name="unitName">name to check is part of the collection</param>
        /// <returns>true if the unitName is in this objets collection</returns>
        internal bool IsUnit(string unitName)
        {
            var unit = (from foundUnit in this.Units
                        where foundUnit.NameLocalized.Contains(unitName)
                        select foundUnit).Count();
            return unit > 0 ? true : false;
        }

        /// <summary>
        /// Return the unit object from the unit name
        /// </summary>
        /// <param name="unitName">name to check is part of the collection</param>
        /// <returns>The Unit object </returns>
        internal UnitInformation FindUnit(string unitName)
        {
            var unit = (from foundUnit in this.Units
                        where foundUnit.NameLocalized.Contains(unitName)
                        select foundUnit).FirstOrDefault<UnitInformation>();
            return unit;
        }

        /// <summary>
        /// Enable data binding for the list views
        /// </summary>
        /// <param name="bindToCollection">True if we are to bind the data sources 
        /// to the control. False otherwise</param>
        internal void ConfigureItemsSourceBinding(bool bindToCollection)
        {
            if (bindToCollection)
            {
                if ( this.PivotUnitSelect.fromListView.ItemsSource == null)
                {
                    // With optimization for data binding, we only want to change the 
                    // source if it has not been set. If the user just trys to update the 
                    // data binding, the ItemsSource needs to be set to null first.
                    this.PivotUnitSelect.fromListView.ItemsSource = this.Units;
                }

                if (this.PivotUnitSelect.toListView.ItemsSource == null)
                {
                    // Favorite collection only has one list view
                    this.PivotUnitSelect.toListView.ItemsSource = this.Units;
                }
            }
            else
            {
                this.PivotUnitSelect.fromListView.ItemsSource = null;
                this.PivotUnitSelect.toListView.ItemsSource = null;
            }
        }


        /// <summary>
        /// Converts the specified value from the source unit type, to the target 
        /// unit value
        /// </summary>
        /// <param name="valueToConvert">The value to convert.</param>
        /// <param name="source">The source unit</param>
        /// <param name="target">The target unit</param>
        /// <returns></returns>
        internal static double Convert(
            double valueToConvert , 
            UnitInformation source, 
            UnitInformation target)
        {

            if ((source == null) || (target == null))
            {
                throw new ArgumentNullException("source" , "source or dest was null" );
            }

            if (source == target)
            {
                return valueToConvert;
            }
            double result = 0.0;
            // Convert to the base unit type for the category. 
            if (source.FormulaInvert)
            {
                result = (valueToConvert - source.Offset) * source.Multiplier;
            }
            else
            {
                result = valueToConvert * source.Multiplier + source.Offset;
            }

            // Convert to the target unit
            if (target.FormulaInvert)
            {
                result = result / target.Multiplier + target.Offset;
            }
            else
            {
                result = (result - target.Offset) / target.Multiplier;
            }
            return result;
        }
    }
}
