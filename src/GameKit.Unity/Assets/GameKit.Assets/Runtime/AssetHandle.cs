using System;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace GameKit.Assets.Runtime
{
    public class AssetHandle
    {
        private readonly AsyncOperationHandle _handle;

        private AssetHandle()
        {
            throw new System.NotSupportedException(
                "AssetHandle cannot be instantiated directly. Use AssetHandle.Create() instead.");
        }

        public AssetHandle(AsyncOperationHandle handle)
        {
            _handle = handle;
        }
    }

    public class AssetHandle<T> : AssetHandle
    {
        private readonly AsyncOperationHandle<T> _handle;

        private AssetHandle(AsyncOperationHandle handle) : base(handle)
        {
            throw new System.NotSupportedException(
                "AssetHandle<T> cannot be instantiated directly. Use AssetHandle<T>.Create() instead.");
        }

        public AssetHandle(AsyncOperationHandle<T> handle) : base(handle)
        {
            _handle = handle;
        }
    }
}