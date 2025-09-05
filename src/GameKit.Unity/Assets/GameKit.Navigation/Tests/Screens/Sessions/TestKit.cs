using System.Threading;
using GameKit.Navigation.Screens.Dialog;
using GameKit.Navigation.Screens.Sessions.Dialog;
using GameKit.Navigation.Screens.Sessions.Dialog.Internal;
using GameKit.Navigation.VContainer;
using UnityEngine;
using VContainer;

namespace GameKit.Navigation.Tests.Screens.Sessions
{
    public static class TestKit
    {
        public static IObjectResolver BuildDialogSimulator()
        {
            var builder = new ContainerBuilder();
            builder.Register<DialogSimulator>(Lifetime.Singleton);
            return builder.Build();
        }

        public static (IQuestionSession<TDialog, TResult> session, CancellationToken ct)
            ResolveQuestion<TDialog, TResult>(
                string id, string? contextName = null
            ) where TDialog : IDialog
        {
            var builder = new ContainerBuilder();
            builder.RegisterDialogs(dialogs =>
            {
                dialogs.InMemory<TDialog>(id); //
            }, contextName);
            builder.Register<QuestionSession<TDialog, TResult>>(Lifetime.Scoped)
                .As<IQuestionSession<TDialog, TResult>>()
                .WithParameter(id);

            var resolver = builder.Build();
            var session = resolver.Resolve<IQuestionSession<TDialog, TResult>>();
            return (session, Application.exitCancellationToken);
        }

        public static (IReviewSession<TDialog, TState> session, CancellationToken ct)
            ResolveReview<TDialog, TState>(
                string id, string? contextName = null
            ) where TDialog : IDialog
        {
            var builder = new ContainerBuilder();
            builder.RegisterDialogs(dialogs =>
            {
                dialogs.InMemory<TDialog>(id); //
            }, contextName);
            builder.Register<ReviewSession<TDialog, TState>>(Lifetime.Scoped)
                .As<IReviewSession<TDialog, TState>>()
                .WithParameter(id);

            var resolver = builder.Build();
            var session = resolver.Resolve<IReviewSession<TDialog, TState>>();
            return (session, Application.exitCancellationToken);
        }
    }
}