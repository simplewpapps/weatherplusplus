using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Runtime;
using Windows.Foundation;
using System.Threading.Tasks;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Popups;
using Windows.UI.StartScreen;
using System.Net.Http;
using Windows.Data.Json;
using Windows.UI.Notifications;
using Windows.Data.Xml.Dom;
using System.Globalization;
using Windows.Storage;
using Windows.UI.Xaml.Media.Imaging;
using System.Diagnostics;
using Windows.ApplicationModel.Resources;

namespace WeatherTestApp2
{
    public sealed partial class MainPage : Page
    {
        private Dictionary<int, string> WeatherImages = new Dictionary<int, string>()
                {
                    { 0, "clear.jpg"},
                    { 1, "clear.jpg"},
                    { 2, "cloudy.jpg"},
                    { 3, "cloudy.jpg"},
                    { 51, "drizzle.jpg"},
                    { 56, "drizzle.jpg"},
                    { 45, "fog.jpg"},
                    { 61, "rain.jpg"},
                    { 80, "rain_showers.jpg"},
                    { 71, "snow.jpg"},
                    { 95, "thunderstorm.jpg"},
                    { 96, "thunderstorm.jpg"}
                };
        public static readonly Dictionary<int, string> weatherDict = new Dictionary<int, string>
        {
            {0, "0"}, //Clear
            {1, "1"}, //Mainly clear
            {2, "2"}, //Partly cloudy
            {3, "3"}, //Cloudy
            {45, "45"}, //Fog
            {48, "48"}, //Rime fog
            {51, "51"}, //Drizzle
            {53, "53"}, //Drizzle
            {55, "55"}, //Drizzle
            {56, "56"}, //Freezing drizzle
            {57, "57"}, //Freezing drizzle
            {61, "61"}, //Rain
            {63, "63"}, //Rain
            {65, "65"}, //Heavy rain
            {66, "66"}, //Freezing rain
            {67, "67"}, //Freezing rain
            {71, "71"}, //Snow
            {73, "73"}, //Snow
            {75, "75"}, //Heavy snow
            {77, "77"}, //Snow grains
            {80, "80"}, //Rain showers
            {81, "81"}, //Rain showers
            {82, "82"}, //Heavy rain showers
            {85, "85"}, //Snow showers
            {86, "86"}, //Heavy snow showers
            {95, "95"}, //Thunderstorm
            {96, "96"}, //Thunderstorm with hail
            {99, "99"} //Thunderstorm with hail
        };

        public void UpdateCity(string cityName, double lat, double lon)
        {
            AppSettings.CityCurrent = cityName;
            AppSettings.Latitude = lat;
            AppSettings.Longitude = lon;

            MainPivot.Title = AppSettings.CityCurrent.ToUpperInvariant();
            GetWeather(AppSettings.Latitude, AppSettings.Longitude);

            // Сохраняем в LocalSettings
            var localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
            localSettings.Values["LastCity"] = AppSettings.CityCurrent;
            localSettings.Values["Latitude"] = AppSettings.Latitude;
            localSettings.Values["Longitude"] = AppSettings.Longitude;
        }


        //private double Latitude = 51.51;
        //private double Longitude = -0.13;

        //private string CityCurrent = "LONDON";


        //private async Task PinCityTile(string cityName)
        //{
        //string tileId = "WeatherTile_" + cityName; //unique id for tile
        //string displayName = "Weather in " + cityName;
        //id = "WeatherTile_" + cityName;

        //Uri logo = new Uri("ms-appx:///Assets/weather2360.png");

        //if (SecondaryTile.Exists(tileId))
        //{
        //var dialog = new MessageDialog(cityName + " already pinned.");
        //await dialog.ShowAsync();
        //return;
        //}

        //var secondaryTile = new Windows.UI.StartScreen.SecondaryTile(
        //tileId,
        //displayName,
        //"args=" + cityName,
        //logo,
        //Windows.UI.StartScreen.TileSize.Square150x150
        //);

        //secondaryTile.VisualElements.Square150x150Logo = logo;
        //secondaryTile.VisualElements.ShowNameOnSquare150x150Logo = true;

