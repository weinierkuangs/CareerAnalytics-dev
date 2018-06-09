using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace CareerAnalytics_dev.Models
{
    public class Captcha
    {
        //model specific fields 
        [Required]
        [Display(Name = "How much is the sum")]
        public string captcha { get; set; }
    }
}