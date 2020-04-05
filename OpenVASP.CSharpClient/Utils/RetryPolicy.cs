using System;
using System.Threading.Tasks;

namespace OpenVASP.CSharpClient
{
    public static class RetryPolicy
    {
        public static async Task<T> ExecuteAsync<T>(Func<Task<T>> func, int retriesCount = 4)
        {
            var counter = 0;
            Exception lastException = null;

            do
            {
                try
                {
                    return await func();
                }
                catch (Exception e)
                {
                    lastException = e;
                }

                var delay = 100 * (int)Math.Pow(2, counter);
                await Task.Delay(delay);
                counter++;
            } while (counter < retriesCount);

            throw lastException;
        }
    }
}