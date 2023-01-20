using System;

namespace Game;

public static class ExternDebug
{
    public static Action<string> printer;

    public static void DBPrint(string txt)
    {
        printer(txt);
    }
}
