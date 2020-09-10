using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.Composition;
using System.Windows.Input;
using System.IO;
using Microsoft.Win32;

using WindowsPhone.Recipes.Push.Messasges;
using WindowsPhone.Recipes.Push.Server.Models;

namespace WindowsPhone.Recipes.Push.Server.ViewModels
{
    /// <summary>
    /// Represents the One Time push notification pattern.
    /// </summary>
    /// <remarks>
    /// This is the simplest push notification pattern of just pushing single time message
    /// to a registered client.
    /// </remarks>
    [Export(typeof(PushPatternViewModel)), PartCreationPolicy(CreationPolicy.Shared)]
    internal sealed class OneTimePushPatternViewModel : PushPatternViewModel
    {        
        #region Properties        

        /// <summary>
        /// Gets or sets a value indicating if tile message should be sent.
        /// </summary>
        public bool IsTileEnabled { get; set; }

        /// <summary>
        /// Gets or sets a value indicating if toast message should be sent.
        /// </summary>
        public bool IsToastEnabled { get; set; }

        /// <summary>
        /// Gets or sets a value indicating if raw message should be sent.
        /// </summary>
        public bool IsRawEnabled { get; set; }

        #endregion

        #region Ctor

        /// <summary>
        /// Initialize new instance of this type with defaults.
        /// </summary>
        public OneTimePushPatternViewModel()
        {
            InitializeDefaults();
        }

        #endregion

        #region Overrides

        /// <summary>
        /// Depends on what message was selected, send all subscribers zero or all three push message types (Tile, Toast, Raw).
        /// </summary>
        protected override void OnSend()
        {
            var messages = new List<PushNotificationMessage>();

            if (IsTileEnabled)
            {
                // Prepare a tile push notification message.
                messages.Add(new TilePushNotificationMessage(MessageSendPriority.High)
                {
                    BackgroundImageUri = BackgroundImageUri,
                    Count = Count,
                    Title = Title
                });
            }


            if (IsToastEnabled)
            {
                // Prepare a toast push notification message.
                messages.Add(new ToastPushNotificationMessage(MessageSendPriority.High)
                {
                    Title = ToastTitle,
                    SubTitle = ToastSubTitle
                });
            }

            if (IsRawEnabled)
            {
                // Prepare a raw push notification message.
                messages.Add(new RawPushNotificationMessage(MessageSendPriority.High)
                {
                    RawData = Encoding.ASCII.GetBytes(RawMessage)
                });
            }

            foreach (var subscriber in PushService.Subscribers)
            {
                messages.ForEach(m => m.SendAsync(subscriber.ChannelUri, Log, Log));
            }
        }

        #endregion

        #region Privates

        private void InitializeDefaults()
        {
            DisplayName = "One Time";
            Description = "This is the simplest push notification pattern of just pushing single time message to a registered client.";
            Count = 1;
            Title = "Game Update";
            ToastTitle = Title;
            ToastSubTitle = "Game has been released";
            RawMessage = ToastSubTitle;
            IsTileEnabled = true;
            IsToastEnabled = true;
            IsRawEnabled = true;
        }

        #endregion
    }
}
