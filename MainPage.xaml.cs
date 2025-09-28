using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
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

namespace WeatherTestApp2
{
    public sealed partial class MainPage : Page
    {
        private Dictionary<string, string> WeatherImages = new Dictionary<string, string>()
                {
                    { "Clear", "clear.jpg"},
                    { "Mainly clear", "clear.jpg"},
                    { "Cloudy", "cloudy.jpg"},
                    { "Drizzle", "drizzle.jpg"},
                    { "Freezing drizzle", "drizzle.jpg"},
                    { "Fog", "fog.jpg"},
                    { "Rain", "rain.jpg"},
                    { "Rain showers", "rain_showers.jpg"},
                    { "Snow", "snow.jpg"},
                    { "Thunderstorm", "thunderstorm.jpg"},
                    { "Thunderstorm with hail", "thunderstorm.jpg"}
                };

        private double Latitude = 51.51;
        private double Longitude = -0.13;

        private string CityCurrent = "LONDON";

        private Dictionary<string, double[]> cities = new Dictionary<string, double[]>
        {
            {"las vegas", new double[] {36.17, -115.14}},
            {"new york", new double[] {40.71, -74.01}},
            {"kyiv", new double[] {50.45, 30.52}},
            {"london", new double[] {51.51, -0.13}},
            {"paris", new double[] {48.85, 2.35}},
            {"tokyo", new double[] {35.68, 139.69}},
            {"sydney", new double[] {-33.87, 151.21}},
            {"moscow", new double[] {55.75, 37.62}},
            {"berlin", new double[] {52.52, 13.40}},
            {"rome", new double[] {41.90, 12.49}},
            {"kharkiv", new double[] {49.99, 36.23}},
            {"warsaw", new double[] {52.23, 21.02}},
            {"lodz", new double[] {51.75, 19.45}},
            {"bucharest", new double[] {44.42, 26.10}},
            {"new delhi", new double[] {28.61, 77.20}}
        };

        private async void ChangeCity_Click(object sender, RoutedEventArgs e)
        {
            var inputTextBox = new TextBox { AcceptsReturn = false, Height = 32 };
            var dialog = new ContentDialog
            {
                Title = "Enter name of the city",
                Content = inputTextBox,
                PrimaryButtonText = "OK",
                SecondaryButtonText = "Cancel"
            };

            var result = await dialog.ShowAsync();

            if (result == ContentDialogResult.Primary)
            {
                string cityName = inputTextBox.Text.Trim().ToLower();

                if (cities.ContainsKey(cityName))
                {
                    double[] coords = cities[cityName];
                    Latitude = coords[0];
                    Longitude = coords[1];

                    CityCurrent = CultureInfo.CurrentCulture.TextInfo.ToUpper(cityName);
                    MainPivot.Title = "WEATHER++ - " + CityCurrent;

                    var localSettings = ApplicationData.Current.LocalSettings;
                    localSettings.Values["LastCity"] = CityCurrent;

                    GetWeather(Latitude, Longitude);
                    LoadFiveDayForecast(Latitude, Longitude);
                }
            }
            else
            {
                await new MessageDialog("City not found.").ShowAsync();
            }
        }

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

            GetWeather(Latitude, Longitude);
            LoadFiveDayForecast(Latitude, Longitude);
        }

        public static class AppSettings
        {
            private static bool _isCelsius = true;

            public static bool IsCelsius
            {
                get { return _isCelsius;  }
                set { _isCelsius = value;  }
            }
        }

    
        public MainPage()
        {
            this.InitializeComponent();

            GetWeather(Latitude, Longitude);

            MainPivot.Title = "WEATHER++ - " + CityCurrent;

            this.NavigationCacheMode = NavigationCacheMode.Required;

            var localSettings = ApplicationData.Current.LocalSettings;

            if (localSettings.Values.ContainsKey("SelectedCity"))
            {
                string savedCity = localSettings.Values["SelectedCity"].ToString();
                if (cities.ContainsKey(savedCity))
                {
                    double[] coords = cities[savedCity];
                    Latitude = coords[0];
                    Longitude = coords[1];
                }
            }

            if (localSettings.Values.ContainsKey("IsCelsius"))
            {
                AppSettings.IsCelsius = (bool)localSettings.Values["IsCelsius"];
            }

            if (localSettings.Values.ContainsKey("LastCity"))
            {
                CityCurrent = localSettings.Values["LastCity"].ToString();
            }
            else
            {
                CityCurrent = "LONDON"; //default city
            }

            MainPivot.Title = "WEATHER++ - " + CityCurrent;

            if (cities.ContainsKey(CityCurrent.ToLower()))
            {
                double[] coords = cities[CityCurrent.ToLower()];
                Latitude = coords[0];
                Longitude = coords[1];
            }



            ShowFirstRunMessage();

            GetWeather(Latitude, Longitude);
            LoadFiveDayForecast(Latitude, Longitude);
        }

