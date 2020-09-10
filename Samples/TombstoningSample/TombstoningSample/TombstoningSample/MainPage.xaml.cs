using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Microsoft.Phone.Controls;
using System.Threading;
using System.IO;
using System.IO.IsolatedStorage;

namespace TombstoningSample
{
    public partial class MainPage : PhoneApplicationPage
    {
        // boolean used to track if the page is new, and therefore state needs to be restored.
        bool newPageInstance = false;

        // string used to represent the data used by the page. A real application will likely use a more
        // complex data structure, such as an XML document. The only requirement is that the object be serializable.
        string pageDataObject;


        /// <summary>
        /// Main page constructor.
        /// </summary>
        public MainPage()
        {
            InitializeComponent();

            // The constructor is not called if the page is already in memory and therefore page and application
            // state are still valid. Set newPageInstance to true so that in OnNavigatedTo, we know we need to 
            // recreate page and application state.
            newPageInstance = true;
        }

        /// <summary>
        /// Override of Page's OnNavigatedFrom method. Use helper class to preserve the visual state of the page's
        /// controls. Preserving application state takes place, if necessary, in the tombstoning event handlers
        /// in App.xaml.cs.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnNavigatedFrom(System.Windows.Navigation.NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);

            // Call the custom helper functions to preserve the state of the UI
            StateUtils.PreserveState(State, textBox1);
            StateUtils.PreserveState(State, checkBox1);
            StateUtils.PreserveState(State, scrollViewer1);
            StateUtils.PreserveState(State, slider1);
            StateUtils.PreserveState(State, radioButton1);
            StateUtils.PreserveState(State, radioButton2);
            StateUtils.PreserveState(State, radioButton3);
            StateUtils.PreserveState(State, radioButtonA);
            StateUtils.PreserveState(State, radioButtonB);
            StateUtils.PreserveState(State, radioButtonC);
            StateUtils.PreserveFocusState(State, ContentPanel);



            // Set a key in the State dictionary that will be checked for in OnNavigatedTo
            this.State["PreservingPageState"] = true;

            // Set newPageInstance back to false. It will be set back to true if the constructor is called again.
            newPageInstance = false;
        }

        /// <summary>
        /// Override of Page's OnNavigatedTo method. The page's visual state is restored. Then, the application
        /// state data is either set from the Application state dictionary or it is retrieved asynchronously.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            // Call the base implementation
            base.OnNavigatedTo(e);

            // If the constructor has been called AND the PreservingPageState key is in the State dictionary,
            // then the UI state for this page should be restored
            if (newPageInstance && this.State.ContainsKey("PreservingPageState"))
            {
                // Call the custom helper functions to restore the state of the UI
                StateUtils.RestoreState(State, textBox1, "");
                StateUtils.RestoreState(State, checkBox1, false);
                StateUtils.RestoreState(State, slider1, 0);
                StateUtils.RestoreState(State, radioButton1, false);
                StateUtils.RestoreState(State, radioButton2, false);
                StateUtils.RestoreState(State, radioButton3, false);
                StateUtils.RestoreState(State, radioButtonA, false);
                StateUtils.RestoreState(State, radioButtonB, false);
                StateUtils.RestoreState(State, radioButtonC, false);
                StateUtils.RestoreState(State, scrollViewer1);
                StateUtils.RestoreFocusState(State, ContentPanel);
            }


            // If this is a new page instance, the data must be retrieved in some way
            // If not, the page was already in memory and the data already exists
            if (newPageInstance)
            {

                // if the application member variable is empty, call the method that loads data.
                if ((Application.Current as TombstoningSample.App).AppDataObject == null)
                {
                    GetDataAsync();
                }
                else
                {
                    // Otherwise set the page's data object from the application member variable
                    pageDataObject = (Application.Current as TombstoningSample.App).AppDataObject;

                    // Show the data to the user
                    SetData(pageDataObject, "preserved state");
                }
            }

