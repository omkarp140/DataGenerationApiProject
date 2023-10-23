using Microsoft.AspNetCore.Mvc;
using SF.DataGeneration.BLL.Services.BackgroundServices.EmailDataGeneration;
using SF.DataGeneration.Models.BackgroundJob.EmailDataGeneration;
using SF.DataGeneration.Models.Dto.Email;
using SF.DataGeneration.Models.Enum;

namespace SF.DataGeneration.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EmailDataGenerationController : ControllerBase
    {
        private readonly IEmailDataGenerationQueueManager _emailDataGenerationQueueManager;

        public EmailDataGenerationController(IEmailDataGenerationQueueManager emailDataGenerationQueueManager)
        {
            _emailDataGenerationQueueManager = emailDataGenerationQueueManager;
        }

        [HttpPost("SyncBulkEmailsOnEmailBot", Name = "SyncBulkEmailsOnEmailBot")]
        public async Task<IActionResult> SyncBulkEmailsOnEmailBot(EmailDataGenerationUserInputDto request, StudioEnvironment environment)
        {
            await _emailDataGenerationQueueManager.QueueEmailDataGenerationJob(new EmailDataGenerationBackgroundJob() { Request = request, Environment = environment });
            return Ok();
        }
    }
}
