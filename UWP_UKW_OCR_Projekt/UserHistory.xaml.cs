using System;
using System.Collections.Generic;
using System.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.Graphics.Imaging;
using Windows.Storage;
using Windows.UI.Xaml.Media.Imaging;
using Windows.Storage.Streams;

namespace OCR_v1
{
    public sealed partial class UserHistory : Page
    {
        List<string> Logs = new List<string>();
        private async void UpdateUserHistory()
        {
            StorageFolder localFolder = await Windows.Storage.ApplicationData.Current.LocalFolder.CreateFolderAsync("OCR_UKW_Project", Windows.Storage.CreationCollisionOption.OpenIfExists);
            StorageFile DataFile = await localFolder.CreateFileAsync("OCR_UKW_Project_UserData.txt", CreationCollisionOption.OpenIfExists);

            var Data = await FileIO.ReadLinesAsync(DataFile);
            if (Data.ToString() == "" || Data.LongCount() == 0 )
            {
                Windows.UI.Popups.MessageDialog msg = new Windows.UI.Popups.MessageDialog("Twoja historia jest pusta.");
                await msg.ShowAsync();
                BitmapLang.ocrEngine = null;
                BitmapLang.Name = null;
                BitmapLang.softwareBitmap = null;
                this.Frame.Navigate(typeof(MainPage));
            }
            else
            {
                foreach (var line in Data)
                {
                    int tmp = DateTimeOffset.Now.ToString().Length;
                    LBoxHistory.Items.Add(line.Substring(0,19)+" - "+line.Substring(tmp));
                    Logs.Add(line.Substring(tmp));

                }
                string klucz = LBoxHistory.Name;
                if (ApplicationData.Current.LocalSettings.Values.ContainsKey(klucz))
                {
                    LBoxHistory.SelectedIndex = (int)ApplicationData.Current.LocalSettings.Values[klucz];
                }
                else
                    LBoxHistory.SelectedIndex = 0;


            }
        }
        public UserHistory()
        {
            this.InitializeComponent();
            UpdateUserHistory();
        }

        private void BackToMenuBtn_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(MainPage));
        }

        private async void GoToOCR_Click(object sender, RoutedEventArgs e)
        {
            if (Logs[LBoxHistory.SelectedIndex]!="")
            {
                try { 
                    IRandomAccessStream plikStream;
                    StorageFolder localFolder = await Windows.Storage.ApplicationData.Current.LocalFolder.CreateFolderAsync("OCR_UKW_Project", Windows.Storage.CreationCollisionOption.OpenIfExists);
                    StorageFile file = await localFolder.GetFileAsync(Logs[LBoxHistory.SelectedIndex]);
                    using (plikStream = await file.OpenAsync(FileAccessMode.Read))
                    {
                        BitmapDecoder decoder = await BitmapDecoder.CreateAsync(plikStream);
                        BitmapLang.softwareBitmap = await decoder.GetSoftwareBitmapAsync(BitmapPixelFormat.Bgra8, BitmapAlphaMode.Premultiplied);
                    }
                    BitmapLang.Name = Logs[LBoxHistory.SelectedIndex];
                    this.Frame.Navigate(typeof(OCRMagic));
                }
                catch(UnauthorizedAccessException)
                {
                    BitmapLang.ocrEngine = null;
                    BitmapLang.Name = null;
                    BitmapLang.softwareBitmap = null;
                    Windows.UI.Popups.MessageDialog msg = new Windows.UI.Popups.MessageDialog("UnauthorizedAccessException: Nie można uzyskać dostępu do pliku.");
                    await msg.ShowAsync();
                    
                }
                catch (Exception ex)
                {
                    Windows.UI.Popups.MessageDialog msg = new Windows.UI.Popups.MessageDialog(ex.ToString());
                    await msg.ShowAsync();
                }
            }
            else
            {
                Windows.UI.Popups.MessageDialog msg = new Windows.UI.Popups.MessageDialog("Brak szczegółowych informacji na temat pliku.");
                await msg.ShowAsync();
            }
        }

        private async void CleanHistoryBtn_Click(object sender, RoutedEventArgs e)
        {
            Windows.Storage.StorageFolder localFolder = await Windows.Storage.ApplicationData.Current.LocalFolder.CreateFolderAsync("OCR_UKW_Project", Windows.Storage.CreationCollisionOption.OpenIfExists);
            var DataFile = await localFolder.CreateFileAsync("OCR_UKW_Project_UserData.txt", CreationCollisionOption.OpenIfExists);

            try
            {
                StorageFolder OCRFolder = await ApplicationData.Current.LocalFolder.GetFolderAsync("OCR_UKW_Project");
                
                if (OCRFolder != null)
                {
                    await OCRFolder.DeleteAsync(StorageDeleteOption.PermanentDelete);
                    ApplicationData.Current.LocalSettings.Values[LBoxHistory.Name] = 0;

                }
            }
            catch (Exception ex)
            {

                Windows.UI.Popups.MessageDialog msg = new Windows.UI.Popups.MessageDialog(ex.ToString());
                await msg.ShowAsync();

            }
            LBoxHistory.Items.Clear();
            this.Frame.Navigate(typeof(MainPage));
        }

        private async void LBoxHistory_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (LBoxHistory.Items.Count == 0)
            {
                return;
            }
            string klucz = (sender as ListBox).Name;
            int index = (sender as ListBox).SelectedIndex;
            ApplicationData.Current.LocalSettings.Values[klucz] = index;
            try
            {


                BitmapImage bitmapImage = new BitmapImage();
                IRandomAccessStream plikStream;
                StorageFolder localFolder = await Windows.Storage.ApplicationData.Current.LocalFolder.CreateFolderAsync("OCR_UKW_Project", Windows.Storage.CreationCollisionOption.OpenIfExists);
                StorageFile file = await localFolder.GetFileAsync(Logs[LBoxHistory.SelectedIndex]);
                using (plikStream = await file.OpenAsync(FileAccessMode.Read))
                {
                    bitmapImage.SetSource(plikStream);

                }
                imgView.Source = bitmapImage;

            }
            catch (Exception ex)
            {
                Windows.UI.Popups.MessageDialog msg = new Windows.UI.Popups.MessageDialog(ex.ToString());
                await msg.ShowAsync();
            }
        }

    }
}
