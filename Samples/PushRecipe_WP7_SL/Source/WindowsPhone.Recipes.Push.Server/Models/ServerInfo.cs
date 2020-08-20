using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace WindowsPhone.Recipes.Push.Server.Models
{
    /// <summary>
    /// Server status data contract.
    /// </summary>
    [DataContract]
    public class ServerInfo
    {
        /// <summary>
        /// Current push pattern.
        /// </summary>
        [DataMember]
        public string PushPattern { get; set; }

        /// <summary>
        /// Current tile counter value.
        /// </summary>
        [DataMember]
        public int Counter { get; set; }
    }
}
