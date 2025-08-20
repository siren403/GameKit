// Licensed to the.NET Foundation under one or more agreements.
// The.NET Foundation licenses this file to you under the MIT license.

using Cysharp.Threading.Tasks;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace GameKit.Assets.Extensions
{
    public static class AsyncOperationHandleExtensions
    {
        public static async UniTask<AsyncHandleSnapshot<T>> CaptureWithRelease<T>(
            this AsyncOperationHandle<T> handle)
        {
            await handle;
            AsyncHandleSnapshot<T> snapshot = new()
            {
                Status = handle.Status,
                Result = handle.Result,
                OperationException = handle.OperationException,
                DebugName = handle.DebugName
            };
            handle.Release();
            return snapshot;
        }
    }

}