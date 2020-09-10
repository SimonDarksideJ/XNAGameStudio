using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using System.IO;
using System.ServiceModel.Web;

namespace WindowsPhone.Recipes.Push.Server.Services
{
    /// <summary>
    /// Provides custom tile images.
    /// </summary>
    [ServiceContract]
    public interface IImageService
    {
        /// <summary>
        /// Get custom tile image.
        /// </summary>
        /// <param name="parameter">The tile image request parameter.</param>
        /// <returns>Custom tile image stream.</returns>
        [OperationContract, WebGet]
        Stream GetTileImage(string parameter);
    }
}