            // Set the new page instance to false. It will not be set to true 
            // unless the page constructor is called.
            newPageInstance = false;
        }


        /// <summary>
        /// This method is called from the OnNavigatedTo handler on the UI thread. 
        /// It creates a background thread and calls GetData which handles the data retrieval.
        /// </summary>
        public void GetDataAsync()
        {
            // Let the user know that data is being loaded in the background.
            statusTextBlock.Text = "loading data...";

            // Call the GetData method on a new thread.
            Thread t = new Thread(new ThreadStart(GetData));
            t.Start();

        }


        /// <summary>
        /// GetData is called on a background thread. If data is present in Isolated Storage, and its save
        /// date is recent, load the data from Isolated Storage. Otherwise, start an asynchronous http
        /// request to obtain fresh data.
        /// </summary>
        public void GetData()
        {
            // Check the time elapsed since data was last saved to Isolated Storage
            TimeSpan TimeSinceLastSave = TimeSpan.FromSeconds(0);
            if (IsolatedStorageSettings.ApplicationSettings.Contains("DataLastSave"))
            {
                DateTime dataLastSave = (DateTime)IsolatedStorageSettings.ApplicationSettings["DataLastSave"];
                TimeSinceLastSave = DateTime.Now - dataLastSave;
            }

            // Check to see if data exists in Isolated Storage and see if the data is fresh.
            // This example uses 30 seconds as the valid time window to make it easy to test. 
            // Real apps will use a larger window.
            IsolatedStorageFile isoStore = IsolatedStorageFile.GetUserStoreForApplication();
            if (isoStore.FileExists("myDataFile.txt") && TimeSinceLastSave.TotalSeconds < 30)
            {
                // This method loads the data from Isolated Storage, if it is available.
                StreamReader sr = new StreamReader(isoStore.OpenFile("myDataFile.txt", FileMode.Open));
                string data = sr.ReadToEnd();
                sr.Close();

                // Use the Dispatcher to call SetData on the UI thread, passing the retrieved data.
                Dispatcher.BeginInvoke(() => { SetData(data, "isolated storage"); });
            }
            else
            {
                // Otherwise it gets the data from the Web. 
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(new Uri("http://windowsteamblog.com/windows_phone/b/windowsphone/rss.aspx"));
                request.BeginGetResponse(HandleWebResponse, request);
            }


        }

        /// <summary>
        /// Event handler for the asynchronous Web response.
        /// </summary>
        /// <param name="result"></param>
        public void HandleWebResponse(IAsyncResult result)
        {
            // Put this in a try block in case the Web request was unsuccessful.
            try
            {
                // Get the request from the IAsyncResult
                HttpWebRequest request = (HttpWebRequest)(result.AsyncState);

                // Read the response stream from the response.
                StreamReader sr = new StreamReader(request.EndGetResponse(result).GetResponseStream());
                string data = sr.ReadToEnd();

                // Use the Dispatcher to call SetData on the UI thread, passing the retrieved data.
                Dispatcher.BeginInvoke(() => { SetData(data, "web"); });
            }
            catch 
            {
                // If the data request fails, alert the user
                Dispatcher.BeginInvoke(() => { statusTextBlock.Text = "Unable to get data from Web."; });
            }
        }

        /// <summary>
        /// If data was obtained asynchronously from the Web or from Isolated Storage, this method
        /// is invoked on the UI thread to update the page to show the data.
        /// </summary>
        /// <param name="data"></param>
        /// <param name="source"></param>
        public void SetData(string data, string source)
        {
            pageDataObject = data;

            // Show the data to the user
            dataTextBlock.Text = pageDataObject;

            // Set the Application class member variable to the data so that the
            // Application class can store it when the application is deactivated or closed.
            (Application.Current as TombstoningSample.App).AppDataObject = pageDataObject;

            statusTextBlock.Text = "data retrieved from " + source + ".";
        }

    }
}