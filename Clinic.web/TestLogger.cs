using System;
using System.IO;

public class TestLogger {
    public static void Log(string msg) {
        File.AppendAllText(@"m:\IValley\Clinic\Clinic\Clinic.web\sitemap_debug.txt", msg + Environment.NewLine);
    }
}
