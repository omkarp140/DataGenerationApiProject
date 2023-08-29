using Microsoft.AspNetCore.Mvc;
using SF.DataGeneration.BLL.Services.BackgroundServices.DocumentGeneration;
using SF.DataGeneration.BLL.Services.BackgroundServices.SendingDocumentsWithoutTagging;
using SF.DataGeneration.Models.BackgroundJob.DocumentGeneration;
using SF.DataGeneration.Models.BackgroundJob.DocumentSending;
using SF.DataGeneration.Models.Dto.Document;
using SF.DataGeneration.Models.Enum;

namespace SF.DataGeneration.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DocumentGenerationController : ControllerBase
    {
        private readonly IDocumentGenerationQueueManager _documentGenerationQueueManager;
        private readonly IDocumentSendingQueueManager _documentSendingQueueManager;

        public DocumentGenerationController(IDocumentGenerationQueueManager documentGenerationQueueManager,
                                            IDocumentSendingQueueManager documentSendingQueueManager)
        {
            _documentGenerationQueueManager = documentGenerationQueueManager;
            _documentSendingQueueManager = documentSendingQueueManager;
        }


        [HttpPost("GenerateDocumentsOnBot", Name = "GenerateDocumentsOnBot")]
        public async Task<IActionResult> GenerateDocumentsOnBot(DocumentGenerationUserInputDto request, StudioEnvironment environment)
        {
            await _documentGenerationQueueManager.QueueDocumentGenerationJob(new DocumentGenerationBackgroundJob() { Request = request, Environment = environment });
            return Ok();
        }

        [HttpPost("SendDocumentsToBotWithoutTagging", Name = "SendDocumentsToBotWithoutTagging")]
        public async Task<IActionResult> SendDocumentsToBotWithoutTagging(DocumentGenerationUserInputDto request, StudioEnvironment environment)
        {
            await _documentSendingQueueManager.QueueDocumentSendingJob(new DocumentSendingBackgroundJob() { Request = request, Environment = environment });
            return Ok();
        }
    }
}
