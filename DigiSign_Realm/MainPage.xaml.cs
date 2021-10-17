using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Realms;
using Realms.Sync;
using System.Threading.Tasks;
using Windows.UI.ViewManagement;
using Windows.UI;
using Windows.UI.Xaml.Media.Imaging;
using Windows.ApplicationModel.Resources;
using Windows.Security.ExchangeActiveSyncProvisioning;
using Windows.Networking;
using Windows.Networking.Connectivity;
using Windows.Storage.Streams;
using Windows.UI.Xaml.Media;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace DigiSign_Realm
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        DispatcherTimer _disTimer;
        int _currentIndex = 0;
        int _currentTimer = 0;
        string _realmAppID = "";
        string _realmAPIKey = "";
        string _realmPartition = "";

        public Realms.Sync.App app { get; set; }
        public Realms.Sync.User user { get; set; }
        public Realms.Sync.SyncConfiguration config { get; set; }
        public Realm realm { get; set; }
        public string partition { get; set; }
        private IQueryable<Models.Sign> allSigns = null;
        private List<Models.Sign> allSignsList = null;
        private List<IDisposable> _tokens = null;

        public MainPage()
        {
            this.InitializeComponent();

            var view = ApplicationView.GetForCurrentView();
            view.TitleBar.BackgroundColor = Color.FromArgb(255, 11, 140, 26);
            view.TitleBar.ButtonBackgroundColor = Color.FromArgb(255, 11, 140, 26);
            view.TitleBar.ButtonForegroundColor = Colors.White;
            view.TitleBar.ButtonPressedForegroundColor = Color.FromArgb(255, 11, 140, 26);
            view.TitleBar.ButtonPressedBackgroundColor = Colors.White;
            view.TitleBar.ButtonHoverBackgroundColor = Colors.White;
            view.TitleBar.ButtonHoverForegroundColor = Color.FromArgb(255, 11, 140, 26);

            var resources = new ResourceLoader("Resources");
            _realmAppID = resources.GetString("AutoProvsionRealmAppID");
            _realmAPIKey = resources.GetString("AutoProvisionRealmAPIKey");
            //_realmPartition = resources.GetString("AutoProvisionRealmParition");

            _disTimer = new DispatcherTimer();
            _disTimer.Tick += _disTimer_Tick;

            EnableSync();
        }

        public async Task EnableSync()
        {
            // https://stackoverflow.com/questions/31746613/how-do-i-get-a-unique-identifier-for-a-device-within-windows-10-universal
            var deviceInformation = new EasClientDeviceInformation();
            string deviceID = deviceInformation.FriendlyName.ToString() + "_" + deviceInformation.Id.ToString();
            txt_deviceID.Text = deviceID;

            // https://stackoverflow.com/questions/33770429/how-do-i-find-the-local-ip-address-on-a-win-10-uwp-project
            foreach (HostName localHostName in NetworkInformation.GetHostNames())
            {
                if (localHostName.IPInformation != null)
                {
                    if (localHostName.Type == HostNameType.Ipv4)
                    {
                        txt_ipaddr.Text = localHostName.ToString();
                        break;
                    }
                }
            }

            // connect
            app = Realms.Sync.App.Create(_realmAppID);
            user = await app.LogInAsync(Realms.Sync.Credentials.ApiKey(_realmAPIKey));

            // get auto provision details
            var bsonval = await user.Functions.CallAsync("getMyPartitions", deviceID);
            _realmPartition = bsonval.ToString();

            if (_realmPartition.Length < 2)
            {
                txt_error.Text = "Registration API returned empty or null.";
                await Task.Delay(TimeSpan.FromSeconds(15));
                EnableSync();
            }
            else
            {
                txt_error.Text = "Syncing signs...";

                // get actual signs
                config = new Realms.Sync.SyncConfiguration("GLOBAL", user);
                realm = await Realm.GetInstanceAsync(config);

                await realm.GetSession().WaitForDownloadAsync();

                allSigns = realm.All<Models.Sign>();

                var token = realm.All<Models.Sign>().SubscribeForNotifications((sender, changes, error) =>
                {
                    allSigns.OrderBy(sign => sign.Order);
                    DisplayNext();
                });
            }
        }

        private void _disTimer_Tick(object sender, object e)
        {
            DisplayNext();
        }

        async void DisplayNext()
        {

            if (allSigns == null)
            {
                ctr_configstack.Visibility = Visibility.Visible;
                ctr_view_image.Visibility = Visibility.Collapsed;
                ctr_view_media.Visibility = Visibility.Collapsed;
                ctr_view_text.Visibility = Visibility.Collapsed;
                ctr_view_web.Visibility = Visibility.Collapsed;

                txt_error.Text = "Registration success; screen list is null.";
            }
            else if (allSigns.Count() == 0)
            {
                ctr_configstack.Visibility = Visibility.Visible;
                ctr_view_image.Visibility = Visibility.Collapsed;
                ctr_view_media.Visibility = Visibility.Collapsed;
                ctr_view_text.Visibility = Visibility.Collapsed;
                ctr_view_web.Visibility = Visibility.Collapsed;

                txt_error.Text = "Registration success; screen count is " + allSigns.Count().ToString();
            }
            else
            { 
                if (_currentIndex == allSigns.Count())
                {
                    _currentIndex = 0;
                }

                var partitions = _realmPartition.Split(",").ToList();

                Models.Sign s = allSigns.OrderBy(sign => sign.Order).ElementAt(_currentIndex);

                if (s.Type == "hide")
                {
                    _currentIndex++;
                    DisplayNext();
                } 
                // cant figure out linq syntax for this 
                else if (!partitions.Contains(s.Feed)) {
                    _currentIndex++;
                    DisplayNext();
                }
                else
                {
                    ctr_configstack.Visibility = Visibility.Collapsed;
                    if (s.Type == "web")
                    {
                        ctr_view_web.Visibility = Visibility.Visible;
                        ctr_view_image.Visibility = Visibility.Collapsed;
                        ctr_view_text.Visibility = Visibility.Collapsed;
                        ctr_view_media.Visibility = Visibility.Collapsed;

                        ctr_view_web.Visibility = Visibility.Visible;
                        ctr_view_web.Navigate(new Uri(s.URI));

                    }
                    if (s.Type == "video")
                    {
                        ctr_view_web.Visibility = Visibility.Collapsed;
                        ctr_view_image.Visibility = Visibility.Collapsed;
                        ctr_view_text.Visibility = Visibility.Collapsed;
                        ctr_view_media.Visibility = Visibility.Visible;

                        ctr_view_media.Source = new Uri(s.URI);

                    }
                    else if (s.Type == "image")
                    {
                        ctr_view_image.Visibility = Visibility.Visible;
                        ctr_view_web.Visibility = Visibility.Collapsed;
                        ctr_view_text.Visibility = Visibility.Collapsed;
                        ctr_view_media.Visibility = Visibility.Collapsed;

                        BitmapImage imageSource = new BitmapImage(new Uri(s.URI));
                        ctr_view_image.Width = imageSource.DecodePixelHeight = (int)this.ActualWidth;
                        ctr_view_image.Source = imageSource;
                    }
                    else if (s.Type == "base64image")
                    {
                        ctr_view_image.Visibility = Visibility.Visible;
                        ctr_view_web.Visibility = Visibility.Collapsed;
                        ctr_view_text.Visibility = Visibility.Collapsed;
                        ctr_view_media.Visibility = Visibility.Collapsed;

                        var ims = new InMemoryRandomAccessStream();
                        var bytes = Convert.FromBase64String(s.Text);
                        var dataWriter = new DataWriter(ims);
                        dataWriter.WriteBytes(bytes);
                        await dataWriter.StoreAsync();
                        ims.Seek(0);
                        var imageSource = new BitmapImage();
                        imageSource.SetSource(ims);
                        ctr_view_image.Width = imageSource.DecodePixelHeight = (int)this.ActualWidth;
                        ctr_view_image.Source = imageSource;

                    }
                    else if (s.Type == "text")
                    {
                        ctr_view_text.Visibility = Visibility.Visible;
                        ctr_view_web.Visibility = Visibility.Collapsed;
                        ctr_view_image.Visibility = Visibility.Collapsed;
                        ctr_view_media.Visibility = Visibility.Collapsed;

                        ctr_view_text.Document.SetText(Windows.UI.Text.TextSetOptions.None, s.Text);
                    }

                    _currentTimer = Convert.ToInt32(s.Duration);
                    _disTimer.Interval = new TimeSpan(0, 0, _currentTimer);
                    _disTimer.Start();

                    _currentIndex++;
                }
            }

        }

        private void ctr_view_image_ImageFailed(object sender, ExceptionRoutedEventArgs e)
        {
            _currentIndex++;
            DisplayNext();
        }

        private void ctr_view_web_NavigationFailed(object sender, WebViewNavigationFailedEventArgs e)
        {
            _currentIndex++;
            DisplayNext();
        }
    }
}
