using System;
using System.Threading.Tasks;

namespace FileManager.Utils;

public class ThrottledTaskQueue
{
    private Task? MainTask { get; set; }
    private bool HasContinuationQueued { get; set; }
    private object LockObject { get; } = new();

    private Func<Task>? PendingContinuation { get; set; }

    private bool IsContinuationChained { get; set; }

    public void QueueWork(Func<Task> work)
    {
        lock (LockObject)
        {
            if (MainTask == null || MainTask.IsCompleted)
            {
                // Run immediately
                MainTask = work();
                PendingContinuation = null;
                IsContinuationChained = false;
            }
            else
            {
                // Replace existing continuation
                PendingContinuation = work;

                if (!IsContinuationChained)
                {
                    // Chain one continuation only
                    IsContinuationChained = true;

                    MainTask = MainTask.ContinueWith(_ =>
                    {
                        Func<Task>? continuation;

                        lock (LockObject)
                        {
                            continuation = PendingContinuation;
                            PendingContinuation = null;
                            IsContinuationChained = false;
                        }

                        return continuation?.Invoke() ?? Task.CompletedTask;
                    }).Unwrap();
                }
            }
        }
    }
}