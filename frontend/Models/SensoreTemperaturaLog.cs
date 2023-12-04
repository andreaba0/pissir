namespace frontend.Models
{
    public class SensoreTemperaturaLog
    {
        public string Id { get; set; }
        public string Tipo { get; set; }
        public required string Time { get; set; }
        public required float Temperatura { get; set; }
    }
}
