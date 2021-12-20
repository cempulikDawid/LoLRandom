using System;
using System.IO;
using System.Text.Json;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using HtmlAgilityPack;

namespace LoLRandom
{
    public partial class MainWindow : Window
    {
        Random random;
        ImageSourceConverter sourceConverter;

        List<RadioButton> lines = new List<RadioButton>();

        public MainWindow()
        {
            random = new Random();
            sourceConverter = new ImageSourceConverter();

            InitializeComponent();
            lines.Add(radioTop);
            lines.Add(radioJungle);
            lines.Add(radioMid);
            lines.Add(radioBot);
            lines.Add(radioSupport);
        }

        private void buttonLosuj_Click(object sender, RoutedEventArgs e)
        {
            if (App.champList.champions.Count > 0)
            {
                Champion champion = App.champList.champions.First();

                if (radioDowolna.IsChecked == true)
                {
                    champion = App.champList.champions[random.Next(App.champList.champions.Count)];
                }
                else if (radioWybrana.IsChecked == true)
                {
                    if(radioTop.IsChecked == true) champion = App.champList.topChampions[random.Next(App.champList.topChampions.Count)];
                    else if(radioJungle.IsChecked == true) champion = App.champList.jungleChampions[random.Next(App.champList.jungleChampions.Count)];
                    else if (radioMid.IsChecked == true) champion = App.champList.midChampions[random.Next(App.champList.midChampions.Count)];
                    else if (radioBot.IsChecked == true) champion = App.champList.botChampions[random.Next(App.champList.botChampions.Count)];
                    else if (radioSupport.IsChecked == true) champion = App.champList.supportChampions[random.Next(App.champList.supportChampions.Count)];
                }

                nazwaPostaci.Content = champion.name;

                if (File.Exists(App.imagesFilePath + champion.id + ".jpg"))
                {
                    zdjeciePostaci.Source = (ImageSource)sourceConverter.ConvertFromString(App.imagesFilePath + champion.id + ".jpg");
                }
                else
                {
                    zdjeciePostaci.Source = (ImageSource)sourceConverter.ConvertFromString(App.imagesURL + champion.id + "_0.jpg");
                }
            }
        }

        private void radioDowolna_Checked(object sender, RoutedEventArgs e)
        {
            foreach (RadioButton radio in lines)
            {
                radio.Visibility = Visibility.Collapsed;
            }
        }

        private void radioWybrana_Checked(object sender, RoutedEventArgs e)
        {
            foreach (RadioButton radio in lines)
            {
                radio.Visibility = Visibility.Visible;
            }
        }
    }
}