        //await secondaryTile.RequestCreateAsync();


        //}

        //private void UpdateSecondaryTile(string tileId, double TempToday, string cityName)
        //{

        //string tileXmlString = string.Format(
        //"<tile>" +
        //"<visual>" +
        //"<binding template='TileMedium'>" +
        //"<text hint-style='title'>{0}</text>" +
        //"</binding>" +
        //"</visual>" +
        //"</tile>",
        //empToday.ToString("F0"), cityName
        //);
        //XmlDocument tileXml = new XmlDocument();
        //tileXml.LoadXml(tileXmlString);

        //TileNotification tileNotification = new TileNotification(tileXml);
        //TileUpdater updater = TileUpdateManager.CreateTileUpdaterForSecondaryTile(tileId);
        //updater.Clear();
        //updater.Update(tileNotification);

        //}



        private void ToggleTemperature_Click(object sender, RoutedEventArgs e)
        {
            AppSettings.IsCelsius = !AppSettings.IsCelsius;

            ApplicationData.Current.LocalSettings.Values["IsCelsius"] = AppSettings.IsCelsius;

            GetWeather(AppSettings.Latitude, AppSettings.Longitude);
            LoadHourlyForecast(AppSettings.Latitude, AppSettings.Longitude);
            LoadDailyForecast(AppSettings.Latitude, AppSettings.Longitude);
        }



    
        public MainPage()
        {
            this.InitializeComponent();

            MainPivot.Title = AppSettings.CityCurrent.ToUpperInvariant();

            this.NavigationCacheMode = NavigationCacheMode.Required;

            var localSettings = ApplicationData.Current.LocalSettings;

            if (localSettings.Values.ContainsKey("Latitude") && localSettings.Values.ContainsKey("Longitude"))
            {
                AppSettings.Latitude = (double)localSettings.Values["Latitude"];
                AppSettings.Longitude = (double)localSettings.Values["Longitude"];
            }

            if (localSettings.Values.ContainsKey("LastCity"))
            {
                AppSettings.CityCurrent = localSettings.Values["LastCity"].ToString();
            }
            else
            {
                AppSettings.CityCurrent = "LONDON"; // default
            }


            if (localSettings.Values.ContainsKey("IsCelsius"))
            {
                AppSettings.IsCelsius = (bool)localSettings.Values["IsCelsius"];
            }

            ShowFirstRunMessage();
            MainPivot.Title = AppSettings.CityCurrent.ToUpperInvariant();
            GetWeather(AppSettings.Latitude, AppSettings.Longitude);
            LoadHourlyForecast(AppSettings.Latitude, AppSettings.Longitude);
            LoadDailyForecast(AppSettings.Latitude, AppSettings.Longitude);
        }

