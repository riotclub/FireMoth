using System;
using System.Security;

namespace RiotClub.FireMoth.Services.Output;

/// <summary>
/// Provides a method to display a progress bar in a console window.
/// </summary>
public static class ConsoleProgressBar
{
    private const int ProgressBarColumns = 72;
    
    /// <summary>
    /// Writes a progress bar to console standard output.
    /// </summary>
    /// <param name="progress">A <see cref="float"/> with the amount of progress to display.</param>
    /// <param name="resetCursorLocation">If <c>true</c>, resets the cursor to its original location after displaying
    /// the progress bar.</param>
    public static void WriteProgressBar(float progress, bool resetCursorLocation = true)
    {
        Console.Write($"[{progress * 100,3:F0}% ");
        var progressBarValue = Convert.ToInt32(Math.Floor(progress * ProgressBarColumns));
        var progressBar = new string('|', progressBarValue);
        Console.Write($"{progressBar,-ProgressBarColumns}]");
        if (resetCursorLocation)
            Console.CursorLeft = 0;
        else
            Console.WriteLine();
    }

    
    /// <summary>
    /// Attempts to set console cursor visibility to the provided value.
    /// </summary>
    /// <param name="visible"><c>true</c> to set the cursor visible; false otherwise.</param>
    /// <returns><c>true</c> if on Windows platform; false otherwise.</returns>
    public static bool TrySetCursorVisibility(bool visible)
    {
        var success = false;
        try
        {
            Console.CursorVisible = visible;
            success = true;
        }
        catch (PlatformNotSupportedException) { }
        catch (SecurityException) { }
        
        return success;
    }
}