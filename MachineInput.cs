using Godot;
using System;
using System.Collections.Generic;

public partial class Machine : Node
{
    bool[] keysDown = new bool[45];
    public override void _Input(InputEvent @event)
    {
        if (@event is InputEventKey keyEvent)
        {
            keysDown[keyMapping(keyEvent.Keycode)] = keyEvent.Pressed;
        }
    }

    private int keyMapping(Key k) => k switch
    {
        Key.Kp0 => 0, // numbers will match
        Key.Kp1 => 1,
        Key.Kp2 => 2,
        Key.Kp3 => 3,
        Key.Kp4 => 4,
        Key.Kp5 => 5,
        Key.Kp6 => 6,
        Key.Kp7 => 7,
        Key.Kp8 => 8,
        Key.Kp9 => 9,
        Key.Key0 => 0,
        Key.Key1 => 1,
        Key.Key2 => 2,
        Key.Key3 => 3,
        Key.Key4 => 4,
        Key.Key5 => 5,
        Key.Key6 => 6,
        Key.Key7 => 7,
        Key.Key8 => 8,
        Key.Key9 => 9,
        Key.A => 10, // A-F will match with their hex
        Key.B => 11,
        Key.C => 12,
        Key.D => 13,
        Key.E => 14,
        Key.F => 15, 
        Key.G => 16,
        Key.H => 17,
        Key.I => 18,
        Key.J => 19,
        Key.K => 20,
        Key.L => 21,
        Key.M => 22,
        Key.N => 23,
        Key.O => 24,
        Key.P => 25,
        Key.Q => 26,
        Key.R => 27,
        Key.S => 28,
        Key.T => 29,
        Key.U => 30,
        Key.V => 31,
        Key.W => 32,
        Key.X => 33,
        Key.Y => 34,
        Key.Z => 35,
        Key.Space => 36,
        Key.Enter => 37,
        Key.Tab => 38,
        Key.Shift => 39,
        Key.Up => 40,
        Key.Down => 41,
        Key.Right => 42,
        Key.Left => 43,
        _ => 44
    };
}
