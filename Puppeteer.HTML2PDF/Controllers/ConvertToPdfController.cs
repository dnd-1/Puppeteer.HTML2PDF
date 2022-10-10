using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Puppeteer_HTML2PDF.Data;
using Puppeteer_HTML2PDF.Data.Models.DTO;
using Puppeteer_HTML2PDF.Data.Models.Properties;

namespace Puppeteer_HTML2PDF.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ConvertToPdfController : ControllerBase
    {
        
        private readonly ILogger<ConvertToPdfController> _logger;
        private readonly ApplicationDbContext _context;
        private readonly ServiceProperties _properties;


        public ConvertToPdfController(ILogger<ConvertToPdfController> logger,
            ApplicationDbContext context,
            IOptionsSnapshot<ServiceProperties> properties)
        {
            _logger = logger;
            _context = context;
            _properties = properties.Value;
        }

        /// <summary>
        /// Get convert task by id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        /// 
        [HttpGet]
        public async Task<ActionResult<PdfConvertingTask>> OnGet(Guid id)
        {
            if (id == Guid.Empty)
            {
                _logger.LogError("Empty Guid");
                return NotFound();
            }
            
            if (!_context.Tasks.Any(t => t.Id == id))
            {
                _logger.LogError("no task with id {TaskId}", id);
                return NotFound($"no task with id \"{id}\"");
            }

            PdfConvertingTask? task = _context.Tasks.SingleOrDefault(t => t.Id == id);

            task.OutPdfFileFullName = $"{_properties.OutPathName}/{task.OutPdfFileName}";

            return Ok(task);
        }

        /// <summary>
        /// Add new task for create
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        /// 
        [HttpPost]
        public async Task<IActionResult> OnPost([FromForm]string url)
        {
            if (string.IsNullOrEmpty(url.Trim()))
            {
                _logger.LogError("Empty url string");
                return BadRequest("Empty url string");
            }


            if (!(Uri.TryCreate(url, UriKind.Absolute, out Uri? uriResult) 
                  && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps)))
            {
                _logger.LogError("Can't validate url string \"{Url}\"", url);
                return BadRequest($"Can't validate url string \"{url}\"");
            }

            PdfConvertingTask task = new PdfConvertingTask()
            {
                Id = Guid.NewGuid(),
                State = ConvertingState.Waiting,
                FromUrl = uriResult,
                OutPdfFileName = "",
            };

            _context.Tasks.Add(task);
            await _context.SaveChangesAsync();

            return Ok(task);
        }
        
    }
}