        private void ChangeCity_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(SearchPage));
        }

        public static string Localized(string key, params object[] args)
        {
            var loader = new ResourceLoader();
            string format = loader.GetString(key);
            return string.Format(format, args);
        }

        private async void ShowFirstRunMessage()
        {
            var localSettings = ApplicationData.Current.LocalSettings;

            if (!localSettings.Values.ContainsKey("HasRunBefore"))
            {
                localSettings.Values["HasRunBefore"] = true;

                var dialog = new MessageDialog("Welcome to Weather++ app! Right now current city to show weather is set to LONDON, but you can change it by tapping globe icon. Thanks!");
                await dialog.ShowAsync();
            }
        }

        public class HourlyForecast
        {
            public string Time { get; set; }
            public string Temperature { get; set; }
            public string Status { get; set; }
        }

        public class DailyForecast
        {
            public string Day { get; set; }
            public string TemperatureMax { get; set; }
            public string TemperatureMin { get; set; }
            public string Status { get; set; }
        }

        private async void MessageBox(string msg)
        {
            var dialog = new MessageDialog(msg);
            await dialog.ShowAsync();
        }

        private async void Provider_Info(object sender, RoutedEventArgs e)
        {
            var dialog = new MessageDialog("Weather++ is a WP8.1 weather app with simple features and data. \nWeather provided by Open-Meteo: https://open-meteo.com/. \nRomanian translation by: u/MildOff2024 \n \nImage Credits: \n \nSunny image by jplenio: https://pixabay.com/photos/sun-sky-blue-sunlight-sunbeam-3588618/ \n \nCloudy image by JACLOU-DL: https://pixabay.com/photos/clouds-cumulus-cloudy-sky-air-5481190/ \n \nDrizzle image by gaborszoke: https://pixabay.com/photos/rain-car-window-gloomy-raindrops-4440791/ \n \nFog image by Nature_Brothers: https://pixabay.com/photos/fog-trees-forest-foggy-woods-6122490/ \n \nRain clouds image by ELG21: https://pixabay.com/photos/clouds-storm-rain-weather-air-9550640/ \n \nRain showers image by Hans: https://pixabay.com/photos/downpour-rain-shower-rain-shower-8823/ \n \nSnow image by adege: https://pixabay.com/photos/snow-new-zealand-snowdrift-snowy-4066640/ \n \nThunderstorm image by bogitw: https://pixabay.com/photos/flash-sky-clouds-energy-1156822/");
            await dialog.ShowAsync();
        }

        //private void ShowWeatherToast(double temp, double feels)
        //{
        //string degreeSymbol = AppSettings.IsCelsius ? "°C" : "°F";

        //string message = "Today's weather " +
        //temp.ToString("F0") + degreeSymbol +
        //", feels like " + feels.ToString("F0") + degreeSymbol + ".";
        //var toastXml = ToastNotificationManager.GetTemplateContent(ToastTemplateType.ToastText02);
        //var stringElements = toastXml.GetElementsByTagName("text");
        //stringElements[0].AppendChild(toastXml.CreateTextNode("Weather++"));
        //stringElements[1].AppendChild(toastXml.CreateTextNode(message));

        //DateTimeOffset scheduledTime = DateTimeOffset.Now.AddMinutes(30);
        //var scheduledToast = new ScheduledToastNotification(toastXml, scheduledTime);

        //ToastNotificationManager.CreateToastNotifier().AddToSchedule(scheduledToast);
        //}

        private async void LoadDailyForecast(double lat, double lon)
        {
            try
            {
                var client = new HttpClient();
                string url = "https://api.open-meteo.com/v1/forecast?latitude="
                    + lat.ToString(System.Globalization.CultureInfo.InvariantCulture)
                    + "&longitude="
                    + lon.ToString(System.Globalization.CultureInfo.InvariantCulture)
                    + "&daily=temperature_2m_max,temperature_2m_min,weather_code&timezone=auto";
                string json = await client.GetStringAsync(url);
                JsonObject root = JsonObject.Parse(json);

                JsonObject daily = root.GetNamedObject("daily");
                JsonArray days = daily.GetNamedArray("time");
                JsonArray tempsMax = daily.GetNamedArray("temperature_2m_max");
                JsonArray tempsMin = daily.GetNamedArray("temperature_2m_min");
                JsonArray codes = daily.GetNamedArray("weather_code");

                var list = new List<DailyForecast>();

                DateTime now = DateTime.Today;

                int start = 0;

                for (int i = 0; i < days.Count; i++)
                {
                    DateTime t = DateTime.Parse(days[i].GetString());
                    if (t.Date <= now)
                        continue;
                }

                for (int i = start; i < start + 7 && i < (int)days.Count; i++)
                {
                    string day = DateTime.Parse(days[i].GetString()).ToString("MMM dd");
                    double tempMax = tempsMax[i].GetNumber();
                    double tempMin = tempsMin[i].GetNumber();
                    if (!AppSettings.IsCelsius)
                    {
                        tempMax = tempMax * 9 / 5 + 32;
                        tempMin = tempMin * 9 / 5 + 32;
                    }
                    int code = (int)codes[i].GetNumber();
                    string status = Localized("WeatherCode_" + weatherDict[code]);

                    list.Add(new DailyForecast
                    {
                        Day = day,
                        TemperatureMax = tempMax.ToString("F0") + (AppSettings.IsCelsius ? "°C" : "°F") + " - " + tempMin.ToString("F0") + (AppSettings.IsCelsius ? "°C" : "°F"),
                        //TemperatureMin = tempMin.ToString("F0") + (AppSettings.IsCelsius ? "°C" : "°F"),
                        Status = status
                    });

                }
                DailyList.ItemsSource = list;
            }
            catch (Exception ex)
            {
                await new MessageDialog("Error loading daily forecast: " + ex.Message).ShowAsync();
            }
        }

        private async void LoadHourlyForecast(double lat, double lon)
        {
            try
            {
                var client = new HttpClient();
                string url = "https://api.open-meteo.com/v1/forecast?latitude="
                    + lat.ToString(System.Globalization.CultureInfo.InvariantCulture)
                    + "&longitude="
                    + lon.ToString(System.Globalization.CultureInfo.InvariantCulture)
                    + "&hourly=temperature_2m,weathercode&timezone=auto";
                string json = await client.GetStringAsync(url);
                JsonObject root = JsonObject.Parse(json);

                JsonObject hourly = root.GetNamedObject("hourly");
                JsonArray times = hourly.GetNamedArray("time");
                JsonArray temps = hourly.GetNamedArray("temperature_2m");
                JsonArray codes = hourly.GetNamedArray("weathercode");

                var list = new List<HourlyForecast>();

                DateTime now = DateTime.UtcNow;

                int startIndex = 0;

                for (int i = 0; i < times.Count; i++)
                {
                    DateTime t = DateTime.Parse(times[i].GetString());
                    if (t >= now)
                    {
                        startIndex = i;
                        break;
                    }
                }

                for (int i = startIndex; i < startIndex + 12 && i < (int)times.Count; i++)
                {
                    string time = DateTime.Parse(times[i].GetString()).ToLocalTime().ToString("HH:mm");
                    double temp = temps[i].GetNumber();
                    if (!AppSettings.IsCelsius)
                        temp = temp * 9 / 5 + 32;
                    int code = (int)codes[i].GetNumber();
                    string status = Localized("WeatherCode_" + weatherDict[code]);

                    list.Add(new HourlyForecast
                    {
                        Time = time,
                        Temperature = temp.ToString("F0") + (AppSettings.IsCelsius ? "°C" : "°F"),
                        Status = status
                    });
                }

                HourlyList.ItemsSource = list;
            }
            catch (Exception ex)
            {
                await new MessageDialog("Error loading hourly forecast: " + ex.Message).ShowAsync();
            }
        }


        public void LiveTileYAY(double temp, string cond)
        {
            string degreeSymbol = AppSettings.IsCelsius ? "°C" : "°F";

            XmlDocument xml = new XmlDocument();

            XmlElement tile = xml.CreateElement("tile");
            xml.AppendChild(tile);

            XmlElement visual = xml.CreateElement("visual");
            visual.SetAttribute("version", "2");
            tile.AppendChild(visual);

            XmlElement binding = xml.CreateElement("binding");
            binding.SetAttribute("template", "TileSquare150x150Text01");
            visual.AppendChild(binding);

            XmlElement textTemp = xml.CreateElement("text");
            textTemp.SetAttribute("id", "1");
            textTemp.InnerText = temp.ToString() + degreeSymbol;
            binding.AppendChild(textTemp);

            XmlElement textCond = xml.CreateElement("text");
            textCond.SetAttribute("id", "2");
            textCond.InnerText = cond;
            binding.AppendChild(textCond);

            TileNotification tilenotif = new TileNotification(xml);

            TileUpdater updater = TileUpdateManager.CreateTileUpdaterForApplication();
            updater.Update(tilenotif);
        }

        private async void GetWeather(double lat, double lon)
        {
            try
            {
                var client = new HttpClient();
                string url = "https://api.open-meteo.com/v1/forecast?latitude="
                    + lat.ToString(System.Globalization.CultureInfo.InvariantCulture)
                    + "&longitude="
                    + lon.ToString(System.Globalization.CultureInfo.InvariantCulture)
                    + "&hourly=temperature_2m,windspeed_10m,uv_index,rain,snowfall,apparent_temperature,relativehumidity_2m,weathercode,surface_pressure,visibility&current_weather=true&timezone=auto";
                string json = await client.GetStringAsync(url);
                JsonObject root = JsonObject.Parse(json);

                string degreeSymbol = AppSettings.IsCelsius ? "°C" : "°F";

                //Today
                JsonObject hourly = root.GetNamedObject("hourly");
                JsonArray uvArray = hourly.GetNamedArray("uv_index");
                JsonArray precipitationArray = hourly.GetNamedArray("rain");
                JsonArray snowArray = hourly.GetNamedArray("snowfall");
                JsonArray feelsArray = hourly.GetNamedArray("apparent_temperature");
                JsonArray humidArray = hourly.GetNamedArray("relativehumidity_2m");
                JsonArray tempArray = hourly.GetNamedArray("temperature_2m");
                JsonArray windArray = hourly.GetNamedArray("windspeed_10m");
                JsonArray weatherCodeArray = hourly.GetNamedArray("weathercode");
                JsonArray pressureArray = hourly.GetNamedArray("surface_pressure");
                JsonArray visibleArray = hourly.GetNamedArray("visibility");
                int code = (int)weatherCodeArray[0].GetNumber();
                string status = Localized("WeatherCode_" + weatherDict[code]);
                TypeToday.Text = status;

                double tempValueToday = tempArray.GetNumberAt(0);
                if (!AppSettings.IsCelsius)
                {
                    tempValueToday = tempValueToday * 9 / 5 + 32; //c -> f
                }
                double windSpeed = windArray.GetNumberAt(0);
                double UVToday = uvArray.GetNumberAt(0);
                double rainToday = precipitationArray.GetNumberAt(0);
                double snowToday = snowArray.GetNumberAt(0);
                double feelsToday = feelsArray.GetNumberAt(0);
                if (!AppSettings.IsCelsius)
                {
                    feelsToday = feelsToday * 9 / 5 + 32; //c -> f for feels like
                }
                double humidToday = humidArray.GetNumberAt(0);
                double pressToday = pressureArray.GetNumberAt(0);
                double visibleToday = visibleArray.GetNumberAt(0);

                visibleToday = visibleToday / 1000; //divide so it will be correct
                TodayTemp.Text = tempValueToday.ToString("F0") + degreeSymbol;
                TodayCondition.Text = Localized("WindLang", windSpeed).ToString();
                UV.Text = Localized("UVValue", UVToday).ToString();
                TodayUVValue.Text = UVToday.ToString();
                TodayPrecip.Text = Localized("RainLang", rainToday).ToString();
                TodaySnow.Text = Localized("SnowLang", snowToday).ToString();
                TodayFeelsLike.Text = Localized("FeelsLang", feelsToday).ToString() + degreeSymbol;
                TodayHumid.Text = Localized("HumidLang", humidToday).ToString();
                TodayVisible.Text = Localized("VisibleLang", visibleToday).ToString();

                if (BGIMG != null && WeatherImages.ContainsKey(code))
                {
                    BGIMG.Source = new BitmapImage(new Uri("ms-appx:///Assets/Backgrounds/" + WeatherImages[code], UriKind.Absolute));
                }
                else
                {
                    Debug.WriteLine("No image for: " + status);
                }

                //UpdateTile(tempIntToday, windSpeed);
                LiveTileYAY(tempValueToday, status);

            }
            catch (Exception ex)
            {
                MessageBox("Error occured or internet missing.");
                return;
            }
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {

            base.OnNavigatedTo(e);

            var localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;

            double lat = 52.52;
            double lon = 13.41;
            string city = "BERLIN";

            if (localSettings.Values.ContainsKey("Latitude"))
                lat = (double)localSettings.Values["Latitude"];
            if (localSettings.Values.ContainsKey("Longitude"))
                lon = (double)localSettings.Values["Longitude"];
            if (localSettings.Values.ContainsKey("City"))
                city = (string)localSettings.Values["City"];

            if (!AppSettings.IsCelsius)
            {
                AppSettings.IsCelsius = false;
            }

            MainPivot.Title = city.ToUpperInvariant();
            GetWeather(lat, lon);
            LoadHourlyForecast(lat, lon);
            LoadDailyForecast(lat, lon);
        }
    }
}