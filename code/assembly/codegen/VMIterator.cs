using System;
using System.Collections;
using System.Collections.Generic;
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
        // TODO(srp): get them here somehow vvvv
        private InstructionSets sets;
        private ICollection<InstructionSets.Available> availSets;

        public VMIterator(CompiledCode target, Action<VMIterator> onDestroy)
        {
            this.target = target;
            unsubscribeTarget = onDestroy;
        }

        ~VMIterator()
        {
            unsubscribeTarget(this);
        }

        public void SetInstructionSets(InstructionSets sets)
        {
            this.sets = sets;
        }

        public void SetAvailableSets(ICollection<InstructionSets.Available> availSets)
        {
            this.availSets = availSets;
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
                target.runtimeError = $"[ERROR] {target.MethodArea[instrPtr].GetSourcePos()}: Tried to execute unassigned instruction memory at runtime.";
                return false;
            }

            if (target.MethodArea[instrPtr].GetParamInfo().GetParamType() != ParamInfo.ParamType.None)
            {
                // TODO: address is assigned but it's parameter
                var instrQuery = sets.Get(target.MethodArea[instrPtr].GetParamInfo().Get(), availSets);
                var instr = instrQuery.Match(x=>x, ()=>null);
                if (instr is null)
                {
                    target.runtimeError = $"[ERROR] {target.MethodArea[instrPtr].GetSourcePos()}: No instruction matches this index: {target.MethodArea[instrPtr].GetParamInfo().Get()}.";
                    return false;
                }
                instr.Instruction(target.MethodArea, ref instrPtr, out target.runtimeError);
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
