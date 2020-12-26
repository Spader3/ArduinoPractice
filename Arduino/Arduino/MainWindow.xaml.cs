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
        byte[] currentFile = new byte[128];
        private delegate void updateDelegate(string txt);
        public MainWindow()
        {
            InitializeComponent();
            
        }
        private bool ArduinoDetected()
        {
            try
            {
                //currentPort.Open();
                //System.Threading.Thread.Sleep(1000);
                //// небольшая пауза, ведь SerialPort не терпит суеты
                //byte[] array = new byte[128];
                //array[0] = 1;
                //currentPort.Write(array, 0, array.Length);
                //string returnMessage = currentPort.ReadLine();
                //currentPort.Close();
                //// необходимо чтобы void loop() в скетче содержал код Serial.println("Info from Arduino");
                //if (!String.IsNullOrEmpty(returnMessage))
                //{
                //    return true;
                //}
                //else
                //{
                //    return false;
                //}
                return true;
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
                byte[] array = new byte[128];
                currentPort.Read(array, 0, 128);
                //arduinoResponse.Dispatcher.BeginInvoke(new updateDelegate(updateTextBox), strFromPort);
                var a = 10;
            }
            catch { }
        }

        private void updateTextBox(string txt)
        {
            arduinoResponse.Content = txt;
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

        private void Canvas_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                // Note that you can have more than one file.
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);

                // Assuming you have one file that you care about, pass it off to whatever
                // handling code you have defined.
                currentFile = File.ReadAllBytes(files[0]);
                currentFileName.Content = System.IO.Path.GetFileName(files[0]);
            }
        }

        private void btnHash_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btnArduino_Click(object sender, RoutedEventArgs e)
        {
            if (!currentPort.IsOpen) return;

            currentPort.Write(currentFile, 0, currentFile.Length);
        }
    }
}
