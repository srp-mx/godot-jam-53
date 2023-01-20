using Godot;
using System;

using Game.Assembly;

public partial class Machine : Node
{
    bool OnCooldown = false;

    private void checkHeat()
    {
        if (registers[(int)Register.HTM] == 255)
        {
            OnCooldown = true;
        }
    }

    private void raiseHeat(int amount)
    {
        registers[(int)Register.HTM] += amount;
        var val = registers[(int)Register.HTM];
        registers[(int)Register.HTM] = val > 255 ? 255 : val;
    }

    private void lowerHeat(int amount)
    {
        registers[(int)Register.HTM] -= amount;
        var val = registers[(int)Register.HTM];
        registers[(int)Register.HTM] = val < 0 ? 0 : val;
    }
}
