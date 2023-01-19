namespace Game.Assembly;

public enum Register
{
    A = 0, 
    B, 
    C, 
    D, 
    IP, // instruction ptr
    SP, // stack ptr
    ZF, // zero flag
    EF, // equal flag
    CF, // carry flag
    LEQF, // leq flag
    LEF, // less flag
    KEYF, // is key down
    HTM, // heat measure
    AMM, // ammo measure

    None // Must be last item
}
