using System;
using GameKit.Navigation.Screens.Dialog;
using GameKit.Navigation.Screens.Sessions.Dialog;
using GameKit.Navigation.Screens.Sessions.Dialog.Internal;
using VContainer;

namespace GameKit.Navigation.VContainer
{
    public class DialogBuilder : ScreenBuilder<IDialog>
    {
        public DialogBuilder(IContainerBuilder builder) : base(builder)
        {
        }

        public void QuestionInHierarchy<TDialog, TResult>(string id) where TDialog : IDialog
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentException("Dialog ID cannot be null or empty.", nameof(id));
            }

            InHierarchy<TDialog>(id);
            Builder.Register<QuestionSession<TDialog, TResult>>(Lifetime.Scoped)
                .As<IQuestionSession<TDialog, TResult>>()
                .WithParameter(id);

            Builder.RegisterBuildCallback(static container =>
            {
                _ = container.Resolve<IQuestionSession<TDialog, TResult>>();
            });
        }
    }
}