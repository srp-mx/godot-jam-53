namespace Game.Assembly;

public enum Register
{
    A = 0, 
    B, 
    C, 
    D, 
    ZF, // zero flag
    EF, // equal flag
    CF, // carry flag
    LEQF, // leq flag
    EQF, // equal flag
    LEF, // less flag
    HTM, // heat measure
    AMM, // ammo measure

    None // Must be last item
}
