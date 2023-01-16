using System;
using System.Collections;
namespace Game.Assembly;

// NOTE(srp): We iterate over the object code in order to ensure 
// correctness automagically

public partial class CompiledCode
{
    private class VMIterator : IEnumerator
    {
        private CompiledCode target;
        private Action<VMIterator> unsubscribeTarget;
        private int instrPtr = 0;

        public VMIterator(CompiledCode target, Action<VMIterator> onDestroy)
        {
            this.target = target;
            unsubscribeTarget = onDestroy;
        }

        ~VMIterator()
        {
            unsubscribeTarget(this);
        }

        public void OnRecompile() { Reset(); }

        public bool MoveNext()
        {
            if (instrPtr < 0 || instrPtr > 255)
            {
                target.runtimeError = "[ERROR]: Tried to execute instruction memory out of bounds.";
                return false;
            }

            if (target.MethodArea[instrPtr].IsUnassigned())
            {
                target.runtimeError = "[ERROR]: Tried to execute unassigned instruction memory at runtime.";
                return false;
            }

            return target.MethodArea[instrPtr].Execute(target.MethodArea, ref instrPtr, out target.runtimeError); 
        }

        public void Reset()
        {
            instrPtr = 0;
        }

        object IEnumerator.Current
        {
            get
            {
                return Current;
            }
        }

        public int Current
        {
            get
            {
                return instrPtr;
            }
        }
    }
}
