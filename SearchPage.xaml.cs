using WeatherTestApp2.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Graphics.Display;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.Data.Json;
using System.Net.Http;
using Windows.UI.Popups;
using System.Globalization;

namespace WeatherTestApp2
{
    public sealed partial class SearchPage : Page
    {
        private NavigationHelper navigationHelper;
        private ObservableDictionary defaultViewModel = new ObservableDictionary();

        public SearchPage()
        {
            this.InitializeComponent();

            this.navigationHelper = new NavigationHelper(this);
            this.navigationHelper.LoadState += this.NavigationHelper_LoadState;
            this.navigationHelper.SaveState += this.NavigationHelper_SaveState;
        }

        private void ResultsList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selected = (CityResult)ResultsList.SelectedItem;
            if (selected != null)
            {
                var localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
                localSettings.Values["Latitude"] = selected.Latitude;
                localSettings.Values["Longitude"] = selected.Longitude;
                localSettings.Values["City"] = selected.Name;

                Frame.GoBack();
            }
        }

        private async void SearchCityAsync(string cityName)
        {
            try
            {
                string query = Uri.EscapeDataString(cityName).Replace("%20", "+");
                string url = "https://geocoding-api.open-meteo.com/v1/search?name=" + query + "&count=25&language=" + CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;

                var client = new HttpClient();
                string json = await client.GetStringAsync(url);
                JsonObject root = JsonObject.Parse(json);

                if (!root.ContainsKey("results"))
                {
                    await new MessageDialog("No results found.").ShowAsync();
                    return;
                }

                JsonArray results = root.GetNamedArray("results");
                var list = new List<CityResult>();

                for (int i = 0; i < results.Count; i++)
                {
                    JsonObject city = results[i].GetObject();

                    string name = city.GetNamedString("name", "Unknown");
                    string country = city.GetNamedString("country", "");
                    string adminone = city.GetNamedString("admin1", "");
                    string admintwo = city.GetNamedString("admin2", "");
                    double lat = city.GetNamedNumber("latitude");
                    double lon = city.GetNamedNumber("longitude");

                    list.Add(new CityResult
                    {
                        Name = name,
                        Country = country,
                        AdminOne = adminone,
                        AdminTwo = admintwo,
                        Latitude = lat,
                        Longitude = lon
                    });
                }
                ResultsList.ItemsSource = list;
            }
            catch (Exception ex)
            {
                await new MessageDialog("Error loading cities: " + ex.Message).ShowAsync();
            }
        }


        private void SearchBox_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Enter)
            {
                e.Handled = true;
                SearchCityAsync(SearchBox.Text);
            }
        }

        public NavigationHelper NavigationHelper
        {
            get { return this.navigationHelper; }
        }

        public ObservableDictionary DefaultViewModel
        {
            get { return this.defaultViewModel; }
        }
        private void NavigationHelper_LoadState(object sender, LoadStateEventArgs e)
        {
        }

        private void NavigationHelper_SaveState(object sender, SaveStateEventArgs e)
        {
        }

        #region NavigationHelper registration

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            this.navigationHelper.OnNavigatedTo(e);
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            this.navigationHelper.OnNavigatedFrom(e);
        }

        #endregion
    }
    public class CityResult
    {
        public string Name { get; set; }
        public string Country { get; set; }
        public string AdminOne { get; set; }
        public string AdminTwo { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }

        public override string ToString()
        {
            return Name + (string.IsNullOrEmpty(Country) ? "" : ", " + Country) + ", " + AdminOne + ", " + AdminTwo;
        }
    }
}
