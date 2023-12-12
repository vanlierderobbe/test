using Microcharts;
using SkiaSharp;

namespace MauiApp1;

public partial class MainPage1 : ContentPage
{
    private Dictionary<string, int> colorCounts = new Dictionary<string, int>
    {
        { "rood", 0 },
        { "groen", 0 },
        { "blauw", 0 },
        { "geel", 0 },
        { "oranje", 0 },
        { "bruin", 0 }
    };
    private bool isConnected = true;
    private bool isSubscribed = false;

    public MainPage1()
    {
        InitializeComponent();
        UpdateButtonLabel();

        startButton.IsEnabled = false;
        resetButton.IsEnabled = false;

        // Abonneer op het ConnectionStatusChanged-event van MainPage2
        MessagingCenter.Subscribe<MainPage2, bool>(this, "ConnectionStatusChanged", (sender, isConnected) =>
        {
            this.isConnected = isConnected;
            UpdateUIBasedOnConnection();
        });
    }

    private void UpdateUIBasedOnConnection()
    {
        // Activeer of deactiveer de Start en Reset knoppen op basis van de verbindingsstatus
        startButton.IsEnabled = isConnected;
        resetButton.IsEnabled = isConnected;
    }

    private void OnStartButtonClicked(object sender, EventArgs e)
    {
        if (!isSubscribed)
        {
            // Abonneer op het MessagingCenter-event als nog niet geabonneerd
            MessagingCenter.Subscribe<MainPage2, string>(this, "DataReceived", HandleReceivedData);
            isSubscribed = true;
        }
        else
        {
            // Als al geabonneerd, unsubscribe om verdere berichten te stoppen
            MessagingCenter.Unsubscribe<MainPage2, string>(this, "DataReceived");
            isSubscribed = false;
        }
        UpdateButtonLabel(); // Update de tekst van de knop na elke klik
    }

    private void HandleReceivedData(MainPage2 sender, string receivedData)
    {
        Console.WriteLine($"Ontvangen JSON-tekst: {receivedData}");

        try
        {
            // Deserialiseer de ontvangen JSON-tekst naar een object
            dynamic parsedData = Newtonsoft.Json.JsonConvert.DeserializeObject(receivedData);

            // Controleer of de eigenschap "c" aanwezig is in de ontvangen JSON-tekst
            if (parsedData != null && parsedData.c != null)
            {
                string color = parsedData.c; // Krijg de waarde van de eigenschap "c"

                // Toon de gedetecteerde kleur in het label
                colorLabel.Text = $"Gedetecteerde kleur: {color}";

                // Update de grafieken
                UpdateChart(color);
            }
            else
            {
                Console.WriteLine("Ongeldige JSON-structuur: Eigenschap 'c' ontbreekt.");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Fout bij het deserialiseren: {ex.Message}");
        }

        isConnected = true; // Stel de verbindingsstatus in op 'true' als er gegevens worden ontvangen
        UpdateConnectionStatusOnMainThread();
    }

    private void UpdateButtonLabel()
    {
        startButton.Text = isSubscribed ? "Stop" : "Start"; // Verander de tekst van de knop op basis van de huidige status
    }

    private void UpdateChart(string color)
    {
        // Voeg de kleur toe aan de colorCounts dictionary of update het aantal als de kleur al aanwezig is
        if (colorCounts.ContainsKey(color))
        {
            colorCounts[color]++;
        }
        else
        {
            colorCounts[color] = 1;
        }

        // Update de grafiek met de nieuwe data zonder animatie
        var entries = colorCounts.Select(pair => new ChartEntry(pair.Value)
        {
            Label = pair.Key,
            ValueLabel = pair.Value.ToString(),
            Color = GetColorFromString(pair.Key)
        });

        // Maak een nieuwe grafiek met de bijgewerkte entries
        var updatedChart = new PieChart { Entries = entries.ToList(), LabelTextSize = 45, BackgroundColor = SKColor.Parse("#f4f6fa") };
        var updatedChart2 = new RadarChart { Entries = entries.ToList(), LabelTextSize = 45, BackgroundColor = SKColor.Parse("#f4f6fa") };
        var updatedChart3 = new BarChart { Entries = entries.ToList(), LabelTextSize = 45, ValueLabelOrientation = Orientation.Horizontal, LabelOrientation = Orientation.Horizontal, BackgroundColor = SKColor.Parse("#f4f6fa") };
        var updatedChart4 = new LineChart { Entries = entries.ToList(), LabelTextSize = 45, ValueLabelOrientation = Orientation.Horizontal, LabelOrientation = Orientation.Horizontal, BackgroundColor = SKColor.Parse("#f4f6fa") };

        // Zet de AnimationDuration op TimeSpan.Zero om animaties uit te schakelen
        updatedChart.AnimationDuration = TimeSpan.Zero;
        updatedChart2.AnimationDuration = TimeSpan.Zero;
        updatedChart3.AnimationDuration = TimeSpan.Zero;
        updatedChart4.AnimationDuration = TimeSpan.Zero;

        chartView3.Chart = updatedChart;
        chartView.Chart = updatedChart2;
        chartView1.Chart = updatedChart3;
        chartView2.Chart = updatedChart4;

        chartView3.InvalidateSurface();
        chartView.InvalidateSurface();
        chartView1.InvalidateSurface();
        chartView2.InvalidateSurface();

    }

    private SKColor GetColorFromString(string colorName)
    {
        switch (colorName.ToLower())
        {
            case "rood":
                return SKColor.Parse("#C21D30");
            case "groen":
                return SKColor.Parse("#22C452");
            case "blauw":
                return SKColor.Parse("#1E73BE");
            case "geel":
                return SKColor.Parse("#EEE200");
            case "oranje":
                return SKColor.Parse("#F26F22");
            case "bruin":
                return SKColor.Parse("#6A4929");
            default:
                return SKColor.Empty;
        }
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();

        // Reset de verbindingsstatus wanneer de pagina wordt verlaten
        isConnected = false;
        UpdateConnectionStatusOnMainThread();
    }

    private void UpdateConnectionStatusOnMainThread()
    {
        Device.BeginInvokeOnMainThread(() =>
        {
            // Update de kleur van de BoxView op basis van de verbindingsstatus
            if (isConnected)
            {
                connectionStatusIndicator.Color = Microsoft.Maui.Graphics.Colors.Green; // Groen als verbonden
                connectionStatusLabel.Text = "Verbonden"; // Tekst voor verbonden status
            }
            else
            {
                connectionStatusIndicator.Color = Microsoft.Maui.Graphics.Colors.Red; // Rood als niet verbonden
                connectionStatusLabel.Text = "Niet verbonden"; // Tekst voor niet verbonden status
            }
        });
    }

    private void OnResetButtonClicked(object sender, EventArgs e)
    {
        // Reset de kleurtellingen naar 0
        foreach (var key in colorCounts.Keys.ToList())
        {
            colorCounts[key] = 0;
        }

        // Reset de verbindingssatus
        isConnected = false;
        UpdateConnectionStatusOnMainThread();
    }
}