using System.ComponentModel;
using Windows.Foundation;
using Windows.Media.Ocr;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media;

namespace OCR_v1
{
    class WordOverlay : INotifyPropertyChanged
    {
        private OcrWord word;

        // Lewe i gorne ustawienie wordbox'a otrzymane z ocr'a
        public Thickness WordPosition { get; private set; }

        //Skalowanie szerkosci
        public double WordWidth { get; private set; }

        //Skalowanie wysokosci
        public double WordHeight { get; private set; }

        public event PropertyChangedEventHandler PropertyChanged;

        public WordOverlay(OcrWord ocrWord)
        {
            word = ocrWord;

            UpdateProps(word.BoundingRect);
        }

        public void Transform(ScaleTransform scale)
        {
            // Skalowanie i zaktualizowanie wymiarow
            UpdateProps(scale.TransformBounds(word.BoundingRect));
        }

        public Binding CreateWordPositionBinding()
        {
            Binding positionBinding = new Binding()
            {
                Path = new PropertyPath("WordPosition"),
                Source = this
            };

            return positionBinding;
        }

        public Binding CreateWordWidthBinding()
        {
            Binding widthBinding = new Binding()
            {
                Path = new PropertyPath("WordWidth"),
                Source = this
            };

            return widthBinding;
        }

        public Binding CreateWordHeightBinding()
        {
            Binding heightBinding = new Binding()
            {
                Path = new PropertyPath("WordHeight"),
                Source = this
            };

            return heightBinding;
        }

        private void UpdateProps(Rect wordBoundingBox)
        {
            WordPosition = new Thickness(wordBoundingBox.Left, wordBoundingBox.Top, 0, 0);
            WordWidth = wordBoundingBox.Width;
            WordHeight = wordBoundingBox.Height;

            OnPropertyChanged("WordPosition");
            OnPropertyChanged("WordWidth");
            OnPropertyChanged("WordHeight");
        }

        protected void OnPropertyChanged(string PropertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(PropertyName));
            }
        }
    }
}
