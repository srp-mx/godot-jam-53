namespace Codegen;
public class Program
{
    public static void Main(string[] args)
    {
        new ShaderFunctionFix().Do("../materials");
    }
}
