using System;
using UnityEngine.Assertions;

namespace GameKit.SceneLauncher
{
    public sealed class InitializableLazy<T>
    {
        private readonly Func<T> _valueFactory;

        private T _value;

        public InitializableLazy(Func<T> valueFactory)
        {
            Assert.IsNotNull(valueFactory);
            _valueFactory = valueFactory;
        }

        public T Value
        {
            get
            {
                if (_value == null)
                {
                    _value = _valueFactory();
                }

                return _value;
            }
        }

        public void Initialize()
        {
            _value = _valueFactory();
        }
    }
}