﻿using System;

namespace FileManager.Utils;

public static class FileSizeExtensions
{
    // Borrowed from https://stackoverflow.com/a/4975942.
    public static string ToReadableByteString(this long byteCount)
    {
        string[] suf = { "B", "KB", "MB", "GB", "TB", "PB", "EB" }; //Longs run out around EB
        if (byteCount == 0)
            return "0" + suf[0];
        long bytes = Math.Abs(byteCount);
        int place = Convert.ToInt32(Math.Floor(Math.Log(bytes, 1024)));
        double num = Math.Round(bytes / Math.Pow(1024, place), 1);
        return (Math.Sign(byteCount) * num).ToString() + " " +  suf[place];
    }
}