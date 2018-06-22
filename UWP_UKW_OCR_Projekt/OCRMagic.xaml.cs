using System;
using System.Collections.Generic;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using System.Threading.Tasks;
using Windows.Graphics.Imaging;
using Windows.Media.Ocr;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.UI.Xaml.Media.Imaging;

namespace OCR_v1
{

    public sealed partial class OCRMagic : Page
    {
        private SoftwareBitmap bitmap;
        StorageFile DataFile;
        private List<WordOverlay> wordBoxes = new List<WordOverlay>();
        OcrEngine ocrEngine = null;
        private async Task LoadImage(StorageFile file)
        {
            
            using (var stream = await file.OpenAsync(Windows.Storage.FileAccessMode.Read))
            {
                var decoder = await BitmapDecoder.CreateAsync(stream);
               
                bitmap = await decoder.GetSoftwareBitmapAsync(BitmapPixelFormat.Bgra8, BitmapAlphaMode.Premultiplied);

                var imgSource = new WriteableBitmap(bitmap.PixelWidth, bitmap.PixelHeight);
                bitmap.CopyToBuffer(imgSource.PixelBuffer);
                PreviewImage.Source = imgSource;
            }

            OCR();
        }
        private async void PhotoPicker()
        {
            var picker = new FileOpenPicker()
            {
                SuggestedStartLocation = PickerLocationId.PicturesLibrary,
                FileTypeFilter = { ".jpg", ".jpeg", ".png" },
            };
            
            var file = await picker.PickSingleFileAsync();
            if (file != null)
            {
                
                await Windows.Storage.ApplicationData.Current.LocalFolder.CreateFolderAsync("OCR_UKW_Project", Windows.Storage.CreationCollisionOption.OpenIfExists);
                file = await file.CopyAsync(await ApplicationData.Current.LocalFolder.GetFolderAsync("OCR_UKW_Project"), file.Name, NameCollisionOption.GenerateUniqueName);
                BitmapLang.Name = file.Name;
                await LoadImage(file);
            }
            if (file == null)
            {
                Windows.UI.Popups.MessageDialog msg = new Windows.UI.Popups.MessageDialog("Nie wybrano zdjecia.");
                await msg.ShowAsync();
                this.Frame.Navigate(typeof(MainPage));
            }
        }
        private async void UserOption()
        {
            if (BitmapLang.ocrEngine != null && BitmapLang.softwareBitmap != null)
            {
                //Aparat
                ocrEngine = BitmapLang.ocrEngine;
                bitmap = BitmapLang.softwareBitmap;
                var imgSource = new WriteableBitmap(bitmap.PixelWidth, bitmap.PixelHeight);
                bitmap.CopyToBuffer(imgSource.PixelBuffer);
                PreviewImage.Source = imgSource;
                OCR();
                
            }
            if (BitmapLang.ocrEngine != null && BitmapLang.softwareBitmap == null)
            {
                //Zdjecie

                ocrEngine = BitmapLang.ocrEngine;
                PhotoPicker();
            }
            if (BitmapLang.ocrEngine == null && BitmapLang.softwareBitmap == null )
            {
                Windows.UI.Popups.MessageDialog msg = new Windows.UI.Popups.MessageDialog("Coś poszło źle, spróbuj ponownie. :(");
                await msg.ShowAsync();
                this.Frame.Navigate(typeof(MainPage));
            }
        }
        
        public OCRMagic()
        {
            this.InitializeComponent();
            UserOption();
        }
        private void UpdateWordBoxTransform()
        {
            //Zmiana podgladu do wymiarow okna urzadzenia
            ScaleTransform scaleTrasform = new ScaleTransform
            {
                CenterX = 0,
                CenterY = 0,
                ScaleX = PreviewImage.ActualWidth / bitmap.PixelWidth,
                ScaleY = PreviewImage.ActualHeight / bitmap.PixelHeight
            };

            foreach (var item in wordBoxes)
            {
                item.Transform(scaleTrasform);
            }
        }


