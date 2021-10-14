using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Realms;
using Realms.Sync;
using MongoDB.Bson;

using System.Threading.Tasks;
using Windows.UI.ViewManagement;
using Windows.UI;
using Windows.UI.Xaml.Media.Imaging;
using Windows.ApplicationModel.Resources;

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

        public Realms.Sync.App app { get; set; }
        public Realms.Sync.User user { get; set; }
        public Realms.Sync.SyncConfiguration config { get; set; }
        public Realm realm { get; set; }
        public string partition { get; set; }
        private IQueryable<Models.Sign> allSigns = null;
        private List<Models.Sign> allSignsList = null;

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

            _disTimer = new DispatcherTimer();
            _disTimer.Tick += _disTimer_Tick;

            EnableSync();
        }

        public async Task EnableSync()
        {
            app = Realms.Sync.App.Create(_realmAppID);
            user = await app.LogInAsync(Realms.Sync.Credentials.ApiKey(_realmAPIKey));
            //partition = $"user={ user.Id }";
            partition = "ALL";
            config = new Realms.Sync.SyncConfiguration(partition, user);
            realm = await Realm.GetInstanceAsync(config);
            allSigns = realm.All<Models.Sign>();

            var token = realm.All<Models.Sign>().SubscribeForNotifications((sender, changes, error) =>
            {
                allSigns.OrderBy(sign => sign.Order);
                DisplayNext();
            });
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

                txt_error.Text = "Size is null";
            }
            else if (allSigns.Count() == 0)
            {
                ctr_configstack.Visibility = Visibility.Visible;
                ctr_view_image.Visibility = Visibility.Collapsed;
                ctr_view_media.Visibility = Visibility.Collapsed;
                ctr_view_text.Visibility = Visibility.Collapsed;
                ctr_view_web.Visibility = Visibility.Collapsed;

                txt_error.Text = "Count is " + allSigns.Count().ToString();
            }
            else
            { 
                if (_currentIndex == allSigns.Count())
                {
                    _currentIndex = 0;
                }

                allSignsList = allSigns.ToList();

                Models.Sign s = allSignsList[_currentIndex];

                if (s.Type == "hide")
                {
                    _currentIndex = _currentIndex + 1;
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

                    _currentIndex = _currentIndex + 1;
                }
            }

        }
    }
}
