using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;
using Realms;
using Realms.Sync;
using System.IO;
using System.Net;
using Plugin.DeviceInfo;

namespace DigiSign
{
    public partial class MainPage : ContentPage
    {
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
            InitializeComponent();

            _realmAppID = ResourceAP.AutoProvsionRealmAppID.ToString();
            _realmAPIKey = ResourceAP.AutoProvisionRealmAPIKey.ToString();
            txt_connection.Text = "..." + _realmAPIKey.Substring(_realmAPIKey.Length-5) + " @ " + _realmAppID;

            EnableSync();
        }

        public async void EnableSync()
        {
            txt_error.Text = "Starting...";
            // connect
            app = Realms.Sync.App.Create(_realmAppID);
            user = await app.LogInAsync(Credentials.ApiKey(_realmAPIKey));

            // get auto provision details
            var IpAddress = Dns.GetHostAddresses(Dns.GetHostName()).FirstOrDefault();
            txt_deviceID.Text = CrossDeviceInfo.Current.Id.ToString();
            txt_ipaddr.Text = IpAddress.ToString();

            var bsonval = await user.Functions.CallAsync("getMyPartitions", txt_deviceID.Text, txt_ipaddr.Text);
            _realmPartition = bsonval.ToString();

            if (_realmPartition.Length < 2)
            {
                txt_error.Text = "Registration API returned empty or null.";
                await Task.Delay(TimeSpan.FromSeconds(15000));
                EnableSync();
            }
            else
            {
                txt_error.Text = "Syncing signs...";

                // get actual signs
                config = new Realms.Sync.SyncConfiguration("GLOBAL", user);
                realm = await Realm.GetInstanceAsync(config);

                await realm.GetSession().WaitForDownloadAsync();

                allSigns = realm.All<Models.Sign>().OrderBy(sign => sign.Order);

                var token = realm.All<Models.Sign>().SubscribeForNotifications((sender, changes, error) =>
                {
                    allSigns.OrderBy(sign => sign.Order);
                    _currentIndex = 0;
                    //DisplayNext();
                });

                DisplayNext();
            }
        }

        async void DisplayNext()
        {

            if (allSigns == null)
            {
                ctr_configstack.IsVisible = true;
                ctr_view_image.IsVisible = false;
                ctr_view_web.IsVisible = false;

                txt_error.Text = "Registration success; screen list is null.";
            }
            else if (allSigns.Count() == 0)
            {
                ctr_configstack.IsVisible = true;
                ctr_view_image.IsVisible = false;
                ctr_view_web.IsVisible = false;

                txt_error.Text = "Registration success; screen count is " + allSigns.Count().ToString();
            }
            else
            {
                if (_currentIndex == allSigns.Count())
                {
                    _currentIndex = 0;
                    try
                    {
                        var bsonval = await user.Functions.CallAsync("getMyPartitions", txt_deviceID.Text, txt_ipaddr.Text);
                        var rps = bsonval.ToString();
                        if (rps.Length > 0)
                        {
                            _realmPartition = rps;
                        }
                    } catch(Exception ex)
                    {
                    }
                }

                var partitions = _realmPartition.Split(',').ToList();

                Models.Sign s = allSigns.OrderBy(sign => sign.Order).ElementAt(_currentIndex);

                if (s.Type == "hide")
                {
                    _currentIndex++;
                    DisplayNext();
                }
                // cant figure out linq syntax for this 
                else if (!partitions.Contains(s.Feed))
                {
                    _currentIndex++;
                    DisplayNext();
                }
                else
                {
                    ctr_configstack.IsVisible = false;
                    ctr_view_image.IsVisible = false;
                    ctr_view_web.IsVisible = false;
                    ctr_view_text.IsVisible = false;
                    ctr_view_media.IsVisible = false;

                    if (s.Type == "web")
                    {
                        ctr_view_web.IsVisible = true;
                        ctr_view_web.Source = new Uri(s.URI);

                    }
                    else if (s.Type == "video")
                    {
                        ctr_view_media.IsVisible = true;
                        ctr_view_media.Source = new Uri(s.URI);
                    }
                    else if (s.Type == "image")
                    {
                        ctr_view_image.IsVisible = true;
                        ctr_view_image.Source = new Uri(s.URI);
                    }
                    else if ((s.Type == "base64image") || (s.Type == "base64"))
                    {
                        ctr_view_image.IsVisible = true;

                        byte[] Base64Stream = Convert.FromBase64String(s.Text);
                        ctr_view_image.Source = ImageSource.FromStream(() => new MemoryStream(Base64Stream));


                    }
                    else if (s.Type == "text")
                    {
                        ctr_view_text.IsVisible = true;
                        ctr_view_text.Text = s.Text;
                    }

                    _currentTimer = Convert.ToInt32(s.Duration) * 1000;
                    await Task.Delay(_currentTimer);

                    _currentIndex++;
                    DisplayNext();
                }
            }

        }

        private void Button_Clicked(object sender, EventArgs e)
        {
            EnableSync();
        }
    }
}
