using System.Collections;
using Cysharp.Threading.Tasks;
using NUnit.Framework;
using UnityEngine.TestTools;

namespace GameKit.Navigation.Tests.Screens.Sessions
{
    public class ReviewSessionTest
    {

        [UnityTest]
        public IEnumerator ReviewApproveOrReject() => UniTask.ToCoroutine(async () =>
        {
            var (session, ct) = TestKit.ResolveReview<DialogSimulator, SampleReviewForm>(
                nameof(DialogSimulator),
                nameof(ReviewApproveOrReject)
            );
            var initialForm = new SampleReviewForm
            {
                Name = "Initial",
                Progress = 0,
                Count = 0
            };
            var (approved, state) = await session.ExecuteAsync(initialForm, static (dialog, decision) =>
            {
                decision.ApproveOrReject(dialog.OnYesOrNo);
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
            var (session, ct) = TestKit.ResolveReview<DialogSimulator, SampleReviewForm>(
                nameof(DialogSimulator),
                nameof(ReviewModifyAndApprove)
            );
            var initialForm = new SampleReviewForm
            {
                Name = "Initial",
                Progress = 0,
                Count = 0
            };
            var (approved, state) = await session.ExecuteAsync(initialForm, static (dialog, decision) =>
            {
                decision.ApproveOrReject(dialog.OnYesOrNo);
                decision.Modify(dialog.OnClickAddCount, form => form with {Count = form.Count + 1});
                dialog.AddCount();
                dialog.Yes();
            }, ct);
            Assert.That(approved, Is.True);
            Assert.That(state, Is.EqualTo(initialForm with {Count = 1}));

            (approved, state) = await session.ExecuteAsync(initialForm, static (dialog, review) =>
            {
                review.ApproveOrReject(dialog.OnYesOrNo);
                review.Modify(dialog.OnClickRemoveCount, form => form with {Count = form.Count - 1});
                dialog.RemoveCount();
                dialog.Yes();
            }, ct);
            Assert.That(approved, Is.True);
            Assert.That(state, Is.EqualTo(initialForm with {Count = -1}));
        });

        [UnityTest]
        public IEnumerator ReviewModifyAndReject() => UniTask.ToCoroutine(async () =>
        {
            var (session, ct) = TestKit.ResolveReview<DialogSimulator, SampleReviewForm>(
                nameof(DialogSimulator),
                nameof(ReviewModifyAndReject)
            );
            var initialForm = new SampleReviewForm
            {
                Name = "Initial",
                Progress = 0,
                Count = 0
            };

            var (approved, state) = await session.ExecuteAsync(initialForm, static (dialog, decision) =>
            {
                decision.ApproveOrReject(dialog.OnYesOrNo);
                decision.Modify(dialog.OnClickRemoveCount, form => form with {Count = form.Count - 1});
                dialog.RemoveCount();
                dialog.No();
            }, ct);
            Assert.That(approved, Is.False);
            Assert.That(state, Is.EqualTo(initialForm));
        });
    }
}