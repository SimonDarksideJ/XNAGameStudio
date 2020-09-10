using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using WindowsPhone.Recipes.Push.Messasges.Properties;

namespace WindowsPhone.Recipes.Push.Messasges
{
    /// <summary>
    /// Represents a tile push notification message.
    /// </summary>
    /// <remarks>
    /// Every phone application has one assigned 'tile' – a visual, dynamic
    /// representation of the application or its content. A tile displays in
    /// the Start screen if the end user has pinned it.
    /// 
    /// This class members are thread safe.
    /// </remarks>
    public sealed class TilePushNotificationMessage : PushNotificationMessage
    {
        #region Constants

        /// <value>Calculated tile message headers size.</value>
        /// <remarks>This should ne updated if changing the protocol.</remarks>
        private const int TileMessageHeadersSize = 146;

        /// <value>Tile push notification message maximum payload size.</value>
        public const int MaxPayloadSize = MaxMessageSize - TileMessageHeadersSize;

        /// <value>The minimum <see cref="TilePushNotificationMessage.Count"/> value.</value>
        public const int MinCount = 0;

        /// <value>The maximum <see cref="TilePushNotificationMessage.Count"/> value.</value>
        public const int MaxCount = 99;        

        /// <value>Windows phone target.</value>
        private const string WindowsPhoneTarget = "token";

        /// <value>A well formed structure of the tile notification message.</value>
        private const string PayloadString =
            "<?xml version=\"1.0\" encoding=\"utf-8\"?>" +
            "<wp:Notification xmlns:wp=\"WPNotification\">" +
                "<wp:Tile>" +
                    "<wp:BackgroundImage>{0}</wp:BackgroundImage>" +
                    "<wp:Count>{1}</wp:Count>" +
                    "<wp:Title>{2}</wp:Title>" +
                "</wp:Tile>" +
            "</wp:Notification>";

        #endregion

        #region Fields

        /// <value>The phone's local path, or a remote path for the background image.</value>
        private Uri _backgroundImageUri;

        /// <value>An integer value to be displayed in the tile.</value>
        private int _count = MinCount;

        /// <value>The title text should be displayed in the tile.</value>
        private string _title;

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the phone's local path, or a remote path for the background image.
        /// </summary>
        /// <remarks>
        /// If the uri references a remote resource, the maximum allowed size of the tile
        /// image is 80 KB, with a maximum download time of 15 seconds.
        /// </remarks>
        public Uri BackgroundImageUri
        {
            get
            {
                return _backgroundImageUri;
            }

            set
            {
                SafeSet(ref _backgroundImageUri, value);
            }
        }

        /// <summary>
        /// Gets or sets an integer value from 1 to 99 to be displayed in the tile, or 0 to clear count.
        /// </summary>
        public int Count
        {
            get
            {
                return _count;
            }

            set
            {
                if (value < MinCount || value > MaxCount)
                {
                    throw new ArgumentOutOfRangeException(string.Format(Resources.CountValueIsNotValid, value, MinCount, MaxCount));
                }

                SafeSet(ref _count, value);
            }
        }

        /// <summary>
        /// Gets or sets the title text should be displayed in the tile. Null keeps the existing title.
        /// </summary>
        /// <remarks>
        /// The Title must fit a single line of text and should not be wider than the actual tile.
        /// Imperatively a good number of letters would be 18-20 characters long.
        /// </remarks>
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
        /// Tile push notification message class id.
        /// </summary>
        protected override int NotificationClassId
        {
            get { return 1; }
        }

        #endregion

        #region Ctor
        /// <summary>
        /// Initializes a new instance of this type.
        /// </summary>
        /// <param name="sendPriority">The send priority of this message in the MPNS.</param>
        public TilePushNotificationMessage(MessageSendPriority sendPriority = MessageSendPriority.Normal)
            : base(sendPriority)
        {

        } 
        #endregion

        #region Overrides

        /// <summary>
        /// Create the tile message payload.
        /// </summary>
        /// <returns>The message payload bytes.</returns>
        protected override byte[] OnCreatePayload()
        {
            var payloadString = string.Format(PayloadString, BackgroundImageUri, Count, Title);
            return Encoding.ASCII.GetBytes(payloadString);
        }

        /// <summary>
        /// Initialize the request with tile specific headers.
        /// </summary>
        /// <param name="request">The message request.</param>
        protected override void OnInitializeRequest(System.Net.HttpWebRequest request)
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
