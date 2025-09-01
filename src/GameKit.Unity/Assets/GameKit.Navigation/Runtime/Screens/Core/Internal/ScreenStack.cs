using System.Collections.Generic;
using GameKit.Common.Results;
using UnityEngine;

namespace GameKit.Navigation.Screens.Core.Internal
{
    internal class ScreenStack
    {
        private readonly Stack<string> _stack = new();

        public bool Any => _stack.Count > 0;

        public void Push(string id)
        {
            _stack.Push(id);
        }

        public FastResult<string> Pop()
        {
            if (_stack.Count == 0)
            {
                return FastResult<string>.Fail("Screen.EmptyStack");
            }

            return FastResult<string>.Ok(_stack.Pop());
        }

        public bool TryPop(out string id)
        {
            if (Any)
            {
                var popResult = Pop();
                if (!popResult.IsError)
                {
                    id = popResult.Value;
                    return true;
                }
            }

            id = null;
            return false;
        }

        public bool TryPeek(out string id)
        {
            if (Any)
            {
                id = _stack.Peek();
                return true;
            }

            id = null;
            return false;
        }

        public void Clear()
        {
            _stack.Clear();
        }
    }
}