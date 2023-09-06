using Microsoft.AspNetCore.Mvc;
using SF.DataGeneration.BLL.Interfaces;
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
        private readonly IDocumentGenerationService _documentGenerationService;

        public DocumentGenerationController(IDocumentGenerationQueueManager documentGenerationQueueManager,
                                            IDocumentGenerationService documentGenerationService)
        {
            _documentGenerationQueueManager = documentGenerationQueueManager;
            _documentGenerationService = documentGenerationService;
        }


        [HttpPost("GenerateDocumentsOnBot", Name = "GenerateDocumentsOnBot")]
        public async Task<IActionResult> GenerateDocumentsOnBot(DocumentGenerationUserInputDto request, StudioEnvironment environment)
        {
            await _documentGenerationQueueManager.QueueDocumentGenerationJob(new DocumentGenerationBackgroundJob() { Request = request, Environment = environment });
            return Ok();
        }

        [HttpPost("CreateAnnontationSetup", Name = "CreateAnnontationSetup")]
        public async Task<IActionResult> CreateAnnontationSetup(StudioEnvironment environment, Guid documentbotId, string accessToken)
        {
            await _documentGenerationService.CreateAnnontationSetup(environment, documentbotId, accessToken);
            return Ok();
        }

        [HttpPost("MarkDocumentsAsCompleted", Name = "MarkDocumentsAsCompleted")]
        public async Task<IActionResult> MarkSyncedDocumentsAsCompleted(StudioEnvironment environment, Guid documentbotId, string accessToken, string searchText)
        {
            await _documentGenerationService.MarkSyncedDocumentsAsCompleted(environment, documentbotId, accessToken, searchText);
            return Ok();
        }
    }
}
