using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Diagnostics;
using System.Threading;

using WindowsPhone.Recipes.Push.Messasges.Properties;

namespace WindowsPhone.Recipes.Push.Messasges
{
    /// <summary>
    /// Represents a base class for push notification messages.
    /// </summary>
    /// <remarks>
    /// This class members are thread safe.
    /// </remarks>
    public abstract class PushNotificationMessage
    {
        #region Constants

        /// <value>Push notification maximum message size including headers and payload.</value>
        protected const int MaxMessageSize = 1024;

        /// <summary>
        /// Well known push notification message web request headers.
        /// </summary>
        internal static class Headers
        {
            public const string MessageId = "X-MessageID";
            public const string BatchingInterval = "X-NotificationClass";
            public const string NotificationStatus = "X-NotificationStatus";
            public const string DeviceConnectionStatus = "X-DeviceConnectionStatus";
            public const string SubscriptionStatus = "X-SubscriptionStatus";
            public const string WindowsPhoneTarget = "X-WindowsPhone-Target";
        }

        #endregion

        #region Fields

        /// <value>Synchronizes payload manipulations.</value>
        private readonly object _sync = new object();

        /// <value>The payload raw bytes of this message.</value>
        private byte[] _payload;

        private MessageSendPriority _sendPriority;

        #endregion

        #region Properties

        /// <summary>
        /// Gets this message unique ID.
        /// </summary>
        public Guid Id { get; private set; }

        /// <summary>
        /// Gets or sets the send priority of this message in the MPNS.
        /// </summary>
        public MessageSendPriority SendPriority
        {
            get
            {
                return _sendPriority;
            }

            set
            {
                SafeSet(ref _sendPriority, value);
            }
        }

        /// <summary>
        /// Gets or sets the message payload.
        /// </summary>
        protected byte[] Payload
        {
            get
            {
                return _payload;
            }

            set
            {
                SafeSet(ref _payload, value);
            }
        }

        protected abstract int NotificationClassId
        {
            get;
        }

        /// <summary>
        /// Gets or sets the flag indicating that one of the message properties
        /// has changed, thus the payload should be rebuilt.
        /// </summary>
        private bool IsDirty { get; set; }

        #endregion

        #region Ctor

        /// <summary>
        /// Initializes a new instance of this type with <see cref="WindowsPhone.Recipes.Push.Messasges.MessageSendPriority.Normal"/> send priority.
        /// </summary>
        protected PushNotificationMessage(MessageSendPriority sendPriority = MessageSendPriority.Normal)
        {
            Id = Guid.NewGuid();
            SendPriority = sendPriority;
            IsDirty = true;
        }

        #endregion

        #region Operations

        /// <summary>
        /// Synchronously send this messasge to the destination address.
        /// </summary>
        /// <remarks>
        /// Note that properties of this instance may be changed by different threads while
        /// sending, but once the payload created, it won't be changed until the next send.
        /// </remarks>
        /// <param name="uri">Destination address uri.</param>
        /// <exception cref="ArgumentNullException">One of the arguments is null.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Payload size is out of range. For maximum allowed message size see <see cref="PushNotificationMessage.MaxPayloadSize"/></exception>
        /// <exception cref="MessageSendException">Failed to send message for any reason.</exception>
        /// <returns>The result instance with relevant information for this send operation.</returns>
        public MessageSendResult Send(Uri uri)
        {
            Guard.ArgumentNotNull(uri, "uri");

            // Create payload or reuse cached one.
            var payload = GetOrCreatePayload();

            // Create and initialize the request object.
            var request = CreateWebRequest(uri, payload);

            var result = SendSynchronously(payload, uri, request);
            return result;
        }

        /// <summary>
        /// Asynchronously send this messasge to the destination address.
        /// </summary>
        /// <remarks>
        /// This method uses the .NET Thread Pool. Use this method to send one or few
        /// messages asynchronously. If you have many messages to send, please consider
        /// of using the synchronous method with custom (external) queue-thread solution.
        /// 
        /// Note that properties of this instance may be changed by different threads while
        /// sending, but once the payload created, it won't be changed until the next send.
        /// </remarks>
        /// <param name="uri">Destination address uri.</param>
        /// <param name="messageSent">Message sent callback.</param>
        /// <param name="messageError">Message send error callback.</param>
        /// <exception cref="ArgumentNullException">One of the arguments is null.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Payload size is out of range. For maximum allowed message size see <see cref="PushNotificationMessage.MaxPayloadSize"/></exception>        
        public void SendAsync(Uri uri, Action<MessageSendResult> messageSent = null, Action<MessageSendResult> messageError = null)
        {
            Guard.ArgumentNotNull(uri, "uri");

            // Create payload or reuse cached one.
            var payload = GetOrCreatePayload();

            // Create and initialize the request object.
            var request = CreateWebRequest(uri, payload);

            SendAsynchronously(
                payload,
                uri,
                request,
                messageSent ?? (result => { }),
                messageError ?? (result => { }));
        }

        #endregion

        #region Protected & Virtuals

        /// <summary>
        /// Override to create the message payload.
        /// </summary>
        /// <returns>The messasge payload bytes.</returns>
        protected virtual byte[] OnCreatePayload()
        {
            return _payload;
        }

        /// <summary>
        /// Override to initialize the message web request with custom headers.
        /// </summary>
        /// <param name="request">The message web request.</param>
        protected virtual void OnInitializeRequest(HttpWebRequest request)
        {
        }

