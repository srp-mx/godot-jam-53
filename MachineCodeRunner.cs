using Godot;
using System;

using System.Collections;
using System.Threading.Tasks;

using Game.Assembly;

public partial class Machine : Node
{
    IEnumerator currInstruction;
    int instructionPtr;

    bool ended = false;
    Task<bool> task;

    public void StepCode()
    {
        if (!CanRun())
            return;

        task = Task.Factory.StartNew<bool>(() => 
        {
            debugLog("inside tsk");
            bool wouldContinue = currInstruction.MoveNext();
            debugLog("would continue " + wouldContinue.ToString());
            instructionPtr = (int)currInstruction.Current;
            if (!wouldContinue)
            {
                codeLog(code.GetExitStatus().ToString());   
            }
            return wouldContinue;
        });
    }

    private bool CanRun()
    {
        // we have to compile and get it first
        if (currInstruction is null)
            return false;

        // if the code is valid
        if (codeRejected)
            return false;

        // if there's an instruction running, return
        // additionally if it's completed, set ended flag
        if (task is not null)
        {
            if (!task.IsCompleted)
                return false;

            ended = !task.Result;
            //if (ended) debugLog("[ENDED PROGRAM]\n" + code.GetExitStatus().ToString());
            return false;
        }

        debugLog("conditions are a-ok");
        return true;
    }

}
