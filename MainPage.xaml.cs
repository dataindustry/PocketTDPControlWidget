using PocketTDPControlWidget.MainServiceReference;
using System;
using System.Collections.Concurrent;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace PocketTDPControlWidget
{
    public sealed partial class MainPage : Page
    {
        private TDPViewModel ViewModel;
        private ApplicationDataContainer localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;

        ConcurrentQueue<int> TDPQueue = new ConcurrentQueue<int>();
        ConcurrentQueue<bool> ResultQueue = new ConcurrentQueue<bool>();

        public MainPage()
        {
            this.InitializeComponent();

            ApplicationDataCompositeValue composite = (ApplicationDataCompositeValue)localSettings.Values[nameof(this.ViewModel)];
            this.ViewModel = new TDPViewModel();

            if (composite != null)
            {
                this.ViewModel.CurrentTDP = (int)composite[nameof(this.ViewModel.CurrentTDP)];
                this.ViewModel.PresetTDP = (int)composite[nameof(this.ViewModel.PresetTDP)];
                this.ViewModel.ReadingTDP = (int)composite[nameof(this.ViewModel.ReadingTDP)];
            }
            else
            {
                composite = new ApplicationDataCompositeValue();
                composite[nameof(this.ViewModel.CurrentTDP)] = this.ViewModel.CurrentTDP;
                composite[nameof(this.ViewModel.PresetTDP)] = this.ViewModel.PresetTDP;
                composite[nameof(this.ViewModel.ReadingTDP)] = this.ViewModel.ReadingTDP;
                localSettings.Values[nameof(this.ViewModel)] = composite;
            }

            Task t = new Task(() =>
            {
                while (true) {

                    if (TDPQueue.IsEmpty) {
                        Thread.Sleep(100);
                        continue;
                    }

                    TDPQueue.TryDequeue(out var tdp);

                    var client = new MainServiceClient();

                    client.AdjustAsync("a", tdp);
                    client.AdjustAsync("b", tdp);
                    client.AdjustAsync("c", tdp);

                }
            });
            t.Start();

            this.DataContext = this.ViewModel;
            this.ViewModel.PropertyChanged += OnPropertyChanged;

        }

        private void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            TDPQueue.Enqueue(this.ViewModel.CurrentTDP);
        }

        private void AdjustButton_Click(object sender, RoutedEventArgs e)
        {
            Button clickedButton = sender as Button;
            int tdp = Convert.ToInt32(clickedButton.Content.ToString());
            this.ViewModel.CurrentTDP = tdp;
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            this.ViewModel.PresetTDP = this.ViewModel.CurrentTDP;
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            ApplicationDataCompositeValue composite = (ApplicationDataCompositeValue)localSettings.Values[nameof(this.ViewModel)];
            composite[nameof(this.ViewModel.CurrentTDP)] = this.ViewModel.CurrentTDP;
            composite[nameof(this.ViewModel.PresetTDP)] = this.ViewModel.PresetTDP;
            composite[nameof(this.ViewModel.ReadingTDP)] = this.ViewModel.ReadingTDP;
            localSettings.Values[nameof(this.ViewModel)] = composite;
        }

    }
}
