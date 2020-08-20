using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.ServiceModel;

using WindowsPhone.Recipes.Push.Server.Services;

namespace WindowsPhone.Recipes.Push.Server.ViewModels
{
    [Export, PartCreationPolicy(CreationPolicy.Shared)]
    internal class MainViewModel : ViewModelBase
    {
        private PushService _pushService;
        private ImageService _imageService;
        private PushPatternViewModel _activePattern;

        [ImportingConstructor]
        public MainViewModel([ImportMany(typeof(PushPatternViewModel))] IEnumerable<PushPatternViewModel> pushPatterns)
        {
            PushPatterns = pushPatterns;
            ActivePattern = pushPatterns.FirstOrDefault();
        }

        [Import]
        private PushService PushService
        {
            get { return _pushService; }
            set
            {
                _pushService = value;
                _pushService.Subscribed += (s, e) => NotifyPropertyChanged("Subscribers");
                _pushService.Host();
            }
        }

        [Import]
        private ImageService ImageService
        {
            get { return _imageService; }
            set
            {
                _imageService = value;
                _imageService.Host();
            }
        }

        public IEnumerable<PushPatternViewModel> PushPatterns { get; private set; }

        [Import]
        public MessageStatusViewModel MessageStatus { get; private set; }

        public PushPatternViewModel ActivePattern
        {
            get { return _activePattern; }

            set
            {
                if (_activePattern != value)
                {
                    if (_activePattern != null)
                    {
                        // Deactivate old pattern.
                        _activePattern.IsActive = false;
                    }

                    _activePattern = value;
                    if (_activePattern != null)
                    {
                        // Activate new pattern.
                        _activePattern.IsActive = true;
                    }

                    NotifyPropertyChanged("ActivePattern");
                }
            }
        }

        public int Subscribers
        {
            get { return PushService.SubscribersCount; }
        }
    }
}
