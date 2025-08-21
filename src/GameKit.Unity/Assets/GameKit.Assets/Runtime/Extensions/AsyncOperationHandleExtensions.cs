// Licensed to the.NET Foundation under one or more agreements.
// The.NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using GameKit.Common.Results;
using UnityEngine;
using UnityEngine.ResourceManagement;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace GameKit.Assets.Extensions
{
    public static class AsyncOperationHandleExtensions
    {
        private static readonly Dictionary<AsyncOperationHandle, Exception> _handleExceptions = new();

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void RuntimeInitializeOnLoad()
        {
            ResourceManager.ExceptionHandler -= OnException;
            ResourceManager.ExceptionHandler += OnException;

            return;

            void OnException(AsyncOperationHandle handle, Exception exception)
            {
                if (!_handleExceptions.ContainsKey(handle)) return;

                _handleExceptions[handle] = exception;
#if UNITY_EDITOR
                Debug.LogError($"{handle.DebugName}, Exception: {exception.Message}");
#endif
            }
        }

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

        /// <summary>
        /// 핸들이 autoReleaseHandle로 생성되었을 경우,
        /// 완료시 바로 해제되기때문에 Status등의 프로퍼티 접근이 불가능하다.
        /// 그해서 완료 후 디테일한 조건 검사는 회피하고 ResourceManager의 예외 발생으로만 실패 처리한다.
        /// </summary>
        /// <param name="handle"></param>
        /// <param name="progress"></param>
        /// <returns></returns>
        public static async UniTask<FastResult<AsyncOperationHandle>> OrError(
            this AsyncOperationHandle handle,
            IProgress<DownloadStatus> progress = null
        )
        {
            _handleExceptions[handle] = null;
            if (progress is null)
            {
                await handle;
            }
            else
            {
                var initialStatus = handle.GetDownloadStatus();
                if (initialStatus is { TotalBytes: > 0 })
                {
                    progress.Report(initialStatus);
                }

                DownloadStatus previousStatus = initialStatus;
                while (!handle.IsDone && _handleExceptions.GetValueOrDefault(handle, null) is null)
                {
                    await UniTask.Yield();
                    if (handle.IsValid() && handle.IsDone == false)
                    {
                        var status = handle.GetDownloadStatus();
                        var previousBytes = previousStatus.DownloadedBytes;
                        var currentBytes = status.DownloadedBytes;
                        if (status is { TotalBytes: > 0 } && currentBytes > previousBytes)
                        {
                            progress.Report(status);
                        }
                    }
                }
            }

            _handleExceptions.Remove(handle, out var exception);
            if (exception != null)
            {
                return FastResult<AsyncOperationHandle>.Fail(new Error
                {
                    Code = ErrorCodes.ResourceManagerError,
                    Description = exception.Message
                });
            }

            return FastResult<AsyncOperationHandle>.Ok(handle);
        }

        public static async UniTask<FastResult<T>> OrError<T>(
            this AsyncOperationHandle<T> handle
        )
        {
            _handleExceptions[handle] = null;
            var value = await handle.Task.AsUniTask();
            _handleExceptions.Remove(handle, out var exception);

            if (exception != null)
            {
                return FastResult<T>.Fail(new Error
                {
                    Code = ErrorCodes.ResourceManagerError,
                    Description = exception.Message
                });
            }

            return FastResult<T>.Ok(value);
        }
    }
}