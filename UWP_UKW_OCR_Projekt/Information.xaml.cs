using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace OCR_v1
{
    public sealed partial class Information : Page
    {
        public Information()
        {
            this.InitializeComponent();
            InfoTxtBox.Text = @"Informacje ogólne:
Aplikacja ma na celu próbę odczytania tekstu z wybranego lub zrobionego zdjęcia przez użytkownika. Taka funkcja przydaje się przy szybkim zamienieniu tekstu w rzeczywistości na postać elektroniczną.
Informacje dotyczące aplikacji:
Wybór języka w głównym oknie programu polega na ustaleniu priorytetu szukanych słów w dalszej części korzystania z aplikacji.
Pierwsza opcja(od strony lewej) [Ikona przypominająca dokument] pozwala wybrać zdjęcie z pamięci danego urządzenia i przejście do strony gdzie odbędzie się próba uzyskania tekstu.
Druga opcja[Ikona aparatu] pozwala zrobić zdjęcie za pomocą wbudowanego w dane urządzenie aparatu(o ile taki aparat jest dostępny) a następnie  przejście do strony gdzie odbędzie się próba uzyskania tekstu.
Trzecia opcja[Ikona znaku zapytania] pozwala przejść do strony zawierającej informacje o aplikacji.
Czwarta opcja[Ikona przypominająca wypisane wyniki] pozwala przejść do strony zawierającej historię wszystkich prób wydobycia tekstu.";
        }

        private void BackBtn_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(MainPage));
        }
    }
}
