using System;
using System.Reactive.Concurrency;
using System.Reactive.Linq;

namespace Toggl.Foundation.Extensions
{
    public static class RxExtensions
    {
        public static IObservable<T> DelayedConditionalRetry<T>(
            this IObservable<T> source,
            int maxRetries,
            Func<int, TimeSpan> backOffStrategy,
            Func<Exception, bool> shouldRetry,
            IScheduler scheduler)
        {
            var currentAttempt = 0;

            var deferedSource = Observable.Defer(() 
                => source.DelaySubscription(backOffStrategy(currentAttempt++), scheduler) 
            );

            var catcher = deferedSource.Catch<T, Exception>(
                ex => shouldRetry(ex) && currentAttempt < maxRetries
                    ? deferedSource
                    : Observable.Throw<T>(ex)
            );

            return catcher;
        }
    }
}