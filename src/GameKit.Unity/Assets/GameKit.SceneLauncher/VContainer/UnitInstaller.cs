// Licensed to the.NET Foundation under one or more agreements.
// The.NET Foundation licenses this file to you under the MIT license.

using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace GameKit.SceneLauncher.VContainer
{
    public class UnitInstaller : IInstaller
    {
        public static readonly UnitInstaller Instance = new();

        private UnitInstaller()
        {
        }

        public void Install(IContainerBuilder builder)
        {
            Debug.LogError("UnitInstaller is used. This should not happen.");
        }
    }
}