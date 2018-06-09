using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace CareerAnalytics_dev.Models
{
    public class ReportModel
    {
        //following is for part 1(academic info)
        [Display(Name = "First Name")]
        public string FirstName { get; set; }

        [Display(Name = "Last Name")]
        public string LastName { get; set; }

        [Display(Name = "Undergraduate GPA")]
        public string UndergraduateGPA { get; set; }

        [Display(Name = "First Undergraduate Major")]
        public string FirstUndergraduateMajor { get; set; }

        [Display(Name = "Second Undergraduate Major")]
        public string SecondUndergraduateMajor { get; set; }

        [Display(Name = "Graduate GPA")]
        public string GraduateGPA { get; set; }

        [Display(Name = "First Graduate Major")]
        public string FirstGraduateMajor { get; set; }

        [Display(Name = "Second Graduate Major")]
        public string SecondGraduateMajor { get; set; }

        //following is for part 2 (skillsets info)

        [Display(Name = "Management Skill Level")]
        public string ManagementSkillLevel { get; set; }

        [Display(Name = "Entrepreneurship Level")]
        public string EntrepreneurshipLevel { get; set; }

        [Display(Name = "Technical Level")]
        public string TechnicalLevel { get; set; }

        [Display(Name = "Marketing Level")]
        public string MarketingLevel { get; set; }


        //following is for part 3 (Performance) 

        [Display(Name = "Top 3 Majors with the Most High Academic Performers (based on GPA 3.3)")]
        public string Top3MajorsWithHighAcademicPerformers { get; set; }
        public string Top3MajorsWithHighAcademicPerformers1 { get; set; }
        public string Top3MajorsWithHighAcademicPerformers2 { get; set; }
        public string Top3MajorsWithHighAcademicPerformers3 { get; set; }

        [Display(Name = "Top 3 Majors with Most the High Career Performers (based on Self-Report)")]
        public string Top3MajorsWithHighCareerPerformers { get; set; }
        public string Top3MajorsWithHighCareerPerformers1 { get; set; }
        public string Top3MajorsWithHighCareerPerformers2 { get; set; }
        public string Top3MajorsWithHighCareerPerformers3 { get; set; }

        [Display(Name = "Top 3 Majors with the Most Low Academic Performers (based on GPA 3.3)")]
        public string Top3MajorsWithLowAcademicPerformers { get; set; }
        public string Top3MajorsWithLowAcademicPerformers1 { get; set; }
        public string Top3MajorsWithLowAcademicPerformers2 { get; set; }
        public string Top3MajorsWithLowAcademicPerformers3 { get; set; }

        [Display(Name = "Top 3 Majors with Most the Low Career Performers (based on Self-Report)")]
        public string Top3MajorsWithLowCareerPerformers { get; set; }
        public string Top3MajorsWithLowCareerPerformers1 { get; set; }
        public string Top3MajorsWithLowCareerPerformers2 { get; set; }
        public string Top3MajorsWithLowCareerPerformers3 { get; set; }

        //following is for part 4 (Satisfaction)

        [Display(Name = "Top 3 Most Enoyable Majors")]
        public string Top3MostEnjoyableMajors { get; set; }
        public string Top3MostEnjoyableMajors1 { get; set; }
        public string Top3MostEnjoyableMajors2 { get; set; }
        public string Top3MostEnjoyableMajors3 { get; set; }

        [Display(Name = "Top 3 Majors with the Most Satisfied Employees")]
        public string Top3MajorsWithMostSatisfiedEmployees { get; set; }
        public string Top3MajorsWithMostSatisfiedEmployees1 { get; set; }
        public string Top3MajorsWithMostSatisfiedEmployees2 { get; set; }
        public string Top3MajorsWithMostSatisfiedEmployees3 { get; set; }

        [Display(Name = "Top 3 Most Hated Majors")]
        public string Top3MostHatedMajors { get; set; }
        public string Top3MostHatedMajors1 { get; set; }
        public string Top3MostHatedMajors2 { get; set; }
        public string Top3MostHatedMajors3 { get; set; }

        [Display(Name = "Top 3 Majors with the Most Unsatisfied Employees")]
        public string Top3MajorsWithMostUnsatisfiedEmployees { get; set; }
        public string Top3MajorsWithMostUnsatisfiedEmployees1 { get; set; }
        public string Top3MajorsWithMostUnsatisfiedEmployees2 { get; set; }
        public string Top3MajorsWithMostUnsatisfiedEmployees3 { get; set; }

        //following is for part 5 (recoomendation)

        [Display(Name = "Top 3 Recommended Majors based on your skillsets")]
        public string Top3RecommendedMajors { get; set; }
        public string Top3RecommendedMajors1 { get; set; }
        public string Top3RecommendedMajors2 { get; set; }
        public string Top3RecommendedMajors3 { get; set; }
    }
}