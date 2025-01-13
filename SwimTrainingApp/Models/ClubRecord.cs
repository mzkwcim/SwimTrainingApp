namespace SwimTrainingApp.Models
{
    public class ClubRecord
    {
        public int Id { get; set; } // Klucz główny
        public string Distance { get; set; } // dystans
        public string AthleteName { get; set; } // imie
        public string ReadableTime { get; set; } // czasCzytelny
        public string Date { get; set; } // data
        public string City { get; set; } // miasto
        public double Time { get; set; } // czas
    }
}
