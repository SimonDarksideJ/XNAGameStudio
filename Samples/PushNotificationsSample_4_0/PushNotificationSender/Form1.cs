//-----------------------------------------------------------------------------
// Form1.cs
//
// Microsoft Advanced Technology Group
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------

using System;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Xml;
using System.Runtime.Serialization.Formatters.Binary;

namespace PushNotificationSender
{
    /// <summary>
    /// This application contains a single window with UI to send test notifications
    /// to a URI supplied by the user.
    /// </summary>
    public partial class Form1 : Form
    {
        PushNotificationSender pushSender;

        public Form1()
        {
            InitializeComponent();

            pushSender = new PushNotificationSender();
            pushSender.NotificationSendCompleted += PushSender_NotificationSendCompleted;
        }


        private delegate void UpdateServerResponseDelegate(string text);
        private void UpdateServerResponse(string text)
        {
            serverResponse.Text = text;
        }


        /// <summary>
        /// Callback that asynchronously updates the UI with the server's response or error message.
        /// </summary>
        private void PushSender_NotificationSendCompleted(PushNotificationCallbackArgs args)
        {
            string text = "Status Code: " + args.StatusCode.ToString() + Environment.NewLine +
                "TimeStamp: " + args.Timestamp.ToString() + Environment.NewLine +
                "Notification Type: " + args.NotificationType.ToString() + Environment.NewLine +
                "Notification Status: " + args.NotificationStatus + Environment.NewLine +
                "Device Status: " + args.DeviceConnectionStatus + Environment.NewLine +
                "Subscription Status: " + args.DeviceConnectionStatus;

            if (serverResponse.InvokeRequired)
            {
                serverResponse.Invoke(new UpdateServerResponseDelegate(UpdateServerResponse), new object[] { text });
            }
            else
            {
                UpdateServerResponse(text);
            }
        }


        /// <summary>
        /// The user has clicked the button to send a raw notification.
        /// </summary>
        private void buttonSendRaw_Click(object sender, EventArgs e)
        {
            // Send a raw notification.  A raw notification is just a stream of bytes,
            // so the server and client must agree on a format.  In this case,
            // it's just simple string of text.
            MemoryStream stream = new MemoryStream();

            BinaryWriter writer = new BinaryWriter(stream, Encoding.UTF8);
            writer.Write(rawText.Text);

            byte[] payload = stream.ToArray();

            try
            {
                Uri deviceUri = new Uri(phoneURI.Text);
                pushSender.SendRawNotification(deviceUri, payload);
            }
            catch (UriFormatException ex)
            {
                serverResponse.Text = ex.Message;
            }
        }


        /// <summary>
        /// The user has clicked the button to send a tile notification.
        /// </summary>
        private void buttonSendTile_Click(object sender, EventArgs e)
        {
            // Send the tile data from the UI.  
            
            // The client application project contains two tile images:
            //  - tile.png
            //  - tileUpdate.png
            // Sending either of these as an image URI from the sender application
            // will cause the tile background to update on the phone if the user has
            // pinned the application shortcut to their quicklaunch menu.

            int count = Int32.Parse(tileCount.Text);
            tileCount.Text = (count + 1).ToString();

            try
            {
                Uri deviceUri = new Uri(phoneURI.Text);
                pushSender.SendTileNotification(deviceUri, tileTitle.Text, count, tileBackgroundImageUri.Text);
            }
            catch (UriFormatException ex)
            {
                serverResponse.Text = ex.Message;
            }
        }


        /// <summary>
        /// The user has clicked the button to send a toast notification.
        /// </summary>
        private void buttonSendToast_Click(object sender, EventArgs e)
        {
            // Send the toast data from the UI.
            try
            {
                Uri deviceUri = new Uri(phoneURI.Text);
                pushSender.SendToastNotification(deviceUri, toastText1.Text, toastText2.Text);
            }
            catch (UriFormatException ex)
            {
                serverResponse.Text = ex.Message;
            }
            catch (ArgumentNullException ex)
            {
                serverResponse.Text = ex.Message;
            }
        }
    }
}
