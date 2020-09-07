using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using UnityEditor;
using UnityEngine;

public class screenShotEditor : Editor
{
    static Thread thread;
    [MenuItem("Tool/ScreenShot")]
    static void Init()
    {
        UnityEngine. Debug.Log(Application.dataPath + "/Editor/Debug/Shot.exe");
        Process.Start(Application.dataPath+ "/Editor/Debug/Shot.exe", "H:\\UnityAssets.comb\\22.png"); 
    }


}
