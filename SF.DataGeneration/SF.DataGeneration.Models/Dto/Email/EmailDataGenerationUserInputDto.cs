using SF.DataGeneration.Models.Common;

namespace SF.DataGeneration.Models.Dto.Email
{
    public class EmailDataGenerationUserInputDto : CommonUserInputDto
    {
        public Guid EmailbotId { get; set; }
        public int NoOfEmailToGenerate { get; set; }
    }
}
