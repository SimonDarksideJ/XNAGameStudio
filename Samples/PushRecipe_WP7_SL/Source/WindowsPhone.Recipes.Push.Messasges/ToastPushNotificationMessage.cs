using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;

using WindowsPhone.Recipes.Push.Messasges.Properties;

namespace WindowsPhone.Recipes.Push.Messasges
{
    /// <summary>
    /// Represents a toast push notification message.
    /// </summary>
    /// <remarks>
    /// Toast notifications are system-wide notifications that do not disrupt
    /// the user workflow or require intervention to resolve. They are displayed
    /// at the top of the screen for ten seconds before disappearing. If the toast
    /// notification is tapped, the application that sent the toast notification
    /// will launch. A toast notification can be dismissed with a flick.
    /// 
    /// This class members are thread safe.
    /// </remarks>    
    public sealed class ToastPushNotificationMessage : PushNotificationMessage
    {
        #region Constants

        /// <value>Calculated toast message headers size.</value>
        /// <remarks>This should ne updated if changing the protocol.</remarks>
        private const int ToastMessageHeadersSize = 146;

        /// <value>Toast push notification message maximum payload size.</value>
        public const int MaxPayloadSize = MaxMessageSize - ToastMessageHeadersSize;

        /// <value>Windows phone target.</value>
        private const string WindowsPhoneTarget = "toast";

        /// <value>A well formed structure of the toast notification message.</value>
        private const string PayloadString =
            "<?xml version=\"1.0\" encoding=\"utf-8\"?>" +
            "<wp:Notification xmlns:wp=\"WPNotification\">" +
                "<wp:Toast>" +
                    "<wp:Text1>{0}</wp:Text1>" +
                    "<wp:Text2>{1}</wp:Text2>" +
                "</wp:Toast>" +
            "</wp:Notification>";

        #endregion

        #region Fields

        /// <value>The bolded string that should be displayed immediately after the application icon.</value>
        private string _title;

        /// <value>The non-bolded string that should be displayed immediately after the Title.</value>
        private string _subTitle;

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets a bolded string that should be displayed immediately after the application icon.
        /// </summary>
        public string Title
        {
            get
            {
                return _title;
            }

            set
            {
                SafeSet(ref _title, value);
            }
        }

        /// <summary>
        /// Gets or sets a non-bolded string that should be displayed immediately after the Title.
        /// </summary>
        public string SubTitle
        {
            get
            {
                return _subTitle;
            }

            set
            {
                SafeSet(ref _subTitle, value);
            }
        }

        /// <summary>
        /// Toast push notification message class id.
        /// </summary>
        protected override int NotificationClassId
        {
            get { return 2; }
        }

        #endregion

        #region Ctor
        /// <summary>
        /// Initializes a new instance of this type.
        /// </summary>
        /// <param name="sendPriority">The send priority of this message in the MPNS.</param>
        public ToastPushNotificationMessage(MessageSendPriority sendPriority = MessageSendPriority.Normal)
            : base(sendPriority)
        {

        } 
        #endregion

        #region Overrides

        /// <summary>
        /// Create the toast message payload.
        /// </summary>
        /// <returns>The message payload bytes.</returns>
        protected override byte[] OnCreatePayload()
        {
            var payloadString = string.Format(PayloadString, Title, SubTitle);
            return Encoding.ASCII.GetBytes(payloadString);
        }

        /// <summary>
        /// Initialize the request with toast specific headers.
        /// </summary>
        /// <param name="request">The message request.</param>
        protected override void OnInitializeRequest(HttpWebRequest request)
        {
            request.Headers[Headers.WindowsPhoneTarget] = WindowsPhoneTarget;
        }

        protected override void VerifyPayloadSize(byte[] payload)
        {
            if (payload.Length > MaxPayloadSize)
            {
                throw new ArgumentOutOfRangeException(string.Format(Resources.PayloadSizeIsTooBig, MaxPayloadSize));
            }
        }

        #endregion
    }
}
