using System;
using Cysharp.Threading.Tasks.Triggers;
using R3;

namespace GameKit.Navigation.Screens.Sessions.Dialog
{
    public struct DecisionBinder<TState> : IDisposable
    {
        private readonly TState _initialState;
        private readonly ReactiveProperty<TState> _property;
        private readonly ISubject<(bool approved, TState state)> _stream;
        private DisposableBag _disposable;

        public DecisionBinder(TState initialState,
            ReactiveProperty<TState> property,
            ISubject<(bool approved, TState state)> stream
        )
        {
            _initialState = initialState;
            _property = property;
            _stream = stream;
            _disposable = new DisposableBag();
        }

        public void Approve(Observable<Unit> source)
        {
            source.Subscribe((_property, _stream), static (_, state) =>
            {
                var (property, stream) = state;
                stream.OnNext((true, property.Value));
            }).AddTo(ref _disposable);
        }

        public void Reject(Observable<Unit> source)
        {
            source.Subscribe((_initialState, _stream), static (_, state) =>
            {
                var (initial, stream) = state;
                stream.OnNext((false, initial));
            }).AddTo(ref _disposable);
        }

        public void ApproveOrReject(Observable<bool> source)
        {
            source.Subscribe((_initialState, _property, _stream), static (approved, state) =>
            {
                var (initial, property, stream) = state;
                stream.OnNext((approved, approved ? property.Value : initial));
            }).AddTo(ref _disposable);
        }

        public void Modify<T>(Observable<T> source, Func<T, TState, TState> modifier)
        {
            source.Subscribe((_property, modifier), static (value, state) =>
            {
                var (property, modify) = state;
                property.Value = modify(value, property.Value);
            }).AddTo(ref _disposable);
        }

        public void Modify(Observable<Unit> source, Func<TState, TState> modifier)
        {
            source.Subscribe((_property, modifier), static (_, state) =>
            {
                var (property, modify) = state;
                property.Value = modify(property.Value);
            }).AddTo(ref _disposable);
        }

        public void Dispose()
        {
            _disposable.Dispose();
        }
    }
}