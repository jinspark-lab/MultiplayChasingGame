using System.Collections;
using System.Collections.Generic;
using System;

public static class GameMath
{

    public static int GetRandomInt(int min, int max)
    {
        Random rand = new Random();
        return rand.Next(min, max);
    }

}
