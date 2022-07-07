using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomProperties
{
    static public string PATCHES_POSITION = "patchesPosition";
    static public string PATCHES_ACTIVE = "patchesActive";
    static public string WINNING_ACTOR = "winningActor";

#if (UNITY_EDITOR)
    static public int WINNING_TIME = 10;
#else
    static public int WINNING_TIME = 180;
#endif
}
