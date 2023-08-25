using Microsoft.AspNetCore.Mvc;
using SF.DataGeneration.BLL.Services.BackgroundService.DocumentGeneration;
using SF.DataGeneration.Models.Dto.Document;
using SF.DataGeneration.Models.Enum;

namespace SF.DataGeneration.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DocumentGenerationController : ControllerBase
    {
        private readonly IDocumentGenerationQueueManager _queueManager;

        public DocumentGenerationController(IDocumentGenerationQueueManager queueManager)
        {
            _queueManager = queueManager;
        }


        [HttpPost("GenerateDocumentsOnBot", Name = "GenerateDocumentsOnBot")]
        public async Task<IActionResult> GenerateDocumentsOnBot(DocumentGenerationUserInputDto request, StudioEnvironment environment)
        {
            await _queueManager.QueueDocumentGenerationJob(new Models.BackgroundJob.DocumentGenerationJobItem() { Environment = environment, Request = request });
            return Ok();
        }
    }
}
