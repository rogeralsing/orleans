using System;
using System.Threading.Tasks;

namespace Orleans.TestingHost.Utils
{
    /// <summary>
    /// This class is for internal testing use only.
    /// </summary>
    public class AsyncResultHandle : MarshalByRefObject
    {
        bool done = false;
        bool continueFlag = false;

        public virtual void Reset()
        {
            Exception = null;
            Result = null;
            done = false;
            continueFlag = false;
        }

        public bool Done
        {
            get { return done; }
            set { done = value; }
        }

        public bool Continue
        {
            get { return continueFlag; }
            set { continueFlag = value; }
        }

        public Exception Exception { get; set; }

        public object Result { get; set; }

        /// <summary>
        /// </summary>
        /// <param name="timeout"></param>
        /// <returns>Returns <c>true</c> if operation completes before timeout</returns>
        public Task<bool> WaitForFinished(TimeSpan timeout)
        {
            return WaitFor(timeout, () => done);
        }

        /// <summary>
        /// </summary>
        /// <param name="timeout"></param>
        /// <returns>Returns <c>true</c> if operation completes before timeout</returns>
        public Task<bool> WaitForContinue(TimeSpan timeout)
        {
            return WaitFor(timeout, () => continueFlag);
        }

        /// <summary>
        /// </summary>
        /// <param name="timeout"></param>
        /// <param name="checkFlag"></param>
        /// <returns>Returns <c>true</c> if operation completes before timeout</returns>
        public async Task<bool> WaitFor(TimeSpan timeout, Func<bool> checkFlag)
        {
            double remaining = timeout.TotalMilliseconds;
            while (!checkFlag())
            {
                if (remaining < 0)
                {
                    //throw new TimeoutException("Timeout waiting for result for " + timeout);
                    return false;
                }

                await Task.Delay(TimeSpan.FromMilliseconds(200));
                remaining -= 200;
            }

            return true;
        }
    }
}
