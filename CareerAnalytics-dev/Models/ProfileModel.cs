using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace CareerAnalytics_dev.Models
{
    public class ProfileModel
    {
        public int UserID { get; set; }

        [Display(Name = "First Name")]
        [Required(AllowEmptyStrings = false, ErrorMessage = "First Name required")]
        public string FirstName { get; set; }

        [Display(Name = "Last Name")]
        [Required(AllowEmptyStrings = false, ErrorMessage = "Last Name required")]
        public string LastName { get; set; }

        [Display(Name = "Date of Birth")]
        [DataType(DataType.Date)]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:MM/dd/yyyy}")]
        [Required(AllowEmptyStrings = false, ErrorMessage = "Date of Birth required")]
        public DateTime DateOfBirth { get; set; }

        [Display(Name = "Academic Status")]
        [Required(AllowEmptyStrings = false, ErrorMessage = "Academic Status required")]
        public string AcademicStatus { get; set; }

        [Display(Name = "Employment Status")]
        [Required(AllowEmptyStrings = false, ErrorMessage = "Employment Status required")]
        public string EmploymentStatus { get; set; }

        [Display(Name = "Job Title")]
        [Required(AllowEmptyStrings = false, ErrorMessage = "Job Title required")]
        public string JobTitle { get; set; }

        [Display(Name = "Job Industry")]
        [Required(AllowEmptyStrings = false, ErrorMessage = "Job Industry required")]
        public string JobIndustry { get; set; }

        [Display(Name = "Institution")]
        [Required(AllowEmptyStrings = false, ErrorMessage = "Institution required")]
        public string Institution { get; set; }

        [Display(Name = "Anticipated Graduation Date")]
        [DataType(DataType.Date)]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:MM/dd/yyyy}")]
        [Required(AllowEmptyStrings = false, ErrorMessage = "Anticipated Graduation Date required")]
        public string AnticipatedGraduationDate { get; set; }

        [Display(Name = "Major")]
        [Required(AllowEmptyStrings = false, ErrorMessage = "Major required")]
        public string Major { get; set; }
        
    }
}