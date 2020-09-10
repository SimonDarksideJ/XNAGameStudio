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
using System.ComponentModel;

namespace Microsoft.Phone.Applications.UnitConverter.Model
{

    /// <summary>
    /// Properties that will be saved as part o the Favorite list that we will
    /// serialize out to a file.
    /// In addition, we will use these properties as a source for data binding
    /// to allow a UI element to display this data.
    /// </summary>
    public class FavoriteData : INotifyPropertyChanged, IEquatable<FavoriteData>
    {
        /// <summary>
        /// Favorite data file in isolated storage
        /// </summary>
        internal const string FavoriteFileName = "WmUnitConverterFavorites.xml";

        #region Bindable Properties


        /// <summary>
        /// Backing store for favorites display string in the list box.
        /// </summary>
        private string labelName;

      
        /// <summary>
        /// Favorite Display name for the list box
        /// </summary>
        public string LabelName
        {
            get { return this.labelName; }

            set
            {
                this.labelName = value;
                this.OnPropertyChanged("LabelName");
            }
        }

        /// <summary>
        /// Backing store for Category
        /// </summary>
        private string category;

      
        /// <summary>
        /// Unit Category
        /// </summary>
        public string Category
        {
            get { return this.category; }

            set
            {
                this.category = value;
                this.OnPropertyChanged("Category");
            }
        }

        /// <summary>
        /// Backing store for Category resource ID
        /// </summary>
        private string categoryResource;


        /// <summary>
        /// Unit Category Resource
        /// </summary>
        public string CategoryResource
        {
            get { return this.categoryResource; }

            set
            {
                this.categoryResource = value;
                this.OnPropertyChanged("CategoryResource");
            }
        }

        /// <summary>
        /// Backing store for the Source Unit type
        /// </summary>
        private string sourceUnitName;

        /// <summary>
        /// Source Unit type information
        /// </summary>
        public string SourceUnitName
        {
            get { return this.sourceUnitName; }

            set
            {
                this.sourceUnitName = value;
                this.OnPropertyChanged("SourceUnitName");
            }
        }

        /// <summary>
        /// Backing store for the Source Unit type
        /// </summary>
        private string sourceUnitNameResource;

        /// <summary>
        /// Source Unit type information
        /// </summary>
        public string SourceUnitNameResource
        {
            get { return this.sourceUnitNameResource; }

            set
            {
                this.sourceUnitNameResource = value;
                this.OnPropertyChanged("SourceUnitNameResource");
            }
        }

        /// <summary>
        /// Backing store for the target Unit type
        /// </summary>
        private string targetUnitName;

        /// <summary>
        /// Target Unit type information
        /// </summary>
        public string TargetUnitName
        {
            get { return this.targetUnitName; }

            set
            {
                this.targetUnitName = value;
                this.OnPropertyChanged("TargetUnitName");
            }
        }

        /// <summary>
        /// Backing store for the target Unit type
        /// </summary>
        private string targetUnitNameResource;

        /// <summary>
        /// Target Unit type information
        /// </summary>
        public string TargetUnitNameResource
        {
            get { return this.targetUnitNameResource; }

            set
            {
                this.targetUnitNameResource = value;
                this.OnPropertyChanged("TargetUnitNameResource");
            }
        }

        /// <summary>
        /// Backing store for the source unit data value for the conversion
        /// </summary>
        private double sourceUnitValue;

        /// <summary>
        /// Gets or sets the source value that the user is entering for the value 
        /// for the conversion
        /// </summary>
        /// <value>The source value.</value>
        public double SourceUnitValue
        {
            get { return this.sourceUnitValue; }

            set
            {
                this.sourceUnitValue = value;
                this.OnPropertyChanged("SourceUnitValue");
            }
        }

        #endregion Bindable properties

        #region INotifyPropertyChanged

        /// <summary>
        /// Standard pattern for data binding and notifications.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Notify subscribers of a change in the property
        /// </summary>
        /// <param name="propertyName">Name of the property to signal there has been a changed</param>
        private void OnPropertyChanged(string propertyName)
        {
            if (this.PropertyChanged != null)
            {
                PropertyChangedEventArgs args = new PropertyChangedEventArgs(propertyName);
                this.PropertyChanged(this, args);
            }
        }

        #endregion
  

        /// <summary>
        /// Parameterless ctor for serialization
        /// </summary>
        public FavoriteData()
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FavoriteData"/> class.
        /// </summary>
        /// <param name="conversion">Current active conversion information</param>
        /// <param name="sourceValue">The source value numeric entered by the user.</param>
        internal FavoriteData(CurrentConversion conversion, double sourceValue)
        {
            this.category            = conversion.CurrentCategory.CategoryLocalized;
            this.categoryResource    = conversion.CurrentCategory.Category;
            this.sourceUnitName      = conversion.SourceUnit.NameLocalized;
            this.sourceUnitNameResource = conversion.SourceUnit.ResourceName;
            this.targetUnitName      = conversion.TargetUnit.NameLocalized;
            this.targetUnitNameResource = conversion.TargetUnit.ResourceName;
            this.sourceUnitValue     = sourceValue;
        }


        /// <summary>
        /// Specify when items are equal
        /// </summary>
        /// <param name="other">The value to compare against</param>
        /// <returns>True if the items are equal</returns>
        public bool Equals(FavoriteData other)
        {
            if (other == null)
            {
                return false;
            }

            return  (this.Category == other.Category &&
                 this.SourceUnitName == other.SourceUnitName &&
                 this.TargetUnitName == other.TargetUnitName)
            ////      &&  this.SourceValue == other.SourceValue) 
            ? true : false;
        }
    }
}
