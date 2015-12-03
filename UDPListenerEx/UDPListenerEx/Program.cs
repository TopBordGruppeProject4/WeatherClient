using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Remoting.Channels;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UDPListenerEx.TempSoundService;
using UDPListenerEx.WeatherService;

namespace UDPListenerEx
{
    class Program
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
                
                string checkWeather = "Error";
                var errorOccurred = false;
                bool done = false;
                TempSoundService.Service1Client service = new Service1Client();
                UdpClient listener = new UdpClient(listenPort);
                IPEndPoint groupEP = new IPEndPoint(IPAddress.Any, listenPort);
                WeatherService.GlobalWeatherSoapClient weather = new GlobalWeatherSoapClient();
                try
                {
                    while (!done)
                    {
                        
                        Console.WriteLine("Waiting for broadcast");
                        byte[] bytes = listener.Receive(ref groupEP);

                        Console.WriteLine("Received broadcast from " +
                            groupEP.ToString() + ": " +
                            Encoding.ASCII.GetString(bytes, 0, bytes.Length));
                        string message = Encoding.ASCII.GetString(bytes, 0, bytes.Length);
                        string[] response = message.Split(':');
                        double temp = Convert.ToDouble(response[6].Trim().Substring(0, 3));
                        Console.WriteLine(temp);
                        string time = DateTime.Now.ToString("HH:mm:ss dd/MM/yyyy");
                        Console.WriteLine(time);
                        Random rand = new Random();
                        double sound = rand.Next(40, 60);
                        Console.WriteLine(sound);
                       
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
                        

                        string rows =service.Datatransfer(time, temp, sound, checkWeather);
                        Console.WriteLine(rows);
                        
                        Thread.Sleep(10000);

                    }

                }
                catch (Exception e)
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
        public static void Main()
        {
            
            UDPListener.StartListener(7000);
            
          
            
            
            

        }
        

        
    }
}
