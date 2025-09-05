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

        [UnityTest]
        public IEnumerator ReviewApproveOrReject() => UniTask.ToCoroutine(async () =>
        {
            var (session, ct) = ResolveReview<DialogSimulator, SampleReviewForm>(
                nameof(DialogSimulator),
                nameof(ReviewApproveOrReject)
            );
            var initialForm = new SampleReviewForm
            {
                Name = "Initial",
                Progress = 0,
                Count = 0
            };
            var (approved, state) = await session.ExecuteAsync(initialForm, static (dialog, review) =>
            {
                review.ApproveOrReject(dialog.OnYesOrNo);
                dialog.Yes();
            }, ct);
            Assert.That(approved, Is.True);
            Assert.That(state, Is.EqualTo(initialForm));

            (approved, state) = await session.ExecuteAsync(initialForm, static (dialog, review) =>
            {
                review.ApproveOrReject(dialog.OnYesOrNo);
                dialog.No();
            }, ct);
            Assert.That(approved, Is.False);
            Assert.That(state, Is.EqualTo(initialForm));
        });

        [UnityTest]
        public IEnumerator ReviewModifyAndApprove() => UniTask.ToCoroutine(async () =>
        {
            var (session, ct) = ResolveReview<DialogSimulator, SampleReviewForm>(
                nameof(DialogSimulator),
                nameof(ReviewModifyAndApprove)
            );
            var initialForm = new SampleReviewForm
            {
                Name = "Initial",
                Progress = 0,
                Count = 0
            };
            var (approved, state) = await session.ExecuteAsync(initialForm, static (dialog, review) =>
            {
                review.ApproveOrReject(dialog.OnYesOrNo);
                review.Modify(dialog.OnClickAddCount, form => form with { Count = form.Count + 1 });
                dialog.AddCount();
                dialog.Yes();
            }, ct);
            Assert.That(approved, Is.True);
            Assert.That(state, Is.EqualTo(initialForm with { Count = 1 }));

            (approved, state) = await session.ExecuteAsync(initialForm, static (dialog, review) =>
            {
                review.ApproveOrReject(dialog.OnYesOrNo);
                review.Modify(dialog.OnClickRemoveCount, form => form with { Count = form.Count - 1 });
                dialog.RemoveCount();
                dialog.Yes();
            }, ct);
            Assert.That(approved, Is.True);
            Assert.That(state, Is.EqualTo(initialForm with { Count = -1 }));
        });

        [UnityTest]
        public IEnumerator ReviewModifyAndReject() => UniTask.ToCoroutine(async () =>
        {
            var (session, ct) = ResolveReview<DialogSimulator, SampleReviewForm>(
                nameof(DialogSimulator),
                nameof(ReviewModifyAndReject)
            );
            var initialForm = new SampleReviewForm
            {
                Name = "Initial",
                Progress = 0,
                Count = 0
            };

            var (approved, state) = await session.ExecuteAsync(initialForm, static (dialog, review) =>
            {
                review.ApproveOrReject(dialog.OnYesOrNo);
                review.Modify(dialog.OnClickRemoveCount, form => form with { Count = form.Count - 1 });
                dialog.RemoveCount();
                dialog.No();
            }, ct);
            Assert.That(approved, Is.False);
            Assert.That(state, Is.EqualTo(initialForm));
        });
    }
}