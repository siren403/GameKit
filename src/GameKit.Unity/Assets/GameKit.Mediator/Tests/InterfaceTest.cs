using System.Collections;
using System.Threading;
using Cysharp.Threading.Tasks;
using GameKit.Mediator.VContainer;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using VContainer;
using VitalRouter.VContainer;

namespace GameKit.Mediator.Tests
{
    using Abstractions;

    public readonly struct GetFullName : ICommand<string>
    {
        public string FirstName { get; init; }
        public string LastName { get; init; }
    }

    public class FullNameHandler : ICommandHandler<GetFullName, string>
    {
        public UniTask<string> ExecuteAsync(GetFullName command, CancellationToken ct)
        {
            string result = $"{command.FirstName} {command.LastName}";
            return UniTask.FromResult(result);
        }
    }

    public class InterfaceTest
    {
        [UnityTest]
        public IEnumerator Sample1() => UniTask.ToCoroutine(async () =>
        {
            var ct = Application.exitCancellationToken;
            var bld = new ContainerBuilder();
            bld.RegisterVitalRouter(routing => { });
            bld.RegisterMediator(mediator => { mediator.RegisterCommand<GetFullName, FullNameHandler, string>(); });
            var container = bld.Build();
            var mediator = container.Resolve<IMediator>();
            string result = await mediator.ExecuteAsync<GetFullName, string>(
                new GetFullName() { FirstName = "John", LastName = "Doe" },
                ct);
            Assert.AreEqual("John Doe", result, "Full name should be 'John Doe'");
        });
    }
}
