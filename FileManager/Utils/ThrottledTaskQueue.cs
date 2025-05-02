using System;
using System.Threading.Tasks;

namespace FileManager.Utils;

public class ThrottledTaskQueue
{
    private Task? MainTask { get; set; }
    private bool HasContinuationQueued { get; set; }
    private object LockObject { get; } = new();

    public void QueueWork(Func<Task> work)
    {
        lock (LockObject)
        {
            if (MainTask == null || MainTask.IsCompleted)
            {
                // Run immediately
                MainTask = work();
                HasContinuationQueued = false;
            }
            else if (!HasContinuationQueued)
            {
                // Queue continuation only once
                HasContinuationQueued = true;
                MainTask = MainTask.ContinueWith(_ => work()).Unwrap().ContinueWith(_ =>
                {
                    lock (LockObject)
                    {
                        HasContinuationQueued = false;
                    }
                });
            }
        }
    }
}