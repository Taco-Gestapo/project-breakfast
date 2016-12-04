namespace NightLight
{
    interface ITool
    {
        /// <summary>
        /// Sets up the tool
        /// </summary>
        /// <param name="target">The targets IP address</param>
        /// <param name="port">The targets port</param>
        /// <param name="connections">Amount of connections that should be connected to the target. Can also be used as amount of packets</param>
        /// <param name="timeout">The maximum time the tool is allowed to run. When the timeout is done, the tool would stop running</param>
        void Init(string target, int port, int connections, int timeout);

        /// <summary>
        /// Starts the tool
        /// </summary>
        void Start();

        /// <summary>
        /// Gets a bool on wether the tool is timed out and should be terminated
        /// </summary>
        bool IsTimedOut { get; }

        /// <summary>
        /// Aborts the tools run-time
        /// </summary>
        void Abort();

        /// <summary>
        /// Gets the status to the console when it is cycled each 100ms from the GUI thread
        /// </summary>
        /// <returns>Returns a string that shall be written to the console of the tools state</returns>
        string GetStatus();
    }
}
