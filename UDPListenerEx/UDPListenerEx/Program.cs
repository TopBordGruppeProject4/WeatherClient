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
        
        public static class UDPListener
        {
            

            public static void StartListener(int ListenPort)
            {
                bool done = false;
                TempSoundService.Service1Client service = new Service1Client();
                UdpClient listener = new UdpClient(ListenPort);
                IPEndPoint groupEP = new IPEndPoint(IPAddress.Any, ListenPort);
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
                       
                        string TodaysWeather = weather.GetWeather("Roskilde", "Denmark");
                        string[] Weatherlist = TodaysWeather.Split('>');
                        string[] Weatherlist2 = Weatherlist[9].Split('<');
                        string[] Weatherlist3 = Weatherlist2[0].Split(' ');
                        string[] Weatherlist4 = Weatherlist3[3].Split('(');
                        Console.WriteLine(Weatherlist4[1]);

                        string rows =service.Datatransfer(time, temp, sound, Weatherlist4[1]);
                        Console.WriteLine(rows);
                        
                        Thread.Sleep(10000);

                    }

                }
                catch (Exception e)
                {
                    Trace.WriteLine(e.ToString());
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
