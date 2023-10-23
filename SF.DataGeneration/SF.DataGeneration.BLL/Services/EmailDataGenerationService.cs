using SF.DataGeneration.BLL.Helpers;
using SF.DataGeneration.BLL.Interfaces;
using SF.DataGeneration.Models.Dto.Email;
using SF.DataGeneration.Models.Enum;
using SF.DataGeneration.Models.StudioApiModels.RequestDto;

namespace SF.DataGeneration.BLL.Services
{
    public class EmailDataGenerationService : IEmailDataGenerationService
    {
        private readonly IEmailBotStudioApiService _emailbotStudioApiService;

        public EmailDataGenerationService(IEmailBotStudioApiService emailBotStudioApiService)
        {
            _emailbotStudioApiService = emailBotStudioApiService;
        }

        public async Task SyncBulkEmailsOnEmailBot(EmailDataGenerationUserInputDto request, StudioEnvironment environment)
        {
            await _emailbotStudioApiService.SetupHttpClientAuthorizationHeaderAndApiUrl(request, environment);
            var emails = await GenerateEmailData(request.NoOfEmailToGenerate);
            foreach (var email in emails)
            {
                var test = await _emailbotStudioApiService.SendEmailToBotInStudio(email);
            }
            
            //ParallelOptions options = new() { MaxDegreeOfParallelism = 5 };
            //await Parallel.ForEachAsync(emails, options, async (email, ct) =>
            //{
            //    var test =  await _emailbotStudioApiService.SendEmailToBotInStudio(email);
            //});
        }

        private async Task<List<EmailBodyDto>> GenerateEmailData(int noOfEmailToGenerate)
        {
            var _random = new Random();            
            var emails = new List<EmailBodyDto>();

            for(int i = 0; i< noOfEmailToGenerate; i++)
            {
                var fromEmailId = TextHelperService.GenerateReadbleRandomData(RandomDataType.EmailId.ToString());
                var subject = $"Claim Request - {TextHelperService.GenerateReadbleRandomData(RandomDataType.Name.ToString())} Sikh";
                var htmlBody = $"<p>Hi Madam, </p> <br /> <p> I have a health insurance with your company and the policy number is {TextHelperService.GenerateReadbleRandomData(RandomDataType.PolicyNo.ToString())}. <br /> Due to an accident last week, " +
                               $"I was admitted to Birla hospital for treatment and <br /> I will be discharged soon. <br /> It would be great if you could release the funds as soon " +
                               $"as possible so I can <br /> clear the bills in a timely manner. My expenditure is covered under the contract <br /> and I would like to settle my bill " +
                               $"through the insurance. <br /> I have attached the bill of USD {_random.Next(50000, 99999).ToString()} duly signed by the hospital authorities <br /> along with the complete report from " +
                               $"the diagnosing doctor. </p> <br /><br /> <p>You can reach me on {TextHelperService.GenerateReadbleRandomData(RandomDataType.MobileNo.ToString())} <br /> Date of birth:- {TextHelperService.GenerateReadbleRandomData(RandomDataType.Date.ToString())} <br /> Location - {TextHelperService.GenerateReadbleRandomData(RandomDataType.City.ToString())} <br /> Claim Amount - {_random.Next(50000, 99999).ToString()} </p>" +
                               $"<br /><br /> <p>Best Regards, <br /> {TextHelperService.GenerateReadbleRandomData(RandomDataType.Name.ToString())} Sikh <br /> Sr. QA // Simplifai AS <br /> Address: The Pavilion, S.No. 105/C, 4th Floor, Baner, <br /> {TextHelperService.GenerateReadbleRandomData(RandomDataType.City.ToString())}- 411045 " +
                               $"<br /> Direct: +91 {TextHelperService.GenerateReadbleRandomData(RandomDataType.MobileNo.ToString())} <br /> {fromEmailId}</p>";

                emails.Add(new EmailBodyDto
                {
                    From = fromEmailId,
                    Subject = subject,
                    HtmlBody = htmlBody,
                    DateReceived = DateTime.Now,
                });
            }
            return emails;
        }
    }
}