        /// <summary>
        /// Check the size of the payload and reject it if too big.
        /// </summary>
        /// <param name="payload">Payload raw bytes.</param>
        protected abstract void VerifyPayloadSize(byte[] payload);

        /// <summary>
        /// Safely set oldValue with newValue in case that are different, and raise the dirty flag.
        /// </summary>
        /// <typeparam name="T">The type of the value.</typeparam>
        /// <param name="oldValue">The old value.</param>
        /// <param name="newValue">The new value.</param>
        protected void SafeSet<T>(ref T oldValue, T newValue)
        {
            lock (_sync)
            {
                if (!object.Equals(oldValue, newValue))
                {
                    oldValue = newValue;
                    IsDirty = true;
                }
            }
        }

        #endregion

        #region Privates

        /// <summary>
        /// Synchronously send this message to the destination uri.
        /// </summary>
        /// <param name="payload">The message payload bytes.</param>
        /// <param name="uri">The message destination uri.</param>
        /// <param name="payload">Initialized Web request instance.</param>
        /// <returns>The result instance with relevant information for this send operation.</returns>
        private MessageSendResult SendSynchronously(byte[] payload, Uri uri, HttpWebRequest request)
        {
            try
            {
                // Get the request stream.
                using (var requestStream = request.GetRequestStream())
                {
                    // Start to write the payload to the stream.
                    requestStream.Write(payload, 0, payload.Length);

                    // Switch to receiving the response from MPNS.
                    using (var response = (HttpWebResponse)request.GetResponse())
                    {
                        var result = new MessageSendResult(this, uri, response);
                        if (response.StatusCode != HttpStatusCode.OK)
                        {
                            throw new InvalidOperationException(string.Format(Resources.ServerErrorStatusCode, response.StatusCode));
                        }

                        return result;
                    }
                }
            }
            catch (WebException ex)
            {
                var result = new MessageSendResult(this, uri, ex);
                throw new MessageSendException(result, ex);
            }
            catch (Exception ex)
            {
                var result = new MessageSendResult(this, uri, ex);
                throw new MessageSendException(result, ex);
            }
        }

        /// <summary>
        /// Asynchronously send this message to the destination uri using the HttpWebRequest context.
        /// </summary>
        /// <param name="payload">The message payload bytes.</param>
        /// <param name="uri">The message destination uri.</param>
        /// <param name="payload">Initialized Web request instance.</param>
        /// <param name="sent">Message sent callback.</param>
        /// <param name="error">Message send error callback.</param>
        /// <returns>The result instance with relevant information for this send operation.</returns>
        private void SendAsynchronously(byte[] payload, Uri uri, HttpWebRequest request, Action<MessageSendResult> sent, Action<MessageSendResult> error)
        {
            try
            {
                // Get the request stream asynchronously.
                request.BeginGetRequestStream(requestAsyncResult =>
                {
                    try
                    {
                        using (var requestStream = request.EndGetRequestStream(requestAsyncResult))
                        {
                            // Start writing the payload to the stream.
                            requestStream.Write(payload, 0, payload.Length);
                        }

                        // Switch to receiving the response from MPNS asynchronously.
                        request.BeginGetResponse(responseAsyncResult =>
                        {
                            try
                            {
                                using (var response = (HttpWebResponse)request.EndGetResponse(responseAsyncResult))
                                {
                                    var result = new MessageSendResult(this, uri, response);
                                    if (response.StatusCode == HttpStatusCode.OK)
                                    {
                                        sent(result);
                                    }
                                    else
                                    {
                                        error(result);
                                    }
                                }
                            }
                            catch (Exception ex3)
                            {
                                error(new MessageSendResult(this, uri, ex3));
                            }                            
                        }, null);
                    }
                    catch (Exception ex2)
                    {
                        error(new MessageSendResult(this, uri, ex2));                        
                    }
                }, null);
            }
            catch (Exception ex1)
            {
                error(new MessageSendResult(this, uri, ex1));                
            }
        }

        /// <summary>
        /// Create a payload and verify its size.
        /// </summary>
        /// <returns>Payload raw bytes.</returns>
        private byte[] GetOrCreatePayload()
        {
            if (IsDirty)
            {
                lock (_sync)
                {
                    if (IsDirty)
                    {
                        var payload = OnCreatePayload() ?? new byte[0];
                        DebugOutput(payload);
                        VerifyPayloadSize(payload);

                        _payload = payload;

                        IsDirty = false;
                    }
                }
            }

            return _payload;
        }

        private HttpWebRequest CreateWebRequest(Uri uri, byte[] payload)
        {
            var request = (HttpWebRequest)WebRequest.Create(uri);
            request.Method = WebRequestMethods.Http.Post;
            request.ContentType = "text/xml; charset=utf-8";
            request.ContentLength = payload.Length;
            request.Headers[Headers.MessageId] = Id.ToString();

            // Batching interval is composed of the message priority and the message class id.
            int batchingInterval = ((int)SendPriority * 10) + NotificationClassId;
            request.Headers[Headers.BatchingInterval] = batchingInterval.ToString();

            OnInitializeRequest(request);

            return request;
        }

        #endregion

        #region Diagnostics

        [Conditional("DEBUG")]
        private static void DebugOutput(byte[] payload)
        {
            string payloadString = Encoding.ASCII.GetString(payload);
            Debug.WriteLine(payloadString);
        }

        #endregion
    }
}
