using System;

namespace TestTwitch.lib;

public class GodotLogger {
    private string? subjectName = null;
    private string? level = null;
    private Godot.RichTextLabel? _label = null;

    public GodotLogger(Type klass, Godot.RichTextLabel label) {
        subjectName = klass.Name;
        _label = label;
    }

    public void Log(string level="INFO", string color = "white", string msg="") {
        _label!.AppendText($"[color={color}][{subjectName}:{level}][/color] {msg}\n");
    }

    public void LogWarning(string msg) {
        Log("WARN","yellow",msg);
    }

    public void LogInformation(string msg) {
        Log("INFO", "green", msg);
    }

    public void LogError(string msg) {
        Log("ERROR", "red", msg);
    }

    public void LogDebug(string msg) {
        Log("DEBUG", "blue", msg);
    }

    public void LogCritical(string msg) {
        Log("CRIT", "fuchsia", msg);
    }
}