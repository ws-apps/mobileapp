using System;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using Toggl.Ultrawave.Exceptions;

namespace Toggl.Foundation.Extensions
{
    public static class RxExtensions
    {
        public static IObservable<T> DelayedConditionalRetry<T>(
            this IObservable<T> source,
            int maxRetries,
            Func<int, TimeSpan> backOffStrategy,
            Func<Exception, bool> shouldRetryOn,
            IScheduler scheduler)
        {
            var currentAttempt = 0;

            return Observable.Defer(() =>
                {
                    var timeSpan = currentAttempt == 0 ? TimeSpan.Zero : backOffStrategy(currentAttempt);
                    currentAttempt++;
                    return source.DelaySubscription(timeSpan, scheduler);
                })
                .Select<T, (bool succeeded, T t, Exception e)>(result => (true, result, null))
                .Catch<(bool succeeded, T t, Exception e), Exception>(exception => 
                    shouldRetryOn(exception)
                    ? Observable.Throw<(bool succeeded, T t, Exception e)>(exception)
                    : Observable.Return((succeeded: false, t: default(T), e: exception))
                ).Retry(maxRetries + 1)
                .SelectMany(result =>
                    result.succeeded ? Observable.Return(result.t) : Observable.Throw<T>(result.e)
                );
        }
    }
}