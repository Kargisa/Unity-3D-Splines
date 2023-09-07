using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class IndexHelper
{
    public static int LoopIndex(int i, int count) => (i + count) % count;

    public static float LoopIndex(float i, float count) => (i + count) % count;
}
