using System;
namespace Game.Algorithm;

public class Levenshtein
{
    public static int Get(string s1, string s2)
    {
        int s1len, s2len, x, y, lastdiag, olddiag;
        s1len = s1.Length;
        s2len = s2.Length;
        Span<int> column = stackalloc int[s1len + 1];

        for (y = 1; y <= s1len; y++)
        {
            column[y] = y;
        }

        for (x = 1; x <= s2len; x++)
        {
            column[0] = x;
            for (y = 1, lastdiag = x - 1; y <= s1len; y++)
            {
                olddiag = column[y];
                int cost = (s1[y-1] == s2[x-1]) ? 0 : 1;
                column[y] = min3(column[y]+1, column[y-1]+1, lastdiag+cost);
                lastdiag = olddiag;
            }
        }
        return column[s1len];
    }

    private static int min3(int a, int b, int c)
    {
        if (a < b)
            if (a < c)
                return a;
            else
                return c;
        else
            if (b < c)
                return b;
            else
                return c;
    }
}
