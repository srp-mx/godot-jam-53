using Godot;
using System;
using System.Collections.Generic;

public partial class Machine : Node
{
    bool[] keysDown = new bool[45];

    private void checkKeys()
    {
        keysDown[0] = Input.IsKeyPressed(Key.Kp0) || Input.IsKeyPressed(Key.Key0);
        keysDown[1] = Input.IsKeyPressed(Key.Kp1) || Input.IsKeyPressed(Key.Key1);
        keysDown[2] = Input.IsKeyPressed(Key.Kp2) || Input.IsKeyPressed(Key.Key2);
        keysDown[3] = Input.IsKeyPressed(Key.Kp3) || Input.IsKeyPressed(Key.Key3);
        keysDown[4] = Input.IsKeyPressed(Key.Kp4) || Input.IsKeyPressed(Key.Key4);
        keysDown[5] = Input.IsKeyPressed(Key.Kp5) || Input.IsKeyPressed(Key.Key5);
        keysDown[6] = Input.IsKeyPressed(Key.Kp6) || Input.IsKeyPressed(Key.Key6);
        keysDown[7] = Input.IsKeyPressed(Key.Kp7) || Input.IsKeyPressed(Key.Key7);
        keysDown[8] = Input.IsKeyPressed(Key.Kp8) || Input.IsKeyPressed(Key.Key8);
        keysDown[9] = Input.IsKeyPressed(Key.Kp9) || Input.IsKeyPressed(Key.Key9);
        keysDown[10] = Input.IsKeyPressed(Key.A);
        keysDown[11] = Input.IsKeyPressed(Key.B);
        keysDown[12] = Input.IsKeyPressed(Key.C);
        keysDown[13] = Input.IsKeyPressed(Key.D);
        keysDown[14] = Input.IsKeyPressed(Key.E);
        keysDown[15] = Input.IsKeyPressed(Key.F); 
        keysDown[16] = Input.IsKeyPressed(Key.G);
        keysDown[17] = Input.IsKeyPressed(Key.H);
        keysDown[18] = Input.IsKeyPressed(Key.I);
        keysDown[19] = Input.IsKeyPressed(Key.J);
        keysDown[20] = Input.IsKeyPressed(Key.K);
        keysDown[21] = Input.IsKeyPressed(Key.L);
        keysDown[22] = Input.IsKeyPressed(Key.M);
        keysDown[23] = Input.IsKeyPressed(Key.N);
        keysDown[24] = Input.IsKeyPressed(Key.O);
        keysDown[25] = Input.IsKeyPressed(Key.P);
        keysDown[26] = Input.IsKeyPressed(Key.Q);
        keysDown[27] = Input.IsKeyPressed(Key.R);
        keysDown[28] = Input.IsKeyPressed(Key.S);
        keysDown[29] = Input.IsKeyPressed(Key.T);
        keysDown[30] = Input.IsKeyPressed(Key.U);
        keysDown[31] = Input.IsKeyPressed(Key.V);
        keysDown[32] = Input.IsKeyPressed(Key.W);
        keysDown[33] = Input.IsKeyPressed(Key.X);
        keysDown[34] = Input.IsKeyPressed(Key.Y);
        keysDown[35] = Input.IsKeyPressed(Key.Z);
        keysDown[36] = Input.IsKeyPressed(Key.Space);
        keysDown[37] = Input.IsKeyPressed(Key.Enter);
        keysDown[38] = Input.IsKeyPressed(Key.Tab);
        keysDown[39] = Input.IsKeyPressed(Key.Shift);
        keysDown[40] = Input.IsKeyPressed(Key.Up);
        keysDown[41] = Input.IsKeyPressed(Key.Down);
        keysDown[42] = Input.IsKeyPressed(Key.Right);
        keysDown[43] = Input.IsKeyPressed(Key.Left);
        keysDown[44] = false;
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
