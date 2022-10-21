using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
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
using System.Xml.Linq;
using Windows.Storage;

//Szablon elementu Pusta strona jest udokumentowany na stronie https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x415

namespace KalkulatorWalutArtemKuznetsov
{
    /// <summary>
    /// Pusta strona, która może być używana samodzielnie lub do której można nawigować wewnątrz ramki.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        const string daneNBP = "http://www.nbp.pl/kursy/xml/LastA.xml";
        List<PozycjaTabeliA> kursyAktualne = new List<PozycjaTabeliA>();

        public MainPage()
        {
            this.InitializeComponent();
        }

        public class PozycjaTabeliA
        {
            public string przelicznik { get; set; }
            public string kod_waluty { get; set; }
            public string kurs_sredni { get; set; }
        }


        private async void GContentPanel_Loaded(object sender, RoutedEventArgs e)
        {
            var serwerNBP = new HttpClient();
            string dane = "";

            try
            {
                dane = await serwerNBP.GetStringAsync(new Uri(daneNBP));
            }

            catch (Exception ex) { }
            if (dane != null)
            { kursyAktualne.Clear(); }


            XDocument daneKursowe = XDocument.Parse(dane);
            kursyAktualne = (from item in daneKursowe.Descendants("pozycja")
                            select new PozycjaTabeliA()
                            {
                                przelicznik = (item.Element("przelicznik").Value),
                                kod_waluty = (item.Element("kod_waluty").Value),
                                kurs_sredni = (item.Element("kurs_sredni").Value),

                            }).ToList();
            kursyAktualne.Insert(0, new PozycjaTabeliA() { kurs_sredni = "1,0000", kod_waluty = "PLN", przelicznik = "1" });


            lbxZWaluty.ItemsSource = kursyAktualne;
            lbxNaWalute.ItemsSource = kursyAktualne;
            if (ApplicationData.Current.LocalSettings.Values.ContainsKey("zWaluty") && ApplicationData.Current.LocalSettings.Values.ContainsKey("naWalute"))
            {
                lbxZWaluty.SelectedIndex = (int)ApplicationData.Current.LocalSettings.Values["zWaluty"];
                lbxNaWalute.SelectedIndex = (int)ApplicationData.Current.LocalSettings.Values["naWalute"];
            }
            else
            {
                lbxZWaluty.SelectedIndex = -1;
                lbxNaWalute.SelectedIndex = -1;
            }
        }

        private void txtKwota_TextChanged(object sender, TextChangedEventArgs e)
        {
            Przewalutuj();
        }

        public void Przewalutuj()
        {
            double wynik = 0;
            if (txtKwota.Text.Length > 0)
            {
                double liczba1 = double.Parse(txtKwota.Text);
                double kurs1 = double.Parse(kursyAktualne[lbxZWaluty.SelectedIndex].kurs_sredni.Replace(",", "."), System.Globalization.CultureInfo.InvariantCulture);
                double kurs2 = double.Parse(kursyAktualne[lbxNaWalute.SelectedIndex].kurs_sredni.Replace(",", "."), System.Globalization.CultureInfo.InvariantCulture);

                wynik = (liczba1 * kurs1) / kurs2;

                TbPrzeliczona.Text = wynik.ToString();
            }
        }

        private void lbxZWaluty_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Przewalutuj();
            ApplicationData.Current.LocalSettings.Values["zWaluty"] = lbxZWaluty.SelectedIndex;
        }

        private void lbxNaWalute_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Przewalutuj();
            ApplicationData.Current.LocalSettings.Values["NaWalute"] = lbxNaWalute.SelectedIndex;
        }

        private async void ButKoniec_Click(object sender, RoutedEventArgs e)
        {
            ContentDialog KoniecBut = new ContentDialog
            {
                Title = "Na pewno chcesz zamknąć program?",
                CloseButtonText = "Nie",
                PrimaryButtonText = "Tak",

            };
            ContentDialogResult result = await KoniecBut.ShowAsync();
            if (result == ContentDialogResult.Primary)
                Environment.Exit(0);
        }

        private async void ButOprog_Click(object sender, RoutedEventArgs e)
        {
            ContentDialog OprogBut = new ContentDialog
    {
        Title = "O Programie",
        Content = "Artem Kuznetsov, Informatyka, 2 rok",
        CloseButtonText = "Ok",
    };
            _ = await OprogBut.ShowAsync();
        }
    }
}
