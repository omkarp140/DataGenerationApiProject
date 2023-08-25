using Microsoft.AspNetCore.Mvc;
using SF.DataGeneration.BLL.Services.BackgroundServices.DocumentGeneration;
using SF.DataGeneration.Models.BackgroundJob.DocumentGeneration;
using SF.DataGeneration.Models.Dto.Document;
using SF.DataGeneration.Models.Enum;

namespace SF.DataGeneration.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DocumentGenerationController : ControllerBase
    {
        private readonly IDocumentGenerationQueueManager _documentGenerationQueueManager;

        public DocumentGenerationController(IDocumentGenerationQueueManager documentGenerationQueueManager)
        {
            _documentGenerationQueueManager = documentGenerationQueueManager;
        }


        [HttpPost("GenerateDocumentsOnBot", Name = "GenerateDocumentsOnBot")]
        public async Task<IActionResult> GenerateDocumentsOnBot(DocumentGenerationUserInputDto request, StudioEnvironment environment)
        {
            await _documentGenerationQueueManager.QueueDocumentGenerationJob(new DocumentGenerationBackgroundJob() { Request = request, Environment = environment });
            return Ok();
        }
    }
}
