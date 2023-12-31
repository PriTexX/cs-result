﻿using System.Runtime.CompilerServices;

namespace PResult;

public readonly struct AsyncResult<TValue>
{
    private readonly Task<Result<TValue>> _asyncResult;

    private AsyncResult(Task<Result<TValue>> asyncResult)
    {
        _asyncResult = asyncResult;
    }

    public Task<Result<TValue>> AsTask => _asyncResult;

    public static implicit operator AsyncResult<TValue>(Task<Result<TValue>> asyncResult)
    {
        return new AsyncResult<TValue>(asyncResult);
    }

    public Task<TRes> Match<TRes>(Func<TValue, TRes> success, Func<Exception, TRes> fail) =>
        _asyncResult.ContinueWith(finishedTask =>
        {
            var res = finishedTask.Result;
            return res.Match(success, fail);
        });

    public Task<TRes> MatchAsync<TRes>(
        Func<TValue, Task<TRes>> success,
        Func<Exception, Task<TRes>> fail
    ) =>
        _asyncResult
            .ContinueWith(finishedTask =>
            {
                var res = finishedTask.Result;
                return res.MatchAsync(success, fail);
            })
            .Unwrap();

    public Task<TRes> MatchAsync<TRes>(
        Func<TValue, Task<TRes>> success,
        Func<Exception, TRes> fail
    ) =>
        _asyncResult
            .ContinueWith(finishedTask =>
            {
                var res = finishedTask.Result;
                return res.MatchAsync(success, fail);
            })
            .Unwrap();

    public AsyncResult<K> Then<K>(Func<TValue, Result<K>> next)
    {
        return _asyncResult.ContinueWith(finishedTask =>
        {
            var res = finishedTask.Result;
            return res.Then(next);
        });
    }

    public AsyncResult<K> ThenAsync<K>(Func<TValue, Task<Result<K>>> next)
    {
        return _asyncResult
            .ContinueWith(finishedTask =>
            {
                var res = finishedTask.Result;

                return res.IsError
                    ? Task.FromResult(new Result<K>(res.UnsafeError))
                    : next(res.UnsafeValue);
            })
            .Unwrap();
    }

    public AsyncResult<TValue> MapErr(Func<Exception, Result<TValue>> errMap)
    {
        return _asyncResult.ContinueWith(finishedTask =>
        {
            var res = finishedTask.Result;
            return res.MapErr(errMap);
        });
    }

    public AsyncResult<TValue> MapErrAsync(Func<Exception, Task<Result<TValue>>> mapErrAsync)
    {
        return _asyncResult
            .ContinueWith(finishedTask =>
            {
                var res = finishedTask.Result;

                return res.IsError
                    ? mapErrAsync(res.UnsafeError)
                    : Task.FromResult(new Result<TValue>(res.UnsafeValue));
            })
            .Unwrap();
    }

    public TaskAwaiter<Result<TValue>> GetAwaiter()
    {
        return _asyncResult.GetAwaiter();
    }
}
