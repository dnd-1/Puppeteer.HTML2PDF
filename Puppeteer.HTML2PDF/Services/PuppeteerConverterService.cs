using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Puppeteer_HTML2PDF.Data;
using Puppeteer_HTML2PDF.Data.Models.DTO;
using Puppeteer_HTML2PDF.Data.Models.Properties;
using PuppeteerSharp;
using PuppeteerSharp.Media;

namespace Puppeteer_HTML2PDF.Services;

public class PuppeteerConverterService: BackgroundService
{
    private readonly ApplicationDbContext _context;
    private readonly ServiceProperties _properties;
    private readonly ILogger<PuppeteerConverterService> _logger;

    private readonly SemaphoreSlim _taskSemaphore;
    private readonly IBrowser _browser;


    public PuppeteerConverterService(ApplicationDbContext context, IOptionsMonitor<ServiceProperties> properties, ILogger<PuppeteerConverterService> logger)
    {
        _context = context;
        _properties = properties.CurrentValue;
        _logger = logger;
        
        
        new BrowserFetcher().DownloadAsync(BrowserFetcher.DefaultChromiumRevision).Wait();
        _taskSemaphore = new SemaphoreSlim(_properties.MaxParallelTasks);
        
        _browser = Puppeteer.LaunchAsync(new LaunchOptions
        {
            Headless = true,
        }).Result;
    }

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {

            ConvertingState[] endStateList = { ConvertingState.Done, ConvertingState.Cancel, ConvertingState.Error };
            
            if (_context.Tasks.Any(task => !endStateList.Contains(task.State)))
            {
                IEnumerable<Task> tasks = _context.Tasks.Where(task => !endStateList.Contains(task.State)).AsNoTracking()
                    .Select(task => task.Id).ToList().Select(
                        async taskId =>
                        {
                            await _taskSemaphore.WaitAsync(cancellationToken);
                            await Convert(taskId, cancellationToken).WaitAsync(cancellationToken);
                            _taskSemaphore.Release();
                        }
                    );

                try
                {
                    await Task.WhenAll(tasks).WaitAsync(cancellationToken);
                }
                catch (Exception e)
                {
                    _logger.LogError(e, "Exception while running task");
                }

                await _context.SaveChangesAsync(cancellationToken);
            }

            await Task.Delay(5000, cancellationToken);

        }
        
        
        await _browser.CloseAsync();
    }
    
    private async Task Convert(Guid taskId, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Starting convert \"{TaskId}\"", taskId);


        PdfConvertingTask? task =
            await _context.Tasks.SingleOrDefaultAsync(t => t.Id == taskId, cancellationToken: cancellationToken);

        if (task is null)
        {
            _logger.LogError("Error while loading task \"{TaskId}\" from database", taskId);
            return;
        }
        
        if (task.FromUrl is null)
        {
            _logger.LogError("Task with id \"{TaskId}\" dont have valid url for downloading", taskId);

            task.State = ConvertingState.Error;
            task.FinishDate = DateTime.UtcNow;
            return;
        }
        
        if (string.IsNullOrEmpty(task.OutPdfFileName))
        {
            task.OutPdfFileName = $"{task.Id}.pdf";
            _logger.LogWarning("Task with id \"{TaskId}\" dont have out file name. Generating new name {FileName}", task.Id, task.OutPdfFileName);
        }

        await using IPage page = await _browser.NewPageAsync();

        try
        {
            if (!Directory.Exists(_properties.OutPathName))
                Directory.CreateDirectory(_properties.OutPathName);

            await page.GoToAsync(task.FromUrl?.AbsoluteUri).WaitAsync(cancellationToken);
            string pdfFileName = Path.Combine(_properties.OutPathName, task.OutPdfFileName);
            await page.PdfAsync(pdfFileName, new PdfOptions
            {
                Format = PaperFormat.A4,
                DisplayHeaderFooter = true,
            }).WaitAsync(cancellationToken);
            ;

            task.State = ConvertingState.Done;
            task.FinishDate = DateTime.UtcNow;

            _logger.LogInformation("Finish task {TaskId}", taskId);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error");
            throw;
        }
        finally
        {
            await page.CloseAsync();
        }
    }
}