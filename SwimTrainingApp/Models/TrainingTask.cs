﻿using System.ComponentModel.DataAnnotations;

namespace SwimTrainingApp.Models
{
    public class TrainingTask
    {
        public int Id { get; set; }
        public int TrainingId { get; set; } // Klucz obcy do `Training`

        // Nawigacja do `Training` (relacja)
        public Training Training { get; set; }

        public string TrainingSection { get; set; }
        public string TaskDescription { get; set; }
        public int Distance { get; set; }
        public TaskType TaskType { get; set; }
    }



    public enum TaskType
    {
        NN, RR, ANC, ANP, AEC1, AEC2
    }

}
