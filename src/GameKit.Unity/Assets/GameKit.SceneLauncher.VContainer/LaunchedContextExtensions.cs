// Licensed to the.NET Foundation under one or more agreements.
// The.NET Foundation licenses this file to you under the MIT license.

using System;
using VContainer.Unity;

namespace GameKit.SceneLauncher.VContainer
{
    public static class LaunchedContextExtensions
    {
        public static LifetimeScope GetScope(this LaunchedContext context)
        {
            if (context is null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            if (context.Options.Extensions.TryGetValue(LaunchOptionsExtensions.VContainerKey, out object? scope))
            {
                return (scope as LifetimeScope)!;
            }

            throw new InvalidOperationException("No LifetimeScope found in the LaunchedContext.");
        }
    }
}
