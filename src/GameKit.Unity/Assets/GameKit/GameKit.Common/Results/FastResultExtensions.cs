// Licensed to the.NET Foundation under one or more agreements.
// The.NET Foundation licenses this file to you under the MIT license.

using System.Runtime.CompilerServices;
using UnityEngine;

namespace GameKit.Common.Results
{
    public static class FastResultExtensions
    {
        public static bool IsError<T, TCast>(this FastResult<T> result, out FastResult<TCast> fail)
        {
            if (result.IsError)
            {
                fail = result.Cast<TCast>();
                return true;
            }

            fail = default;
            return false;
        }

        // public static bool IsError<TCast>(this FastResult<TCast> result, FastResult<TCast> append,
        //     out FastResult<TCast> fail)
        // {
        //     if (result.IsError)
        //     {
        //         fail = result.Cast(append);
        //         return true;
        //     }
        //
        //     fail = default;
        //     return false;
        // }
    }
}
