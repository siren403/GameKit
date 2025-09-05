using System;
using R3;

namespace GameKit.Navigation.Screens.Sessions.Dialog
{
    public struct AnswerBinder<T> : IDisposable
    {
        private readonly ISubject<T> _stream;
        private DisposableBag _disposable;

        public AnswerBinder(ISubject<T> stream)
        {
            _stream = stream;
            _disposable = new DisposableBag();
        }

        public void Bind(Observable<Unit> source, T value)
        {
            source.Subscribe((value, _stream), static (_, state) =>
            {
                var (value, stream) = state;
                stream.OnNext(value);
            }).AddTo(ref _disposable);
        }

        public void Bind(Observable<T> source)
        {
            source.Subscribe(_stream,
                static (value, stream) => stream.OnNext(value)
            ).AddTo(ref _disposable);
        }

        public void Dispose()
        {
            _disposable.Dispose();
        }
    }
}