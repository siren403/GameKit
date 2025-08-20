using System;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace GameKit.Assets
{
    public readonly struct AsyncHandleSnapshot<T>
    {
        public AsyncOperationStatus Status { get; init; }
        public T Result { get; init; }
        public Exception OperationException { get; init; }
        public string DebugName { get; init; }

        public bool IsSuccess => Status == AsyncOperationStatus.Succeeded;

        public void Deconstruct(out AsyncOperationStatus status, out T result, out Exception operationException,
            out string debugName)
        {
            status = Status;
            result = Result;
            operationException = OperationException;
            debugName = DebugName;
        }
    }
}