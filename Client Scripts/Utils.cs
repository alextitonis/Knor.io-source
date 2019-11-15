using System;
using System.Linq;
using UnityEngine;

public static class Utils
{
    public static bool CloseEnough(Vector3 value1, Vector3 value2, double acceptableDifference)
    {
        bool ok = false;
        if (Mathf.Abs(value1.x - value2.x) <= acceptableDifference)
            ok = true;
        else if (Mathf.Abs(value1.y - value2.y) <= acceptableDifference)
            ok = true;
        else if (Mathf.Abs(value1.z - value2.z) <= acceptableDifference)
            ok = true;

        return ok;
    }
    public static bool CloseEnough(Quaternion value1, Quaternion value2, double acceptableDifference)
    {
        bool ok = false;
        if (Mathf.Abs(value1.x - value2.x) <= acceptableDifference)
            ok = true;
        else if (Mathf.Abs(value1.y - value2.y) <= acceptableDifference)
            ok = true;
        else if (Mathf.Abs(value1.z - value2.z) <= acceptableDifference)
            ok = true;
        else if (Mathf.Abs(value1.w - value2.w) <= acceptableDifference)
            ok = true;

        return ok;
    }

    public static bool HasArguments()
    {
        var args = Environment.GetCommandLineArgs();
        return args.Length > 0;
    }
    public static string GetArg(string name)
    {
        var args = Environment.GetCommandLineArgs();
        for (int i = 0; i < args.Length; i++)
        {
            if (args[i] == name && args.Length > i + 1)
            {
                return args[i + 1];
            }
        }
        return "";
    }

    public static string RandomString(int length)
    {
        System.Random rnd = new System.Random();
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        return new string(Enumerable.Repeat(chars, length)
          .Select(s => s[rnd.Next(s.Length)]).ToArray());
    }
}