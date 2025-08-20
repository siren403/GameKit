using System;
using UnityEngine.SceneManagement;

namespace GameKit.SceneLauncher.VContainer
{
    public abstract class SceneDefinition : IEquatable<SceneDefinition>
    {
        public bool Equals(SceneDefinition? other)
        {
            return other != null && GetHashCode() == other.GetHashCode();
        }
    }

    public class ScenePathDefinition : SceneDefinition
    {
        private readonly string _path;

        public ScenePathDefinition(string path)
        {
            _path = path;
        }

        public override int GetHashCode()
        {
            return _path.GetHashCode();
        }

        public override string ToString()
        {
            return _path;
        }
    }

    public class RuntimeSceneDefinition : SceneDefinition
    {
        private readonly string _path;

        public RuntimeSceneDefinition(Scene scene)
        {
            _path = scene.path;
        }

        public override int GetHashCode()
        {
            return _path.GetHashCode();
        }

        public override string ToString()
        {
            return _path;
        }
    }
}
