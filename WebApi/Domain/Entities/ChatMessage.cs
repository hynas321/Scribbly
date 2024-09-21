namespace WebApi.Domain.Entities;

public class ChatMessage
{
    public string Username { get; set; }
    public string Text { get; set; }
    public string BootstrapBackgroundColor { get; set; }
}

public class BootstrapColors
{
    public static string Red = "danger";
    public static string Yellow = "warning";
    public static string Green = "success";
    public static string Blue = "primary";
    public static string Cyan = "info";
}