        private async void ShowFirstRunMessage()
        {
            var localSettings = ApplicationData.Current.LocalSettings;

            if (!localSettings.Values.ContainsKey("HasRunBefore"))
            {
                localSettings.Values["HasRunBefore"] = true;

                var dialog = new MessageDialog("Welcome to Weather++ app! Right now current city to show weather is set to LONDON, but you can change it by tapping globe icon. List of available cities is in Reddit post. Thanks!");
                await dialog.ShowAsync();
            }
        }

        public class ForecastDay
        {
            public string Date { get; set; }
            public string TempMax { get; set; }
            public string TempMin { get; set; }
        }

        private async void LoadFiveDayForecast(double lat, double lon)
        {
            string url = "https://api.open-meteo.com/v1/forecast?latitude="
                    + lat.ToString(System.Globalization.CultureInfo.InvariantCulture)
                    + "&longitude="
                    + lon.ToString(System.Globalization.CultureInfo.InvariantCulture)
                    + "&daily=temperature_2m_max,temperature_2m_min,windspeed_10m_max&current_weather=true&timezone=auto";

            HttpClient client = new HttpClient();
            string response = await client.GetStringAsync(new Uri(url));

            string degreeSymbol = AppSettings.IsCelsius ? "°C" : "°F";

            JsonObject json = JsonObject.Parse(response);
            var daily = json.GetNamedObject("daily");

            var dates = daily.GetNamedArray("time");
            var maxTemps = daily.GetNamedArray("temperature_2m_max");
            var minTemps = daily.GetNamedArray("temperature_2m_min");

            List<ForecastDay> forecast = new List<ForecastDay>();

            for (int i = 0; i < 5; i++) //first five days
            {
                double max = maxTemps[i].GetNumber();
                double min = minTemps[i].GetNumber();

                if (!AppSettings.IsCelsius)
                {
                    max = max * 9 / 5 + 32;
                    min = min * 9 / 5 + 32;
                }

                forecast.Add(new ForecastDay
                {
                    Date = dates[i].GetString(),
                    TempMax = Convert.ToString(max, CultureInfo.InvariantCulture) + degreeSymbol,
                    TempMin = Convert.ToString(min, CultureInfo.InvariantCulture) + degreeSymbol,
                });
            }

            ForecastList.ItemsSource = forecast;
        }

        private async void Provider_Info(object sender, RoutedEventArgs e)
        {
            var dialog = new MessageDialog("Weather++ is a WP8.1 weather app with simple features and data. \nWeather provided by Open-Meteo: https://open-meteo.com/. \n \nImage Credits: \n \nSunny image by jplenio: https://pixabay.com/photos/sun-sky-blue-sunlight-sunbeam-3588618/ \n \nCloudy image by JACLOU-DL: https://pixabay.com/photos/clouds-cumulus-cloudy-sky-air-5481190/ \n \nDrizzle image by gaborszoke: https://pixabay.com/photos/rain-car-window-gloomy-raindrops-4440791/ \n \nFog image by Nature_Brothers: https://pixabay.com/photos/fog-trees-forest-foggy-woods-6122490/ \n \nRain clouds image by ELG21: https://pixabay.com/photos/clouds-storm-rain-weather-air-9550640/ \n \nRain showers image by Hans: https://pixabay.com/photos/downpour-rain-shower-rain-shower-8823/ \n \nSnow image by adege: https://pixabay.com/photos/snow-new-zealand-snowdrift-snowy-4066640/ \n \nThunderstorm image by bogitw: https://pixabay.com/photos/flash-sky-clouds-energy-1156822/");
            await dialog.ShowAsync();
        }

        private void RefreshWeather_Click(object sender, RoutedEventArgs e)
        {
            GetWeather(Latitude, Longitude);
            LoadFiveDayForecast(Latitude, Longitude);
        }

