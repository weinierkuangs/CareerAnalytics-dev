using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace CareerAnalytics_dev.Models
{
    public class SurveyInfoModel
    {
        [Display(Name = "Number of Survey Taken Last Week")]
        public int NumberOfSurveyTakenLastWeek { get; set; }

        [Display(Name = "Number of Survey Taken Overall")]
        public int NumberOfSurveyTakenOverall { get; set; }

        [Display(Name = "Number of Undergraduates")]
        public int NumberOfUndergradautes { get; set; }
        
    }
}