// Licensed to the.NET Foundation under one or more agreements.
// The.NET Foundation licenses this file to you under the MIT license.

using System.Collections.Generic;
using UnityEngine.Assertions;
using UnityEngine.SceneManagement;
using VContainer.Unity;

namespace GameKit.SceneLauncher.VContainer
{
    public class SceneInstallerResolver
    {
        private readonly Dictionary<string, IInstaller> _nameInstallers = new();
        private readonly Dictionary<int, IInstaller> _buildIndexInstallers = new();

        public SceneInstallerResolver(IInstaller main)
        {
            RegisterBuiltIn(0, main);
        }

        private SceneInstallerResolver()
        {
            throw new System.InvalidOperationException("Default constructor is not allowed.");
        }

        public IInstaller Resolve(Scene scene)
        {
            if (_buildIndexInstallers.TryGetValue(scene.buildIndex, out var installer))
            {
                return installer;
            }

            if (_nameInstallers.TryGetValue(scene.name, out installer))
            {
                return installer;
            }

            return UnitInstaller.Instance;
        }

        public void RegisterBuiltIn(int buildIndex, IInstaller installer)
        {
            Assert.IsTrue(buildIndex >= 0);
            Assert.IsNotNull(installer);
            Assert.IsFalse(_buildIndexInstallers.ContainsKey(buildIndex),
                $"The installer for the scene with build index '{buildIndex}' is already registered.");
            _buildIndexInstallers[buildIndex] = installer;
        }

        public void RegisterName(string name, IInstaller installer)
        {
            Assert.IsFalse(string.IsNullOrWhiteSpace(name));
            Assert.IsNotNull(installer);
            Assert.IsFalse(_nameInstallers.ContainsKey(name),
                $"The installer for the scene '{name}' is already registered.");
            _nameInstallers[name] = installer;
        }
    }
}