        public void Update()
        {
            GetWeather(Latitude, Longitude);
            LoadFiveDayForecast(Latitude, Longitude);
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

        private async void GetWeather(double lat, double lon)
        {
            try
            {
                var client = new HttpClient();
                string url = "https://api.open-meteo.com/v1/forecast?latitude="
                    + lat.ToString(System.Globalization.CultureInfo.InvariantCulture)
                    + "&longitude="
                    + lon.ToString(System.Globalization.CultureInfo.InvariantCulture)
                    + "&hourly=temperature_2m,windspeed_10m,uv_index,precipitation,snowfall,apparent_temperature,relativehumidity_2m,weathercode,surface_pressure,visibility&current_weather=true&timezone=auto";
                string json = await client.GetStringAsync(url);
                JsonObject root = JsonObject.Parse(json);

                string degreeSymbol = AppSettings.IsCelsius ? "°C" : "°F";

                //Today
                JsonObject hourly = root.GetNamedObject("hourly");
                JsonArray uvArray = hourly.GetNamedArray("uv_index");
                JsonArray precipitationArray = hourly.GetNamedArray("precipitation");
                JsonArray snowArray = hourly.GetNamedArray("snowfall");
                JsonArray feelsArray = hourly.GetNamedArray("apparent_temperature");
                JsonArray humidArray = hourly.GetNamedArray("relativehumidity_2m");
                JsonArray tempArray = hourly.GetNamedArray("temperature_2m");
                JsonArray windArray = hourly.GetNamedArray("windspeed_10m");
                JsonArray weatherCodeArray = hourly.GetNamedArray("weathercode");
                JsonArray pressureArray = hourly.GetNamedArray("surface_pressure");
                JsonArray visibleArray = hourly.GetNamedArray("visibility");
                int code = (int)weatherCodeArray[0].GetNumber();

                Dictionary<int, string> weatherDict = new Dictionary<int, string>
                {
                    {0, "Clear"},
                    {1, "Mainly clear"},
                    {2, "Partly cloudy"},
                    {3, "Cloudy"},
                    {45, "Fog"},
                    {48, "Rime fog"},
                    {51, "Drizzle"},
                    {53, "Drizzle"},
                    {55, "Drizzle"},
                    {56, "Freezing drizzle"},
                    {57, "Freezing drizzle"},
                    {61, "Rain"},
                    {63, "Rain"},
                    {65, "Heavy rain"},
                    {66, "Freezing rain"},
                    {67, "Freezing rain"},
                    {71, "Snow"},
                    {73, "Snow"},
                    {75, "Heavy snow"},
                    {77, "Snow grains"},
                    {80, "Rain showers"},
                    {81, "Rain showers"},
                    {82, "Heavy rain showers"},
                    {85, "Snow showers"},
                    {86, "Heavy snow showers"},
                    {95, "Thunderstorm"},
                    {96, "Thunderstorm with hail"},
                    {99, "Thunderstorm with hail"},
                };

                string status = weatherDict.ContainsKey(code) ? weatherDict[code] : "Unknown";
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
                TodayTemp.Text = Convert.ToString(tempValueToday, CultureInfo.InvariantCulture) + degreeSymbol;
                TodayCondition.Text = "Wind: " + windSpeed.ToString() + " km/h";
                TodayUV.Text = "UV: " + UVToday.ToString(CultureInfo.InvariantCulture);
                TodayPrecip.Text = "Rain: " + rainToday.ToString(CultureInfo.InvariantCulture) + " mm";
                TodaySnow.Text = "Snow: " + snowToday.ToString(CultureInfo.InvariantCulture) + " cm";
                TodayFeelsLike.Text = "Feels like: " + feelsToday.ToString(CultureInfo.InvariantCulture) + degreeSymbol;
                TodayHumid.Text = "Humidity: " + humidToday.ToString(CultureInfo.InvariantCulture) + "%";
                TodayPressure.Text = "Pressure: " + pressToday.ToString(CultureInfo.InvariantCulture) + " hPa";
                TodayVisible.Text = "Visibility: " + visibleToday.ToString(CultureInfo.InvariantCulture) + " km";

                if (BGIMG != null && WeatherImages.ContainsKey(status))
                {
                    BGIMG.Source = new BitmapImage(new System.Uri("ms-appx:///Assets/Backgrounds/" + WeatherImages[status], UriKind.Absolute));
                }
                else
                {
                    Debug.WriteLine("No image for: " + status);
                }

                //UpdateTile(tempIntToday, windSpeed);

                string urlTomorrow = "https://api.open-meteo.com/v1/forecast?latitude="
                    + lat.ToString(System.Globalization.CultureInfo.InvariantCulture)
                    + "&longitude="
                    + lon.ToString(System.Globalization.CultureInfo.InvariantCulture)
                    + "&daily=temperature_2m_max,apparent_temperature_max,windspeed_10m_max,precipitation_sum,snowfall_sum,uv_index_max,relative_humidity_2m_max,weathercode,visibility_max,surface_pressure_max&timezone=auto";

                string jsonTomorrow = await client.GetStringAsync(urlTomorrow);
                JsonObject rootTomorrow = JsonObject.Parse(jsonTomorrow);

                //Tomorrow
                JsonObject dailyForecast = rootTomorrow.GetNamedObject("daily");
                JsonArray uvTomorrow = dailyForecast.GetNamedArray("uv_index_max");
                JsonArray precipTomorrow = dailyForecast.GetNamedArray("precipitation_sum");
                JsonArray snowTomorrow = dailyForecast.GetNamedArray("snowfall_sum");
                JsonArray tempMaxArray = dailyForecast.GetNamedArray("temperature_2m_max");
                JsonArray windSpeedArray = dailyForecast.GetNamedArray("windspeed_10m_max");
                JsonArray feelsLikeTomorrowArray = dailyForecast.GetNamedArray("apparent_temperature_max");
                JsonArray humidTomorrowArray = dailyForecast.GetNamedArray("relative_humidity_2m_max");
                JsonArray weatherCodeTomorrowArray = dailyForecast.GetNamedArray("weathercode");
                JsonArray pressureTomorrowArray = dailyForecast.GetNamedArray("surface_pressure_max");
                JsonArray visibleTomorrowArray = dailyForecast.GetNamedArray("visibility_max");
                double tempValueTomorrow = tempMaxArray[1].GetNumber(); //1 - 1 day, 2 - 2 day, 3... - 3 and more days
                if (!AppSettings.IsCelsius)
                {
                    tempValueTomorrow = tempValueTomorrow * 9 / 5 + 32; //c -> f
                }
                double windTomorrow = windSpeedArray[1].GetNumber();
                double UVTomorrow = uvTomorrow[1].GetNumber();
                double rainTomorrow = precipTomorrow[1].GetNumber();
                double SnowTomorrow = snowTomorrow[1].GetNumber();
                double feelsLikeTomorrow = feelsLikeTomorrowArray[1].GetNumber();
                if (!AppSettings.IsCelsius)
                {
                    feelsLikeTomorrow = feelsLikeTomorrow * 9 / 5 + 32; //c -> f for feels like tomorrow
                }
                double HumidTomorrow = humidTomorrowArray[1].GetNumber();
                int codeTomorrow = (int)weatherCodeTomorrowArray[1].GetNumber();
                string statusTomorrow = weatherDict.ContainsKey(codeTomorrow) ? weatherDict[codeTomorrow] : "Unknown";
                double PressTomorrow = pressureTomorrowArray[1].GetNumber();
                double VisibleTomorrow = visibleTomorrowArray[1].GetNumber();
                VisibleTomorrow = VisibleTomorrow / 1000;
                TomorrowTemp.Text = Convert.ToString(tempValueTomorrow, CultureInfo.InvariantCulture) + degreeSymbol;
                TomorrowCondition.Text = "Wind: " + windTomorrow.ToString() + " km/h";
                TomorrowUV.Text = "UV: " + UVTomorrow.ToString(CultureInfo.InvariantCulture);
                TomorrowPrecip.Text = "Rain: " + rainTomorrow.ToString(CultureInfo.InvariantCulture) + " mm";
                TomorrowSnow.Text = "Snow: " + Convert.ToString(SnowTomorrow, CultureInfo.InvariantCulture) + " cm";
                TomorrowFeelsLike.Text = "Feels like: " + Convert.ToString(feelsLikeTomorrow, CultureInfo.InvariantCulture) + degreeSymbol;
                TomorrowHumid.Text = "Humidity: " + Convert.ToString(HumidTomorrow, CultureInfo.InvariantCulture) + "%";
                TypeTomorrow.Text = statusTomorrow;
                TomorrowPressure.Text = "Pressure: " + Convert.ToString(PressTomorrow, CultureInfo.InvariantCulture) + " hPa";
                TomorrowVisible.Text = "Visibility: " + Convert.ToString(VisibleTomorrow, CultureInfo.InvariantCulture) + " km";
            }
            catch (Exception ex)
            {
                var error = new MessageDialog("Error: " + ex.Message);
                var ignore = error.ShowAsync();
            }
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            GetWeather(Latitude, Longitude);
            LoadFiveDayForecast(Latitude, Longitude);

            if (!AppSettings.IsCelsius)
            {
                AppSettings.IsCelsius = false;
            }
        }
    }
}