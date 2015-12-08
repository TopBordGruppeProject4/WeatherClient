using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UDPListenerEx.TempSoundService;
using UDPListenerEx.WeatherService;

namespace UDPListenerEx
{
    /// <summary>
    /// This class contains the method, that we use to get readings from our local censors plus the weather outside.
    /// </summary>
    public static class UDPListener
    {
        /// <summary>
        /// This method is used to get readings from our local censors plus the weather outside.
        /// </summary>
        /// <param name="listenPort"> This parameter is used to choose on which port, that you want to listen on.</param>
        public static void StartListener(int listenPort)
        {
            // Done is used to run the while loop, checkweather is the weather from our third party webservice, and errorOccurred is used to check which method needs to be run
            string checkWeather = "Error";
            var errorOccurred = false;
            bool done = false;

            Service1Client service = new Service1Client();
            UdpClient listener = new UdpClient(listenPort);
            IPEndPoint groupEP = new IPEndPoint(IPAddress.Any, listenPort);
            GlobalWeatherSoapClient weather = new GlobalWeatherSoapClient();
            try
            {
                while (!done)
                {
                    //Here we start our listener, and wait for a broadcast
                    Trace.WriteLine("Waiting for broadcast");
                    byte[] bytes = listener.Receive(ref groupEP);

                    //When we recieve the message, we then use GetString, and afterwards we split it
                    Trace.WriteLine("Received broadcast from " +
                        groupEP + ": " +
                        Encoding.ASCII.GetString(bytes, 0, bytes.Length));

                    string message = Encoding.ASCII.GetString(bytes, 0, bytes.Length);

                    string[] response = message.Split(':');

                    //In response we know where the temperature is, and we convert that to a double. The number we get is around 218, and because we need a value that looks like celcius, we divide it with 10 so we have an acceptable value
                    double temp = Convert.ToDouble(response[6].Trim().Substring(0, 3));

                    temp = temp / 10;
                    Trace.WriteLine(temp);

                    //We need to send a Date and time plus soundlevel, which we randomly generate
                    string time = DateTime.Now.ToString("HH:mm:ss dd/MM/yyyy");
                    Trace.WriteLine(time);
                    Random rand = new Random();
                    double sound = rand.Next(40, 60);
                    Trace.WriteLine(sound);

                    //Here we get the weather for Roskilde, and afterwards we try to split the string that gets returned. But because the string we get can different depending on the weather, and exception can arise, and by catching that we can split it in another way, that then works
                    string todaysWeather = weather.GetWeather("Roskilde", "Denmark");

                    try
                    {
                        string[] weatherlist = todaysWeather.Split('>');
                        string[] weatherlist2 = weatherlist[9].Split('<');
                        string[] weatherlist3 = weatherlist2[0].Split(' ');
                        string[] weatherlist4 = weatherlist3[3].Split('(');
                        checkWeather = weatherlist4[1];
                    }
                    catch (Exception)
                    {

                        errorOccurred = true;
                    }

                    if (errorOccurred)
                    {
                        string[] weatherlist5 = todaysWeather.Split('>');
                        string[] weatherlist6 = weatherlist5[11].Split('<');
                        string[] weatherlist7 = weatherlist6[0].Split(' ');
                        string[] weatherlist8 = weatherlist7[3].Split('(');
                        checkWeather = weatherlist8[1];
                    }


                    string rows = service.Datatransfer(time, temp, sound, checkWeather);
                    Trace.WriteLine(rows);

                    Trace.Flush();
                    Thread.Sleep(10000);

                }

            }
            catch (Exception)
            {
                string time = DateTime.Now.ToString("HH:mm:ss dd/MM/yyyy");
                service.Datatransfer(time, 0, 0, "Error occurred while receiving broadcast");
            }
            finally
            {
                listener.Close();
            }
        }
    }
}
