public class BoolBox
{
    public bool val { get; private set; } = false;

    public BoolBox(bool state = false)
    {
        val = state;
    }

    public bool Get() => val;
    public void Set(bool val) => this.val = val;
    
}
