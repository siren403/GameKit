using System;
using GameKit.Navigation.Screens.Dialog;
using R3;
using VitalRouter;

namespace GameKit.Navigation.Tests.Screens.Sessions
{
    public class DialogSimulator : IDialog, IDisposable
    {
        public bool IsVisible { get; set; }

        private readonly Subject<Unit> _yes = new();
        private readonly Subject<Unit> _no = new();

        public Observable<Unit> OnClickYes => _yes;
        public Observable<Unit> OnClickNo => _no;

        public Observable<bool> OnYesOrNo => Observable.Merge(
            _yes.Select(_ => true),
            _no.Select(_ => false)
        );

        private readonly Subject<Unit> _maybe = new();
        public Observable<Unit> OnClickMaybe => _maybe;

        public Observable<SampleChoice> OnChoice => Observable.Merge(
            _yes.Select(_ => SampleChoice.Yes),
            _no.Select(_ => SampleChoice.No),
            _maybe.Select(_ => SampleChoice.Maybe)
        );

        private readonly Subject<Unit> _scrim = new();
        public Observable<Unit> OnClickScrim => _scrim;

        private readonly Subject<Unit> _addCount = new();
        private readonly Subject<Unit> _removeCount = new();
        public Observable<Unit> OnClickAddCount => _addCount;
        public Observable<Unit> OnClickRemoveCount => _removeCount;

        private readonly ReactiveProperty<float> _progress = new();
        public ReadOnlyReactiveProperty<float> Progress => _progress;

        private readonly ReactiveProperty<string> _name = new();

        public ReadOnlyReactiveProperty<string> Name => _name;


        public Subscription MapTo(ICommandSubscribable subscribable)
        {
            return new Subscription();
        }

        public void Yes() => _yes.OnNext(Unit.Default);
        public void No() => _no.OnNext(Unit.Default);
        public void Maybe() => _maybe.OnNext(Unit.Default);
        public void Scrim() => _scrim.OnNext(Unit.Default);
        
        public void AddCount() => _addCount.OnNext(Unit.Default);
        public void RemoveCount() => _removeCount.OnNext(Unit.Default);
        
        public void SetProgress(float value) => _progress.Value = value;
        public void SetName(string name) => _name.Value = name;

        public void Dispose()
        {
            Disposable.Dispose(
                _yes, _no, _maybe, _scrim
            );

            Disposable.Dispose(
                _addCount, _removeCount, _progress, _name
            );
        }
    }
}