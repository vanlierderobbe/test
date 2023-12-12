using System.Diagnostics;
using System.IO.Ports;
using System.Text;

public class SerialPortManager
{
    private SerialPort _serialPort;

    public event Action<string> DataReceived;

    public SerialPortManager(string portName, int baudRate)
    {
        _serialPort = new SerialPort(portName, baudRate);
        _serialPort.DataReceived += SerialPort_DataReceived;
        _serialPort.Open();
    }

    private StringBuilder _serialBuffer = new StringBuilder();

    private void SerialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
    {
        try
        {
            string receivedData = _serialPort.ReadExisting();
            _serialBuffer.Append(receivedData);

            while (_serialBuffer.ToString().Contains("{") && _serialBuffer.ToString().Contains("}"))
            {
                int startIndex = _serialBuffer.ToString().IndexOf("{");
                int endIndex = _serialBuffer.ToString().IndexOf("}") + 1;

                if (endIndex > startIndex)
                {
                    string completeData = _serialBuffer.ToString(startIndex, endIndex - startIndex);
                    _serialBuffer.Remove(startIndex, endIndex - startIndex);

                    Device.BeginInvokeOnMainThread(() =>
                    {
                        DataReceived?.Invoke(completeData);
                    });
                }
            }
        }
        catch (Exception ex)
        {
            // Foutafhandeling
            Debug.WriteLine($"Fout bij het lezen van de seriële poort: {ex}");
        }
    }

    public void ClosePort()
    {
        _serialPort.Close();
    }

    public bool IsPortOpen()
    {
        return (_serialPort != null && _serialPort.IsOpen);
    }
}