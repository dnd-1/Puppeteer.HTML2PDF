using Microsoft.Extensions.Options;
using Puppeteer_HTML2PDF.Data;
using Puppeteer_HTML2PDF.Data.Models.DTO;
using Puppeteer_HTML2PDF.Data.Models.Properties;
using Quartz;

namespace Puppeteer_HTML2PDF.Jobs;

public class RemoveOldTasksJob: IJob
{

    private readonly ApplicationDbContext _context;
    private readonly ILogger<RemoveOldTasksJob> _logger;
    private readonly ServiceProperties _properties;

    public RemoveOldTasksJob(ApplicationDbContext context, ILogger<RemoveOldTasksJob> logger, IOptionsSnapshot<ServiceProperties> properties)
    {
        _context = context;
        _logger = logger;
        _properties = properties.Value;
    }

    public async Task Execute(IJobExecutionContext context)
    {

        _logger.LogInformation("Start delete old record operation");

        try
        {
            var taskList =
                _context.Tasks.Where(t => t.FinishDate < DateTime.Today.AddDays(_properties.DayToDelete * -1)).ToArray();

            foreach (PdfConvertingTask task in taskList)
            {
                
                if (string.IsNullOrEmpty(task.OutPdfFileName)) 
                    continue;

                string fileName = Path.Combine(_properties.OutPathName, task.OutPdfFileName);
                
                if (!File.Exists(fileName)) 
                    continue;
                
                File.Delete(fileName);
                _logger.LogInformation("Success deleted {DelFileName}", fileName);

            }
            
            _context.Tasks.RemoveRange(taskList);
            int res = await _context.SaveChangesAsync();
            _logger.LogInformation("Success deleted {DelRecordCount} old record", res);
            
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error while deleting old record");
        }
        // finally
        // {
        //     await _context.DisposeAsync();
        // }
        
    }
}