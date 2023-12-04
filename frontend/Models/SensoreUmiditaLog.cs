namespace frontend.Models
{
    public class SensoreUmiditaLog
    {
        public string Id { get; set; }
        public string Tipo { get; set; }
        public required string Time { get; set; }
        public required float Umidita { get; set; }
    }
}
