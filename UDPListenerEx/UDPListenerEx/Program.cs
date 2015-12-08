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
        
        public static void Main()
        {
            SetUpTracing();
            UDPListener.StartListener(7000);

        }

        private static void SetUpTracing()
        {
            Trace.Listeners.Add(new ConsoleTraceListener());
            FileStream fs = new FileStream("c:/temp/udplistenerlog.txt", FileMode.Append);
            TextWriterTraceListener textWriterTraceListener = new TextWriterTraceListener(fs);

            Trace.Listeners.Add(textWriterTraceListener);

            ShowTraceListeners();
        }

        private static void ShowTraceListeners()
        {
            TraceListenerCollection listeners = Trace.Listeners;
            foreach (object listener in listeners)
            {
                Console.WriteLine(listener);
            }
        }

    }
}
