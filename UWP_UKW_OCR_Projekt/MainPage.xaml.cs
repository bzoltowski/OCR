using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.Globalization;
using Windows.Media.Ocr;
using Windows.Media.Capture;
using Windows.Storage;
using Windows.UI.Xaml.Media.Imaging;
using Windows.Storage.Streams;
using Windows.Graphics.Imaging;

namespace OCR_v1
{

    public sealed partial class MainPage : Page
    {

        private void UpdateLanguage()
        {

            foreach (var Lang in OcrEngine.AvailableRecognizerLanguages)
            {
                
                LanguageCBox.ItemsSource = OcrEngine.AvailableRecognizerLanguages;
            }
            
            string klucz = LanguageCBox.Name;
            if (ApplicationData.Current.LocalSettings.Values.ContainsKey(klucz))
            {
                LanguageCBox.SelectedIndex = (int)ApplicationData.Current.LocalSettings.Values[klucz];
            }
            else
                LanguageCBox.SelectedIndex = 0;
        }
        public MainPage()
        {
            this.InitializeComponent();
            BitmapLang.ocrEngine = null;
            BitmapLang.Name = null;
            BitmapLang.softwareBitmap = null;
            
            UpdateLanguage();

        }

        private void OCEBtn_Click(object sender, RoutedEventArgs e)
        {
            BitmapLang.ocrEngine = OcrEngine.TryCreateFromLanguage(LanguageCBox.SelectedValue as Language);
            this.Frame.Navigate(typeof(OCRMagic));
        }
        async void ZrobZdjecie()
        {
            //imgPodglad.Visibility = Visibility.Visible;

            CameraCaptureUI zdjecie = new CameraCaptureUI();
            //BitmapImage bitmapImage = new BitmapImage();
            SoftwareBitmapSource bitmapSource = new SoftwareBitmapSource();
            SoftwareBitmap softwareBitmap = null;
            IRandomAccessStream plikStream;

            StorageFile plik = await zdjecie.CaptureFileAsync(CameraCaptureUIMode.Photo);
            if (plik != null)
            {
                using (plikStream = await plik.OpenAsync(FileAccessMode.Read))
                {
                    //bitmapImage.SetSource(plikStream);
                    BitmapDecoder decoder = await BitmapDecoder.CreateAsync(plikStream);
                    softwareBitmap = await decoder.GetSoftwareBitmapAsync(BitmapPixelFormat.Bgra8, BitmapAlphaMode.Premultiplied);
                   
                    //SoftwareBitmap softwareBitmapBGR8 = SoftwareBitmap.Convert(softwareBitmap, BitmapPixelFormat.Bgra8, BitmapAlphaMode.Premultiplied);
                    //await bitmapSource.SetBitmapAsync(softwareBitmapBGR8);
                }
                //imgPodglad.Source = bitmapImage;
                //txtbSciezka.Text = plik.Path;

                BitmapLang.ocrEngine = OcrEngine.TryCreateFromLanguage(LanguageCBox.SelectedValue as Language);
                BitmapLang.softwareBitmap = softwareBitmap;
                
                await Windows.Storage.ApplicationData.Current.LocalFolder.CreateFolderAsync("OCR_UKW_Project", Windows.Storage.CreationCollisionOption.OpenIfExists);
                plik = await plik.CopyAsync(await ApplicationData.Current.LocalFolder.GetFolderAsync("OCR_UKW_Project"), plik.Name, NameCollisionOption.GenerateUniqueName);
                BitmapLang.Name = plik.Name;
                this.Frame.Navigate(typeof(OCRMagic));
                //imgPodglad.Source = bitmapSource;

            }
            else
            {
                Windows.UI.Popups.MessageDialog msg = new Windows.UI.Popups.MessageDialog("Nie zrobiono zdjecia.");
                await msg.ShowAsync();
            }
        }
        private void TPicBtn_Click(object sender, RoutedEventArgs e)
        {
            ZrobZdjecie();
        }

        private void InfoBtn_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(Information));
        }

        private void HistoryBtn_Click(object sender, RoutedEventArgs e)
        {
            BitmapLang.ocrEngine = OcrEngine.TryCreateFromLanguage(LanguageCBox.SelectedValue as Language);
            this.Frame.Navigate(typeof(UserHistory));
        }

        private void LanguageCBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string klucz = (sender as ComboBox).Name;
            int index = (sender as ComboBox).SelectedIndex;
            ApplicationData.Current.LocalSettings.Values[klucz] = index;
        }
    }
}
