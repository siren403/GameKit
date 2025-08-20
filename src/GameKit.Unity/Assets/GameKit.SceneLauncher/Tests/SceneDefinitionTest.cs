using GameKit.SceneLauncher.VContainer;
using NUnit.Framework;

namespace GameKit.SceneLauncher.Tests
{
    [TestFixture]
    public class SceneDefinitionTest
    {
        [Test]
        public void Equals()
        {
            var a = new ScenePathDefinition("Assets/");
            var b = new ScenePathDefinition("Assets/");

            Assert.True(a.Equals(b));
        }
    }
}
