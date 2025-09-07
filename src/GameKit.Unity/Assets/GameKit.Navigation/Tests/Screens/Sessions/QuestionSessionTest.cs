using System.Collections;
using Cysharp.Threading.Tasks;
using GameKit.Navigation.Screens.Sessions.Dialog;
using NUnit.Framework;
using UnityEngine.TestTools;
using static GameKit.Navigation.Tests.Screens.Sessions.TestKit;

namespace GameKit.Navigation.Tests.Screens.Sessions
{
    public class QuestionSessionTest
    {
        [UnityTest]
        public IEnumerator YesOrNo() => UniTask.ToCoroutine(async () =>
        {
            var (session, ct) = ResolveQuestion<DialogSimulator, bool>(
                nameof(DialogSimulator),
                nameof(YesOrNo)
            );

            var yesOrNo = await session.ExecuteAsync(static (dialog, answer) =>
            {
                answer.Bind(dialog.OnClickYes, true);
                dialog.Yes();
                dialog.Yes();
            }, ct);
            Assert.True(yesOrNo);

            yesOrNo = await session.ExecuteAsync(static (dialog, answer) =>
            {
                answer.No(dialog.OnClickNo);
                dialog.No();
            }, ct);
            Assert.False(yesOrNo);
        });

        [UnityTest]
        public IEnumerator MatchedChoice() => UniTask.ToCoroutine(async () =>
        {
            var (session, ct) = ResolveQuestion<DialogSimulator, SampleChoice>(
                nameof(DialogSimulator),
                nameof(MatchedChoice)
            );
            var choice = await session.ExecuteAsync(static (dialog, answer) =>
            {
                answer.Bind(dialog.OnClickMaybe, SampleChoice.Maybe);
                dialog.Maybe();
            }, ct);
            Assert.That(choice, Is.EqualTo(SampleChoice.Maybe));

            choice = await session.ExecuteAsync(static (dialog, answer) =>
            {
                answer.Bind(dialog.OnClickYes, SampleChoice.Yes);
                dialog.Yes();
            }, ct);
            Assert.That(choice, Is.EqualTo(SampleChoice.Yes));

            choice = await session.ExecuteAsync(static (dialog, answer) =>
            {
                answer.Bind(dialog.OnClickNo, SampleChoice.No);
                dialog.No();
            }, ct);
            Assert.That(choice, Is.EqualTo(SampleChoice.No));

            choice = await session.ExecuteAsync(static (dialog, answer) =>
            {
                answer.Bind(dialog.OnClickScrim, SampleChoice.No);
                dialog.Scrim();
            }, ct);
            Assert.That(choice, Is.EqualTo(SampleChoice.No));
        });

        [UnityTest]
        public IEnumerator PassedChoice() => UniTask.ToCoroutine(async () =>
        {
            var (session, ct) = ResolveQuestion<DialogSimulator, SampleChoice>(
                nameof(DialogSimulator),
                nameof(PassedChoice)
            );
            var choice = await session.ExecuteAsync(static (dialog, answer) =>
            {
                answer.Bind(dialog.OnChoice);
                dialog.Maybe();
            }, ct);
            Assert.That(choice, Is.EqualTo(SampleChoice.Maybe));

            choice = await session.ExecuteAsync(static (dialog, answer) =>
            {
                answer.Bind(dialog.OnChoice);
                dialog.Yes();
            }, ct);
            Assert.That(choice, Is.EqualTo(SampleChoice.Yes));

            choice = await session.ExecuteAsync(static (dialog, answer) =>
            {
                answer.Bind(dialog.OnChoice);
                dialog.No();
            }, ct);
            Assert.That(choice, Is.EqualTo(SampleChoice.No));
        });


    }

}