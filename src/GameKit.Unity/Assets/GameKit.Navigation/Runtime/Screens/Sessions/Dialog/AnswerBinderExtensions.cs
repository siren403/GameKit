using R3;

namespace GameKit.Navigation.Screens.Sessions.Dialog
{
    public static class AnswerBinderExtensions
    {
        public static void Yes(this ref AnswerBinder<bool> binder, Observable<Unit> source)
        {
            binder.Bind(source, true);
        }

        public static void No(this ref AnswerBinder<bool> binder, Observable<Unit> source)
        {
            binder.Bind(source, false);
        }

        public static void No(this ref AnswerBinder<bool> binder,
            Observable<Unit> source1,
            Observable<Unit> source2
        )
        {
            binder.Bind(source1, false);
            binder.Bind(source2, false);
        }

        public static void Pass<T>(this ref AnswerBinder<T> binder, Observable<T> source)
        {
            binder.Bind(source);
        }
    }
}