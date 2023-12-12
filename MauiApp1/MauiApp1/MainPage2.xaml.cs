using System.Diagnostics;
using System.IO.Ports;

namespace MauiApp1
{
    public partial class MainPage2 : ContentPage
    {
        private List<string> availablePorts;
        private SerialPortManager _serialPortManager;
        private bool IsConnected = false; // Nieuwe variabele toegevoegd om de verbindingsstatus bij te houden

        public MainPage2()
        {
            InitializeComponent();
            availablePorts = new List<string>(SerialPort.GetPortNames());

            foreach (string portName in availablePorts)
            {
                portPicker.Items.Add(portName);
            }

            UpdateButtonText();
        }

        private void OnConnectButtonClicked(object sender, EventArgs e)
        {
            connectButton.IsEnabled = false;

            try
            {
                string selectedPort = (string)portPicker.SelectedItem;
                int baudRate = 115200;

                if (IsConnected)
                {
                    _serialPortManager?.ClosePort();
                    IsConnected = false;
                }
                else
                {
                    if (_serialPortManager == null || !_serialPortManager.IsPortOpen())
                    {
                        _serialPortManager = new SerialPortManager(selectedPort, baudRate);
                        _serialPortManager.DataReceived += OnDataReceivedFromSerialPort;
                        IsConnected = true;
                    }
                }
            }
            catch (UnauthorizedAccessException ex)
            {
                // Specifieke afhandeling voor UnauthorizedAccessException
                DisplayAlert("Verbindingsfout", "Geen toegang tot de COM-poort: " + ex.Message, "OK");
            }
            catch (IOException ex)
            {
                // Specifieke afhandeling voor IOException
                DisplayAlert("Verbindingsfout", "Probleem bij het lezen van de COM-poort: " + ex.Message, "OK");
            }
            catch (Exception ex)
            {
                // Algemene foutafhandeling voor andere typen exceptions
                DisplayAlert("Fout", "Je moet een COM-poort selecteren: " + ex.Message, "OK");
            }
            finally
            {
                connectButton.IsEnabled = true;
                UpdateButtonText();
            }
        }

        // In MainPage2.xaml.cs
        private void OnDataReceivedFromSerialPort(string receivedData)
        {
            Device.BeginInvokeOnMainThread(() =>
            {
                // Update de UI van MainPage2 indien nodig
                receivedDataLabel.Text = receivedData;

                // Stuur de data naar MainPage1
                MessagingCenter.Send(this, "DataReceived", receivedData);
            });
        }

        private void UpdateConnectionStatusAndNotify()
        {
            // Stuur een bericht naar MainPage1 met de huidige verbindingsstatus
            MessagingCenter.Send(this, "ConnectionStatusChanged", IsConnected);
        }

        private void UpdateButtonText()
        {
            connectButton.Text = IsConnected ? "Verbinding verbreken" : "Verbinden";
            UpdateConnectionStatusAndNotify();
        }
    }
}
