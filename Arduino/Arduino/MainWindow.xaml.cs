using System;
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
using System.Timers;
using System.IO.Ports;
using System.IO;

namespace Arduino
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        Timer aTimer;
        SerialPort currentPort;
        private delegate void updateDelegate(string txt);
        public MainWindow()
        {
            InitializeComponent();
            
        }
        private bool ArduinoDetected()
        {
            try
            {
                currentPort.Open();
                System.Threading.Thread.Sleep(1000);
                // небольшая пауза, ведь SerialPort не терпит суеты
                currentPort.Write("abc");
                string returnMessage = currentPort.ReadLine();
                currentPort.Close();

                // необходимо чтобы void loop() в скетче содержал код Serial.println("Info from Arduino");
                if (!String.IsNullOrEmpty(returnMessage))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception e)
            {
                return false;
            }
        }
        private void OnTimedEvent(object sender, ElapsedEventArgs e)
        {
            if (!currentPort.IsOpen) return;
            try // так как после закрытия окна таймер еще может выполнится или предел ожидания может быть превышен
            {
                // удалим накопившееся в буфере
                currentPort.DiscardInBuffer();
                // считаем последнее значение 
                string strFromPort = currentPort.ReadLine();
                lblPortData.Dispatcher.BeginInvoke(new updateDelegate(updateTextBox), strFromPort);
            }
            catch { }
        }

        private void updateTextBox(string txt)
        {
            lblPortData.Content = txt;
        }
        private void btnOne_Click(object sender, RoutedEventArgs e)
        {
            if (!currentPort.IsOpen) return;
            byte[] kek = File.ReadAllBytes("d:\\gg.txt");
            currentPort.Write(kek, 0, kek.Length);
        }

        private void btnZero_Click(object sender, RoutedEventArgs e)
        {
            if (!currentPort.IsOpen) return;
            currentPort.Write("abcdef");
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            bool ArduinoPortFound = false;

            try
            {
                string[] ports = SerialPort.GetPortNames();
                foreach (string port in ports)
                {
                    currentPort = new SerialPort(port, 9600);
                    if (ArduinoDetected())
                    {
                        ArduinoPortFound = true;
                        break;
                    }
                    else
                    {
                        ArduinoPortFound = false;
                    }
                }
            }
            catch { }

            if (ArduinoPortFound == false) return;
            System.Threading.Thread.Sleep(500); // немного подождем

            currentPort.BaudRate = 9600;
            currentPort.DtrEnable = true;
            currentPort.ReadTimeout = 1000;
            try
            {
                currentPort.Open();
            }
            catch { }

            aTimer = new System.Timers.Timer(1000);
            aTimer.Elapsed += OnTimedEvent;
            aTimer.AutoReset = true;
            aTimer.Enabled = true;
        }

        private void Window_Closing_1(object sender, System.ComponentModel.CancelEventArgs e)
        {
            aTimer.Enabled = false;
            currentPort.Close();
        }
    }
}