        private void PreviewImage_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            UpdateWordBoxTransform();

            
            var rotate = TextOverlay.RenderTransform as RotateTransform;
            if (rotate != null)
            {
                rotate.CenterX = PreviewImage.ActualWidth / 2;
                rotate.CenterY = PreviewImage.ActualHeight / 2;
            }
        }
        private async void OCR()
        {
            if (bitmap.PixelWidth > OcrEngine.MaxImageDimension || bitmap.PixelHeight > OcrEngine.MaxImageDimension)
            {
                BitmapLang.ocrEngine = null;
                BitmapLang.Name = null;
                BitmapLang.softwareBitmap = null;
                Windows.UI.Popups.MessageDialog msg = new Windows.UI.Popups.MessageDialog("Wymiary tego zdjęcia są zbyt duże, użyj mniejszego.");
                await msg.ShowAsync();
                this.Frame.Navigate(typeof(MainPage));
            }


            if (ocrEngine != null)
            {

                // Rozpoznawanie tekstu z bitmapy
                var ocrResult = await ocrEngine.RecognizeAsync(bitmap);
                
                if (ocrResult==null || ocrResult.Text=="")
                {
                    BitmapLang.ocrEngine = null;
                    BitmapLang.Name = null;
                    BitmapLang.softwareBitmap = null;
                    Windows.UI.Popups.MessageDialog msg = new Windows.UI.Popups.MessageDialog("Nasz algorytm uznał, że nie ma na tym zdjęciu żadnego tekstu." );
                    await msg.ShowAsync();
                    this.Frame.Navigate(typeof(MainPage));
                }

                Windows.Storage.StorageFolder localFolder = await Windows.Storage.ApplicationData.Current.LocalFolder.CreateFolderAsync("OCR_UKW_Project", Windows.Storage.CreationCollisionOption.OpenIfExists);
                DataFile = await localFolder.CreateFileAsync("OCR_UKW_Project_UserData.txt", CreationCollisionOption.OpenIfExists);

                string tmpData = "";
                try { tmpData = await FileIO.ReadTextAsync(DataFile); }
                catch { tmpData = ""; }
                tmpData = tmpData + DateTimeOffset.Now + BitmapLang.Name + "\n";
                await FileIO.WriteTextAsync(DataFile, tmpData);
                // Wyświetlenie tekstu
                ExtractedTextBox.Text = ocrResult.Text;

                if (ocrResult.TextAngle != null)
                {
                    // Jeżeli tekst jest pod katem to obracamy grida
                    TextOverlay.RenderTransform = new RotateTransform
                    {
                        Angle = (double)ocrResult.TextAngle,
                        CenterX = PreviewImage.ActualWidth / 2,
                        CenterY = PreviewImage.ActualHeight / 2
                    };
                }

                // Tworzenie zaznaczeń dla otrzymanych słów
                foreach (var line in ocrResult.Lines)
                {
                    Rect lineRect = Rect.Empty;
                    foreach (var word in line.Words)
                    {
                        lineRect.Union(word.BoundingRect);
                    }

                    // Ustalenia czy tekst jest w pionie czy poziomie (pion np. dla chin).
                    bool isVerticalLine = lineRect.Height > lineRect.Width;

                    foreach (var word in line.Words)
                    {
                        WordOverlay wordBoxOverlay = new WordOverlay(word);

                        // dodanie do listy
                        wordBoxes.Add(wordBoxOverlay);

                        // Jaki typ tekstu
                        var overlay = new Border()
                        {
                            Style = isVerticalLine ?
                                        (Style)this.Resources["HighlightedWordBoxVerticalLine"] :
                                        (Style)this.Resources["HighlightedWordBoxHorizontalLine"]
                        };

                        // Przypisanie word box'a do border
                        overlay.SetBinding(Border.MarginProperty, wordBoxOverlay.CreateWordPositionBinding());
                        overlay.SetBinding(Border.WidthProperty, wordBoxOverlay.CreateWordWidthBinding());
                        overlay.SetBinding(Border.HeightProperty, wordBoxOverlay.CreateWordHeightBinding());

                        // Dodaje box'a do grid'a
                        TextOverlay.Children.Add(overlay);
                    }
                }

                // Rescale word boxes to match current UI size.
                UpdateWordBoxTransform();

            }
            else
            {
                Windows.UI.Popups.MessageDialog msg = new Windows.UI.Popups.MessageDialog("Wybrany język nie jest dostępny. Spróbuj ponownie.");
                await msg.ShowAsync();
            }
        }

        private void BackToMenuBtn_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(MainPage));
        }

        private async void GoToHistory_Click(object sender, RoutedEventArgs e)
        {

            Windows.UI.Popups.MessageDialog msg = new Windows.UI.Popups.MessageDialog( ApplicationData.Current.LocalFolder.Path);
            await msg.ShowAsync();
            this.Frame.Navigate(typeof(UserHistory));
           
        }
    }
}
