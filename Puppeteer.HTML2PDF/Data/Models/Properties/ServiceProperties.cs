namespace Puppeteer_HTML2PDF.Data.Models.Properties;

public class ServiceProperties
{
    public int DayToDelete { get; set; }
    public int MaxParallelTasks { get; set; }
    public string OutPathName { get; set; } = null!;

}