using Microsoft.AspNetCore.Mvc;
using SF.DataGeneration.BLL.Interfaces;
using SF.DataGeneration.Models.Dto.Document;
using SF.DataGeneration.Models.Enum;

namespace SF.DataGeneration.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DocumentGenerationController : ControllerBase
    {
        private readonly IDocumentGenerationService _documentGenerationService;

        public DocumentGenerationController(IDocumentGenerationService documentGenerationService)
        {
            _documentGenerationService = documentGenerationService;
        }


        [HttpPost("GenerateDocumentsOnBot", Name = "GenerateDocumentsOnBot")]
        public async Task GenerateDocumentsOnBot(DocumentGenerationUserInputDto request, StudioEnvironment environment)
        {
            await _documentGenerationService.GenerateDocumentsWithExcelData(request, environment);
        }
    }
}
