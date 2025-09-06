using ImageSliderWPF.Helper;
using ImageSliderWPF.Models;
using OpenWeatherMap;
using Svg2Xaml;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.ServiceModel.Syndication;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using System.Xml;
using System.Xml.Serialization;

namespace ImageSliderWPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private int m_RSSMaxItems = 20;
        private double m_RSSSpeed = 5;
        private int m_RssUpdateInterval = 5; //min
        private ObservableCollection<RSSItem> m_RSSItems = new ObservableCollection<RSSItem>();
        private List<SyndicationItem> m_OriginalRssItems = new List<SyndicationItem>();
        List<WeatherData> lstWeatherData = new List<WeatherData>();
        private string m_RssHeading = "BBC URDU: International ";
        private string m_RssUrl = "http://feeds.bbci.co.uk/urdu/rss.xml#sa-link_location=story-body&intlink_from_url=http%3A%2F%2Fwww.bbc.com%2Furdu%2Finstitutional%2F2009%2F03%2F090306_rss_feed&intlink_ts=1523693531553-sa";
        //private string m_RssUrl = "https://feeds.feedburner.com/geo/GiKR";
        DispatcherTimer rssUpdateTimer;
        private int m_UpdateWeatherInterval = 5;

        private string m_ImageFolderPath = @"C:\Images";
        private int m_MaxImagesInSlider = 200;

        private int visibleView = 0;
        private int Count = 0;
        private int CurrentSourceIndex = 0, CurrentCtrlIndex = 0, oldCtrlIndex = 0, IntervalTimer = 7, FadeTimer = 5;



        DispatcherTimer timerImageChange;
        DispatcherTimer m_WeatherUpdateTimer;
        DispatcherTimer m_ClockTimer;

        List<string> m_ImageDirectoryPathList;


        public MainWindow()
        {
            InitializeComponent();
            KeepAlive();           
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            InitializeClocktimer();
            Task.Delay(500);
            InitializeRSS();
            Task.Delay(500);
            InitializeWeather();
            Task.Delay(500);
            InitializeImageSlider();
        }

        void InitializeClocktimer()
        {
            m_ClockTimer = new DispatcherTimer();
            m_ClockTimer.Interval = new TimeSpan(0, 0, 0);
            m_ClockTimer.Tick += M_ClockTimer_Tick;
            m_ClockTimer.Start();
        }

        private void M_ClockTimer_Tick(object sender, EventArgs e)
        {
            v_Time.Text = DateTime.Now.ToString("hh:mm tt", System.Globalization.CultureInfo.InvariantCulture);
            v_Date.Text = DateTime.Now.ToString("dd-MM-yyyy");
        }

        private async void InitializeImageSlider()
        {
            try
            {
                m_ImageDirectoryPathList = await ReadImages();
                if (m_ImageDirectoryPathList != null && m_ImageDirectoryPathList.Count > 0)
                {
                    timerImageChange = new DispatcherTimer();
                    timerImageChange.Interval = new TimeSpan(0, 0, IntervalTimer);
                    timerImageChange.Tick += TimerImageChange_Tick;
                    timerImageChange.Start();
                }
            }
            catch (Exception ex)
            {
                //MessageBox.Show(ex.Message);
            }
        }

        private void InitializeRSS()
        {
            try
            {
                //RSS
                rssUpdateTimer = new DispatcherTimer();
                rssUpdateTimer.Interval = new TimeSpan(0, m_RssUpdateInterval, 0);
                rssUpdateTimer.Tick += rssUpdateTimer_Tick;
                rssUpdateTimer.Start();

                sliderText.Text = "";
                contentTicker.Rate = m_RSSSpeed;
                contentTicker.Direction = KoderHack.WPF.Controls.TickerDirection.West;

                LoadRSS(m_RssHeading, m_RssUrl);
            }
            catch (Exception ex)
            {
                //MessageBox.Show(ex.Message);
                if (Items != null && Items.Count > 0)
                {
                    v_TickerGrid.Visibility = Visibility.Visible;
                }
                else
                {
                    v_TickerGrid.Visibility = Visibility.Collapsed;
                }
            }
        }

        private void LoadXMLFile()
        {
            try
            {
                Serializer ser = new Serializer();
                string path = string.Empty;
                string xmlInputData = string.Empty;
                string xmlOutputData = string.Empty;

                // EXAMPLE 1
                path = Directory.GetCurrentDirectory() + @"\settings.xml";
                xmlInputData = File.ReadAllText(path);


                RSS rss = ser.Deserialize<RSS>(xmlInputData);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        void rssUpdateTimer_Tick(object sender, EventArgs e)
        {
            try
            {
                LoadRSS(m_RssHeading, m_RssUrl);
            }
            catch (Exception ex)
            {
                if (Items != null && Items.Count > 0)
                {
                    v_TickerGrid.Visibility = Visibility.Visible;
                }
                else
                {
                    v_TickerGrid.Visibility = Visibility.Collapsed;
                }
            }
        }

        public ObservableCollection<RSSItem> Items
        {
            get
            {
                return m_RSSItems;
            }
        }
        public List<SyndicationItem> OriginalRssItems
        {
            get
            {
                return m_OriginalRssItems;
            }
        }

        public RSSSource Source
        {
            get;
            private set;
        }


        private async Task<List<string>> ReadImages()
        {
            try
            {
                List<string> imagePathList = Directory.GetFiles(m_ImageFolderPath, "*.jpg*", SearchOption.AllDirectories).ToList()
                    .Union(Directory.GetFiles(m_ImageFolderPath, "*.jpg*", SearchOption.AllDirectories))
                    .Union(Directory.GetFiles(m_ImageFolderPath, "*.jpeg*", SearchOption.AllDirectories))
                    .Union(Directory.GetFiles(m_ImageFolderPath, "*.png*", SearchOption.AllDirectories))
                    .ToList();

                return imagePathList;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            return null;
        }
        private void TimerImageChange_Tick(object sender, EventArgs e)
        {
            try
            {
                Count++;

                if (PlaySlideShow(m_ImageDirectoryPathList))
                {
                    Count++;
                }
                else
                {
                    MessageBox.Show("PlaySlideShow=False");
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        bool PlaySlideShow(List<string> imagePathList)
        {
            try
            {

                if (CurrentCtrlIndex == 0)
                {
                    oldCtrlIndex = 0;
                }
                else
                {
                    oldCtrlIndex = CurrentCtrlIndex;
                    CurrentCtrlIndex = (CurrentCtrlIndex + 1) % 2;
                    CurrentSourceIndex = (CurrentSourceIndex + 1) % imagePathList.Count;
                }

                string filePath = imagePathList[CurrentSourceIndex];
                FileStream imageStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read | FileShare.Delete);
                BitmapImage imageSource = new BitmapImage();
                imageSource.BeginInit();
                imageSource.StreamSource = imageStream;
                imageSource.EndInit();

                ImageSource newSource = imageSource;
                SetImage(newSource, TimeSpan.FromSeconds(FadeTimer));

                //File.Delete(filePath);

                //Reclaim memory
                imageSource = null;
                GC.Collect();
                GC.WaitForPendingFinalizers();
                GC.Collect();


                if (CurrentCtrlIndex == 0)
                {
                    CurrentCtrlIndex++;
                }

                return true;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        private void SetImage(ImageSource newImageSource, TimeSpan fadeInDuration)
        {
            var fadeInAnimation = new DoubleAnimation(1d, fadeInDuration);
            fadeInAnimation.Completed += (o, e) =>
            {
                if (visibleView == 1)
                {
                    myImage2.Opacity = 0;
                    myImage2.Visibility = Visibility.Hidden;
                }
                else if (visibleView == 2)
                {
                    myImage.Opacity = 0;
                    myImage.Visibility = Visibility.Hidden;
                }
            };

            if (visibleView == 0)
            {
                myImage.Source = newImageSource;
                myImage.Opacity = 1;
                myImage.Visibility = Visibility.Visible;
                visibleView = 1;
            }
            else if (visibleView == 1)
            {
                Panel.SetZIndex(myImage2, 8);
                Panel.SetZIndex(myImage, 0);
                myImage2.Source = newImageSource;
                myImage2.Opacity = 0;
                fadeInAnimation.From = 0;
                fadeInAnimation.To = 1;
                fadeInAnimation.Duration = fadeInDuration;
                visibleView = 2;
                myImage2.Visibility = Visibility.Visible;
                myImage2.BeginAnimation(OpacityProperty, fadeInAnimation);
            }
            else if (visibleView == 2)
            {
                Panel.SetZIndex(myImage, 8);
                Panel.SetZIndex(myImage2, 0);
                myImage.Source = newImageSource;
                myImage.Opacity = 0;
                fadeInAnimation.From = 0;
                fadeInAnimation.To = 1;
                fadeInAnimation.Duration = fadeInDuration;
                visibleView = 1;
                myImage.Visibility = Visibility.Visible;
                myImage.BeginAnimation(OpacityProperty, fadeInAnimation);
            }


            newImageSource = null;
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
        }

        void Restart()
        {
            contentTicker.Stop();
            contentTicker.Start();
        }

        public void LoadRSS(string heading, string url)
        {
            if (!string.IsNullOrEmpty(url))
            {

                try
                {
                    ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

                    WebClient client = new WebClient();


                    Uri uri = new Uri(url);
                    string xml = "";

                    using (StreamReader reader = new StreamReader(client.OpenRead(uri), System.Text.Encoding.GetEncoding(1252)))
                    {
                        xml = reader.ReadToEnd();
                    }

                    Items.Clear();
                    OriginalRssItems.Clear();

                    ParseRssXml(xml, false);
                    //ParseRssXml(xml1, false);
                    SetRssToTickerControl();

                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
        }

        public void ParseRssXml(string xml, bool IsTop = false)
        {
            try
            {
                SyndicationFeed feed = new SyndicationFeed();

                byte[] byteArray = Encoding.GetEncoding(1252).GetBytes(xml);
                MemoryStream stream = new MemoryStream(byteArray);

                using (stream)
                {
                    XmlTextReader xmlReader = new XmlTextReader(stream);
                    feed = SyndicationFeed.Load(xmlReader);
                }


                Items.Clear();
                OriginalRssItems.Clear();

                if (!IsTop)
                {
                    foreach (SyndicationItem item in feed.Items)
                    {
                        PopulateFeed(item);

                        if (Items.Count == m_RSSMaxItems)
                        {
                            break;
                        }
                    }
                }
                else
                    PopulateFeed(feed.Items.First());

                Source = new RSSSource();
                if (feed.Title != null)
                {
                    Source.Title = feed.Title.Text;
                }
                if (feed.Description != null)
                {
                    Source.Description = feed.Description.Text;
                }
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }

        public RSSItem PopulateFeed(SyndicationItem item)
        {
            OriginalRssItems.Add(item);

            RSSItem rssItem = new RSSItem();
            rssItem.Title = item.Title.Text;
            if (item.Summary != null)
            {
                rssItem.Description = item.Summary.Text;
            }
            if (item.LastUpdatedTime != null && item.PublishDate != null)
            {
                rssItem.PubDate = (item.LastUpdatedTime.DateTime > item.PublishDate.DateTime) ? item.LastUpdatedTime.DateTime : item.PublishDate.DateTime;
            }
            else if (item.LastUpdatedTime != null)
            {
                rssItem.PubDate = item.LastUpdatedTime.DateTime;
            }
            else if (item.PublishDate != null)
            {
                rssItem.PubDate = item.PublishDate.DateTime;
            }

            Items.Add(rssItem);

            return rssItem;
        }

        void SetRssToTickerControl()
        {
            String time;
            string source = Source.Title;
            string rss = "";
            string feed = "";
            string LRM = ((char)0x200E).ToString();  // This is a LRM

            foreach (var item in Items)
            {
                time = item.PubDate.ToString("MMM d HH:MM", System.Globalization.CultureInfo.InvariantCulture).ToString();
                //rss = rss + " - " + time  + " " + item.Title + " " + item.Description + " ";
                rss = rss + "     " + time + "- " + item.Title + "  ";
                //feed = "      " + item.Title + " " + item.Description + " " + LRM + time + LRM;
                //feed = "      " + item.Title + " " + LRM + time + LRM;

                rss = rss + feed;
            }

            if (Items.Count > 0)
            {
                v_TickerGrid.Visibility = Visibility.Visible;
            }

            //sliderText.Text = rss + " " + source;
            sliderText.Text = source+"  "+ rss;
            Restart();
        }

        private void InitializeWeather()
        {
            m_WeatherUpdateTimer = new DispatcherTimer();
            m_WeatherUpdateTimer.Interval = new TimeSpan(0, m_UpdateWeatherInterval, 0);
            m_WeatherUpdateTimer.Tick += M_WeatherUpdateTimer_Tick;
            m_WeatherUpdateTimer.Start();

            LoadWeather();
        }

        private void M_WeatherUpdateTimer_Tick(object sender, EventArgs e)
        {
            LoadWeather();
        }

        private async void LoadWeather()
        {
            OpenWeatherMapClient client = null;

            try
            {
                client = new OpenWeatherMapClient("c6b82ccf5c40a3479dff0d236e06bc53");

                if (client != null)
                {

                    //string m_Location = "Lahore,PK";
                    //int m_LocationID = 1172451;

                    //string m_Location = "Rawalpindi,PK";
                    //int m_LocationID = 1166993;

                    //string m_Location = "Sargodha,PK";
                    //int m_LocationID = 1166000;

                    //string m_Location = "Pattoki, PK";
                    //int m_LocationID = 1168226;

                    string m_Location = "Islamabad, PK";
                    int m_LocationID = 1176615;

                    //string m_Location = "Kot Momin, PK";
                    //int m_LocationID = 1182787;

                    //string m_Location = "Samundri";
                    //int m_LocationID = 1163968;

                    bool m_Degree = true;

                    CurrentWeatherResponse currentWeather = null;

                    if (!m_Degree)
                        currentWeather = await client.CurrentWeather.GetByName(m_Location, MetricSystem.Imperial);
                    //currentWeather = await client.CurrentWeather.GetByCityId(m_LocationID, MetricSystem.Imperial);

                    else
                        currentWeather = await client.CurrentWeather.GetByCityId(m_LocationID, MetricSystem.Metric);
                    //currentWeather = await client.CurrentWeather.GetByName(m_Location, MetricSystem.Metric);

                    //set city
                    v_City.Text = currentWeather.City.Name;
                    lstWeatherData.Clear();
                    lstWeatherData.Add(LoadWeatherData(currentWeather.Weather.Icon, currentWeather.Temperature, false, System.DateTime.Now));

                    int forecastDays = 4;

                    if (forecastDays > 1)
                    {
                        ForecastResponse forecast = null;

                        if (!m_Degree)
                            forecast = await client.Forecast.GetByCityId(m_LocationID, true, MetricSystem.Imperial, OpenWeatherMapLanguage.EN, forecastDays);
                        else
                            forecast = await client.Forecast.GetByCityId(m_LocationID, true, MetricSystem.Metric, OpenWeatherMapLanguage.EN, forecastDays);

                        foreach (ForecastTime forecastTime in forecast.Forecast)
                        {
                            if (forecastTime.Day.Date != System.DateTime.Now.Date)
                                lstWeatherData.Add(LoadWeatherData(forecastTime.Symbol.Var, forecastTime.Temperature, true, forecastTime.Day));
                        }
                    }

                    WeatherList.ItemsSource = lstWeatherData;
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show("Weather exception " + ex.Message);
            }


        }

        private void Window_Activated(object sender, EventArgs e)
        {
            if (contentTicker != null)
            {
                contentTicker.Start();
            }

            if (timerImageChange != null)
            {
                timerImageChange.Start();
            }

            if (m_WeatherUpdateTimer != null)
            {
                m_WeatherUpdateTimer.Start();
            }

            if (m_ClockTimer != null)
            {
                m_ClockTimer.Start();
            }
        }

        private void Window_Deactivated(object sender, EventArgs e)
        {
            if (contentTicker != null)
            {
                contentTicker.Stop();
            }

            if (timerImageChange != null)
            {
                timerImageChange.Stop();
            }

            if (m_WeatherUpdateTimer != null)
            {
                m_WeatherUpdateTimer.Stop();
            }

            if (m_ClockTimer != null)
            {
                m_ClockTimer.Stop();
            }
        }

        private WeatherData LoadWeatherData(string icon, Temperature temperature, bool IsForcast, DateTime day)
        {
            WeatherData weatherData = new WeatherData();

            string iconPath = GetIconPath(icon);
            bool m_Degree = true;

            if (!IsForcast)
            {
                if (!m_Degree)
                    weatherData.Temperature = string.Format("{0} °F", Math.Round(temperature.Value));
                else
                    weatherData.Temperature = string.Format("{0} °C", Math.Round(temperature.Value));
            }
            else
            {
                if (!m_Degree)
                    weatherData.Temperature = string.Format("{0} °F", Math.Round(temperature.Max));
                else
                    weatherData.Temperature = string.Format("{0} °C", Math.Round(temperature.Max));
            }

            string dayLocalized = string.Empty;
            string m_DayLanguage = "en-GB";

            var culture = new CultureInfo(m_DayLanguage);

            if (day.Date != System.DateTime.Now.Date)
                dayLocalized = culture.DateTimeFormat.GetDayName(day.DayOfWeek);
            else
            {
                switch (m_DayLanguage)
                {
                    case "en-GB": dayLocalized = "Today"; break;
                    case "sv-SE": dayLocalized = "I dag"; break;
                    case "fi-FI": dayLocalized = "Tänään"; break;
                    case "nn-NO": dayLocalized = "I dag"; break;
                    case "da-DK": dayLocalized = "I dag"; break;
                    case "de-DE": dayLocalized = "Heute"; break;
                }
            }

            weatherData.Day = culture.TextInfo.ToTitleCase(dayLocalized);

            weatherData.IconData = LoadIcon(iconPath);

            return weatherData;
        }

        private string GetIconPath(string icon)
        {
            try
            {
                bool m_Filled = true;

                string iconPath = "";

                if (!m_Filled)
                {
                    switch (icon)
                    {
                        case "01d": iconPath += "Icons/sun-1.svg"; break;
                        case "02d": iconPath += "Icons/partly-cloudy-1.svg"; break;
                        case "03d": iconPath += "Icons/mostly-cloudy-1.svg"; break;
                        case "04d": iconPath += "Icons/mostly-cloudy-2.svg"; break;
                        case "09d": iconPath += "Icons/rain-1.svg"; break;
                        case "10d": iconPath += "Icons/rain-day.svg"; break;
                        case "11d": iconPath += "Icons/severe-thunderstorm.svg"; break;
                        case "13d": iconPath += "Icons/snow.svg"; break;
                        case "50d": iconPath += "Icons/mist.svg"; break;
                        case "01n": iconPath += "Icons/full-moon-2.svg"; break;
                        case "02n": iconPath += "Icons/partly-cloudy-2.svg"; break;
                        case "03n": iconPath += "Icons/mostly-cloudy-1.svg"; break;
                        case "04n": iconPath += "Icons/mostly-cloudy-2.svg"; break;
                        case "09n": iconPath += "Icons/rain-1.svg"; break;
                        case "10n": iconPath += "Icons/rain-night.svg"; break;
                        case "11n": iconPath += "Icons/severe-thunderstorm.svg"; break;
                        case "13n": iconPath += "Icons/snow.svg"; break;
                        case "50n": iconPath += "Icons/mist.svg"; break;
                    }
                }
                else
                {
                    switch (icon)
                    {
                        case "01d": iconPath += "Icons/sun-1-f.svg"; break;
                        case "02d": iconPath += "Icons/partly-cloudy-1-f.svg"; break;
                        case "03d": iconPath += "Icons/mostly-cloudy-1-f.svg"; break;
                        case "04d": iconPath += "Icons/mostly-cloudy-2-f.svg"; break;
                        case "09d": iconPath += "Icons/rain-1-f.svg"; break;
                        case "10d": iconPath += "Icons/rain-day-f.svg"; break;
                        case "11d": iconPath += "Icons/severe-thunderstorm-f.svg"; break;
                        case "13d": iconPath += "Icons/snow-f.svg"; break;
                        case "50d": iconPath += "Icons/mist.svg"; break;
                        case "01n": iconPath += "Icons/full-moon-2-f.svg"; break;
                        case "02n": iconPath += "Icons/partly-cloudy-2-f.svg"; break;
                        case "03n": iconPath += "Icons/mostly-cloudy-1-f.svg"; break;
                        case "04n": iconPath += "Icons/mostly-cloudy-2-f.svg"; break;
                        case "09n": iconPath += "Icons/rain-1-f.svg"; break;
                        case "10n": iconPath += "Icons/rain-night-f.svg"; break;
                        case "11n": iconPath += "Icons/severe-thunderstorm-f.svg"; break;
                        case "13n": iconPath += "Icons/snow-f.svg"; break;
                        case "50n": iconPath += "Icons/mist.svg"; break;
                    }
                }

                return iconPath;
            }
            catch (Exception)
            {
                return string.Empty;
            }
        }
        private DrawingImage LoadIcon(string iconPath)
        {
            try
            {
                string m_IconFontColor = "#FFFFFF";
                XmlDocument svgXml = new XmlDocument();

                //svgXml.Load("Icons/blizzard-f.svg");
                svgXml.Load(iconPath);

                foreach (XmlElement pathElement in svgXml.GetElementsByTagName("path"))
                    pathElement.SetAttribute("fill", m_IconFontColor);

                foreach (XmlElement pathElement in svgXml.GetElementsByTagName("circle"))
                    pathElement.SetAttribute("fill", m_IconFontColor);

                using (XmlReader iconStream = XmlReader.Create(new System.IO.StringReader(svgXml.InnerXml)))
                {
                    return SvgReader.Load(iconStream);
                }
            }
            catch (Exception)
            {
                return null;
            }
        }

        public void SetImageLogo(byte[] logo)
        {
            try
            {
                var image = new BitmapImage();
                using (var mem = new MemoryStream(logo))
                {
                    mem.Position = 0;
                    image.BeginInit();
                    image.CreateOptions = BitmapCreateOptions.PreservePixelFormat;
                    image.CacheOption = BitmapCacheOption.OnLoad;
                    image.UriSource = null;
                    image.StreamSource = mem;
                    image.EndInit();
                }
                image.Freeze();

                v_ImageLogo.Source = image;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        static extern uint SetThreadExecutionState(EXECUTION_STATE esFlags);
        private void KeepAlive()
        {
            SetThreadExecutionState(EXECUTION_STATE.ES_DISPLAY_REQUIRED | EXECUTION_STATE.ES_SYSTEM_REQUIRED | EXECUTION_STATE.ES_CONTINUOUS);
        }

        [FlagsAttribute]
        private enum EXECUTION_STATE : uint
        {
            ES_SYSTEM_REQUIRED = 0x00000001,
            ES_DISPLAY_REQUIRED = 0x00000002,
            ES_CONTINUOUS = 0x80000000,
        }
    }

}
