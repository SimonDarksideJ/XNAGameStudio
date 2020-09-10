using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using System.Windows.Threading;
using System.ComponentModel.Composition;

using WindowsPhone.Recipes.Push.Messasges;
using WindowsPhone.Recipes.Push.Server.Models;

namespace WindowsPhone.Recipes.Push.Server.ViewModels
{
    [Export, Export(typeof(IMessageSendResultLogger)), PartCreationPolicy(CreationPolicy.Shared)]
    internal class MessageStatusViewModel : ViewModelBase, IMessageSendResultLogger
    {
        private readonly ObservableCollection<MessageStatus> _status = new ObservableCollection<MessageStatus>();

        public ObservableCollection<MessageStatus> Status
        {
            get { return _status; }
        }

        void IMessageSendResultLogger.Log(string patternName, MessageSendResult result)
        {
            Dispatcher.BeginInvoke(() => Status.Add(new MessageStatus(patternName, result)));
        }

        void IMessageSendResultLogger.Log(string patternName, MessageSendException exception)
        {
            Dispatcher.BeginInvoke(() => Status.Add(new MessageStatus(patternName, exception.Result)));
        }
    }
}
