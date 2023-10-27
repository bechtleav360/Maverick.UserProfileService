using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace UserProfileService.Common.Tests.Utilities.Utilities
{
    public class AsyncLazy<T> : Lazy<Task<T>>
    {
        public AsyncLazy(Func<T> valueFactory)
            : base(() => Task.Factory.StartNew(valueFactory), true)
        {
        }

        public AsyncLazy(Func<Task<T>> taskFactory)
            : base(() => Task.Factory.StartNew(taskFactory).Unwrap(), true)
        {
        }

        public TaskAwaiter<T> GetAwaiter()
        {
            return Value.GetAwaiter();
        }
    }

    public class AsyncLazy : Lazy<Task>
    {
        public AsyncLazy(Func<Task> taskFactory)
            : base(() => Task.Factory.StartNew(taskFactory).Unwrap(), true)
        {
        }

        public TaskAwaiter GetAwaiter()
        {
            return Value.GetAwaiter();
        }
    }
}
