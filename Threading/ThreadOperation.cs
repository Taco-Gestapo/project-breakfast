using System;
using System.Threading;

namespace NightLight.Threading
{
    class ThreadOperation
    {
        /// <summary>
        /// Holds the refferances to all the threads thats used in the program
        /// </summary>
        private Thread[] threads;

        /// <summary>
        /// Initializes the ThreadOperation class
        /// </summary>
        /// <param name="threadAmount">Amount of threads that shall be used</param>
        public ThreadOperation(uint threadAmount)
        {
            this.threads = new Thread[threadAmount];
        }

        /// <summary>
        /// Prepares all the threads for use
        /// </summary>
        /// <param name="startFunction">Points to the function that shall be executed</param>
        internal void Initialize(ThreadStart startFunction)
        {
            for (int i = 0; i < threads.Length; i++)
            {
                threads[i] = new Thread(startFunction);
            }
        }

        /// <summary>
        /// Starts all the threads
        /// </summary>
        internal void StartThreads()
        {
            for (int i = 0; i < threads.Length; i++)
            {
                threads[i].Start();
            }
        }

        /// <summary>
        /// Stops all the threads
        /// </summary>
        internal void StopThreads()
        {
            try
            {
                for (int i = 0; i < threads.Length; i++)
                {
                    threads[i].Abort();
                }
            }
            catch { }
        }
    }
}
