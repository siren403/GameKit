using GameKit.SceneLauncher.Internal;
using NUnit.Framework;

namespace GameKit.SceneLauncher.Tests
{
    [TestFixture]
    public class InternalTest
    {
        [Test]
        public void MatchAliases()
        {
            var libPath = "Assets/Lib";
            var parser = new ScenePathParser
            {
                Aliases =
                {
                    ["$lib"] = libPath
                }
            };

            // Success
            {
                var scenePath = "Scenes/Scene.unity";
                switch (parser.Parse($"$lib/{scenePath}"))
                {
                    case ParseSuccess(var value):
                        Assert.AreEqual($"{libPath}/{scenePath}", value);
                        break;
                    case ParseFailure(var error):
                        Assert.Fail(error.ToString());
                        break;
                    default:
                        Assert.Fail();
                        break;
                }
            }

            // Failure
            {
                switch (parser.Parse("----failure----/Scenes/Scene.unity"))
                {
                    case ParseSuccess(var value):
                        Assert.Fail(value);
                        break;
                    case ParseFailure(var error):
                        Assert.AreEqual(ParseError.NotStartedAlias, error);
                        break;
                    default:
                        Assert.Fail();
                        break;
                }

                switch (parser.Parse("$----failure----"))
                {
                    case ParseSuccess(var value):
                        Assert.Fail(value);
                        break;
                    case ParseFailure(var error):
                        Assert.AreEqual(ParseError.NotIncludeAliasSymbol, error);
                        break;
                    default:
                        Assert.Fail();
                        break;
                }

                switch (parser.Parse("$----failure----/Scenes/Scene.unity"))
                {
                    case ParseSuccess(var value):
                        Assert.Fail(value);
                        break;
                    case ParseFailure(var error):
                        Assert.AreEqual(ParseError.NotFoundAlias, error);
                        break;
                    default:
                        Assert.Fail();
                        break;
                }
            }
        }
    }


}
