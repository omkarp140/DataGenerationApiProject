namespace SF.DataGeneration.Models.StudioApiModels.RequestDto
{
    public class EmailBodyDto
    {
        public string Subject { get; set; }
        public string HtmlBody { get; set; }
        public string Body { get; set; }
        public string From { get; set; }
        public DateTime DateReceived { get; set; }
    }
}
