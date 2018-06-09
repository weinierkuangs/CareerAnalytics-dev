using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.ComponentModel.DataAnnotations;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data;
using System.Net;
using CareerAnalytics_dev.Models;
using System.Net.Mail;
using System.Web.Security;
using System.Data.Entity.Validation;
using System.IO;
using System.Web.Helpers;
using System.Data.SqlClient;
using System.Drawing;
using System.IO;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Security.Principal;
using System.Threading;

namespace CareerAnalytics_dev.Controllers
{
    public class UserController : Controller
    {
        //Registration Action
        [HttpGet]
        public ActionResult Registration()
        {
            return View();
        }
        //Registration POST action 
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Registration([Bind(Exclude = "IsEmailVerified,ActivationCode")] User user)
        {
            bool Status = false;
            string message = "";
           
            //
            // Model Validation 
            if (ModelState.IsValid)
            {

                #region //Email is already Exist 
                var isExist = IsEmailExist(user.EmailID);
                if (isExist)
                {
                    ModelState.AddModelError("EmailExist", "Email already exist");
                    return View(user);
                }
                #endregion

                #region Generate Activation Code 
                user.ActivationCode = Guid.NewGuid();
                #endregion

                #region Generate existing password 
                user.ExistingPassword = "password";
                #endregion
              
                user.IsEmailVerified = false;

                #region Save to Database
                using (uamanagementEntities dc = new uamanagementEntities())
                {

                    dc.Users.Add(user);
                    dc.SaveChanges();

                    //Send Email to User
                    SendVerificationLinkEmail(user.EmailID, user.ActivationCode.ToString());
                    message = "Registration successfully done. Account activation link " +
                        " has been sent to your email id:" + user.EmailID;
                    Status = true;
                }
                #endregion
            }
            else
            {
                message = "Invalid Request";
            }

            ViewBag.Message = message;
            ViewBag.Status = Status;
            return View(user);
        }

        //Verify Account  
        [HttpGet]
        public ActionResult VerifyAccount(string id)
        {
            bool Status = false;
            using (uamanagementEntities dc = new uamanagementEntities())
            {
                dc.Configuration.ValidateOnSaveEnabled = false; // This line I have added here to avoid 
                                                               
                var v = dc.Users.Where(a => a.ActivationCode == new Guid(id)).FirstOrDefault();
                if (v != null)
                {
                    v.IsEmailVerified = true;
                    dc.SaveChanges();
                    Status = true;
                }
                else
                {
                    ViewBag.Message = "Invalid Request";
                }
            }
            ViewBag.Status = Status;
            return View();
        }

        //Login 
        public ActionResult Login()
        {
            return View();
        }

        //Login POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(UserLogin model,UserLogin login, string ReturnUrl = "")
        {
            if (Session["Captcha"] == null || Session["Captcha"].ToString() != model.Captcha)
            {
                ModelState.AddModelError("Captcha", "Wrong value of sum, please try again.");
                //dispay error and generate a new captcha 
                return View(model);
            }

            string message = "";
            using (uamanagementEntities dc = new uamanagementEntities())
            {
                var v = dc.Users.Where(a => a.EmailID == login.EmailID).FirstOrDefault();
                if (v != null)
                {
                    if (!v.IsEmailVerified)
                    {
                        ViewBag.Message = "Please verify your email first";
                        return View();
                    }

                    if (string.Compare(login.Password, v.Password) == 0)
                    {
                        int timeout = login.RememberMe ? 525600 : 20; // 525600 min = 1 year
                        var ticket = new FormsAuthenticationTicket(login.EmailID, login.RememberMe, timeout);
                        string encrypted = FormsAuthentication.Encrypt(ticket);
                        var cookie = new HttpCookie(FormsAuthentication.FormsCookieName, encrypted);
                        cookie.Expires = DateTime.Now.AddMinutes(timeout);
                        cookie.HttpOnly = true;
                        Response.Cookies.Add(cookie);


                        if (Url.IsLocalUrl(ReturnUrl))
                        {

                            
                            return Redirect(ReturnUrl);
                        }
                        else
                        {
                            String UserType = getUserType(login.EmailID);
                            if(UserType == "1")
                            { 
                                return RedirectToAction("AdminDashboard", "User");
                            }
                            else
                            {
                                return RedirectToAction("Index", "Home");
                            }
                        }
                    }
                    else
                    {
                        
                        message = "Invalid credential provided";
                    }
                }
                else
                {
                    message = "Invalid credential provided";
                }
            }
            ViewBag.Message = message;
            return View();
        }

        /*This action generates the captcha image that is put on the login page*/
        public ActionResult CaptchaImage(string prefix, bool noisy = true)
        {
            var rand = new Random((int)DateTime.Now.Ticks);
            //generate new question 
            int a = rand.Next(10, 99);
            int b = rand.Next(0, 9);
            var captcha = string.Format("{0} + {1} = ?", a, b);

            //store answer 
            Session["Captcha" + prefix] = a + b;

            //image stream 
            FileContentResult img = null;

            using (var mem = new MemoryStream())
            using (var bmp = new Bitmap(130, 30))
            using (var gfx = Graphics.FromImage((Image)bmp))
            {
                gfx.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
                gfx.SmoothingMode = SmoothingMode.AntiAlias;
                gfx.FillRectangle(Brushes.White, new Rectangle(0, 0, bmp.Width, bmp.Height));

                //add noise 
                if (noisy)
                {
                    int i, r, x, y;
                    var pen = new Pen(Color.Yellow);
                    for (i = 1; i < 10; i++)
                    {
                        pen.Color = Color.FromArgb(
                        (rand.Next(0, 255)),
                        (rand.Next(0, 255)),
                        (rand.Next(0, 255)));

                        r = rand.Next(0, (130 / 3));
                        x = rand.Next(0, 130);
                        y = rand.Next(0, 30);

                        gfx.DrawEllipse(pen, x - r, y - r, r, r);
                    }
                }
                //add question 
                gfx.DrawString(captcha, new Font("Tahoma", 15), Brushes.Gray, 2, 3);

                //render as Jpeg 
                bmp.Save(mem, System.Drawing.Imaging.ImageFormat.Jpeg);
                img = this.File(mem.GetBuffer(), "image/Jpeg");
            }

            return img;
        }

        

        //Dashboard Action
        [HttpGet]
        public ActionResult Dashboard(User user)
        {
            TempData["name"] = user.FirstName + " " + user.LastName;
            return View();
        }
        //Logout
        [Authorize]
        [HttpPost]
        public ActionResult Logout()
        {
            FormsAuthentication.SignOut();
            return RedirectToAction("Login", "User");
        }

        //gets the forgot password view
        [HttpGet]
        public ActionResult ForgotPassword()
        {
            return View();
        }

        //This action checks if the email is valid. If it is it will send an email with a link to the resetPassword view
        [HttpPost]
        public ActionResult ForgotPassword(string EmailID)
        {
            //Verify Email ID
            //Generate Reset password link 
            //Send Email 
            string message = "";
            bool status = false;

            using (uamanagementEntities dc = new uamanagementEntities())
            {
                var account = dc.Users.Where(a => a.EmailID == EmailID).FirstOrDefault();
                if (account != null)
                {
                    //Send email for reset password
                    string resetCode = Guid.NewGuid().ToString();
                    SendVerificationLinkEmail(account.EmailID, resetCode, "ResetPassword");
                    account.ResetPasswordCode = resetCode;
                    //This line I have added here to avoid confirm password not match issue , as we had added a confirm password property 
                    //in our model class in part 1
                    dc.Configuration.ValidateOnSaveEnabled = false;
                    dc.SaveChanges();
                    message = "Reset password link has been sent to your email id.";
                }
                else
                {
                    message = "Account not found";
                }
            }
            ViewBag.Message = message;
            return View();
        }

        //This action verifies the code that is generated with the link
        public ActionResult ResetPassword(string id)
        {
            //Verify the reset password link
            //Find account associated with this link
            //redirect to reset password page
            if (string.IsNullOrWhiteSpace(id))
            {
                return HttpNotFound();
            }

            using (uamanagementEntities dc = new uamanagementEntities())
            {
                var user = dc.Users.Where(a => a.ResetPasswordCode == id).FirstOrDefault();
                if (user != null)
                {
                    ResetPasswordModel model = new ResetPasswordModel();
                    model.ResetCode = id;
                    return View(model);
                }
                else
                {
                    return HttpNotFound();
                }
            }
        }

        //This action takes the users new password and verifies it based on the criteria given
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ResetPassword(ResetPasswordModel model)
        {
            var message = "";

            if (ModelState.IsValid)
            {
                #region //Previous password 
                var isExist = IsPasswordExist(model.NewPassword);
                if (isExist)
                {
                    ModelState.AddModelError("PasswordExist", "You have previously used this password. Please enter a new password.");
                    return View(model);
                }
                #endregion

                using (uamanagementEntities dc = new uamanagementEntities())
                {
                    var user = dc.Users.Where(a => a.ResetPasswordCode == model.ResetCode).FirstOrDefault();
                    if (user != null)
                    {
                        user.Password = model.NewPassword;
                        user.ResetPasswordCode = "";
                        dc.Configuration.ValidateOnSaveEnabled = false;
                        dc.SaveChanges();
                        message = "New password updated successfully";
                    }
                }
            }
            else
            {
                message = "Something invalid";
            }
            ViewBag.Message = message;
            return View(model);
        }

        //This method is used to check if the password exists in the database
        [NonAction]
        public bool IsPasswordExist(string existingPassword)
        {
            using (uamanagementEntities dc = new uamanagementEntities())
            {
                var v = dc.Users.Where(a => a.Password == existingPassword).FirstOrDefault();
                return v != null;
            }
        }

        //This method is used to check if the email exists in the database
        [NonAction]
        public bool IsEmailExist(string emailID)
        {
            using (uamanagementEntities dc = new uamanagementEntities())
            {
                var v = dc.Users.Where(a => a.EmailID == emailID).FirstOrDefault();
                return v != null;
            }
        }

        //This method is used to send the verification email with a link 
        [NonAction]
        public void SendVerificationLinkEmail(string emailID, string activationCode, string emailFor = "VerifyAccount")
        {
            var verifyUrl = "/User/" + emailFor + "/" + activationCode;
            var link = Request.Url.AbsoluteUri.Replace(Request.Url.PathAndQuery, verifyUrl);

            var fromEmail = new MailAddress("careeranalytics2018@gmail.com", "Testing");
            var toEmail = new MailAddress(emailID);
            var fromEmailPassword = "yujie-1276"; // Replace with actual password 

            string subject = "";
            string body = "";
            if (emailFor == "VerifyAccount")
            {
                subject = "Your account is successfully created!";
                body = "<br/><br/>We are excited to tell you that your Dotnet Awesome account is" +
                    " successfully created. Please click on the below link to verify your account" +
                    " <br/><br/><a href='" + link + "'>" + link + "</a> ";
            }
            else if (emailFor == "ResetPassword")
            {
                subject = "Reset Password";
                body = "Hi,<br/><br/>We got request for reset your account password. Please click on the below link to reset your password" +
                    "<br/><br/><a href=" + link + ">Reset Password link</a>";
            }


            var smtp = new SmtpClient
            {
                Host = "smtp.gmail.com",
                Port = 587,
                EnableSsl = true,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(fromEmail.Address, fromEmailPassword)
            };

            using (var message = new MailMessage(fromEmail, toEmail)
            {
                Subject = subject,
                Body = body,
                IsBodyHtml = true
            })
                smtp.Send(message);
        }


        string connectionString = "data source=Uamanagement.eil-server.cba.ua.edu, 1433;initial catalog=uamanagement;persist security info=True;user id=sa;Password=Capstone2018;multipleactiveresultsets=True;application name=EntityFramework";
       
        // GET: Profile
        public ActionResult UserProfile(string emailid)
        {
            ProfileModel productModel = new ProfileModel();
            DataTable dtblProfile = new DataTable();
            using (SqlConnection sqlCon = new SqlConnection(connectionString))
            {
                sqlCon.Open();
                string query = "SELECT UserID, FirstName, LastName, DateOfBirth, AcademicStatus," +
                                                          "EmploymentStatus, JobTitle, JobIndustry, Institution," +
                                                          "AnticipatedGraduationDate, Major FROM Users where EmailID =@EmailID";
                SqlDataAdapter sqlDa = new SqlDataAdapter(query, sqlCon);
                sqlDa.SelectCommand.Parameters.AddWithValue("@EmailID", emailid);
                sqlDa.Fill(dtblProfile);
            }
            if (dtblProfile.Rows.Count == 1)
            {
                productModel.UserID = Convert.ToInt32(dtblProfile.Rows[0][0].ToString());
                productModel.FirstName = dtblProfile.Rows[0][1].ToString();
                productModel.LastName = dtblProfile.Rows[0][2].ToString();
                productModel.DateOfBirth = Convert.ToDateTime(dtblProfile.Rows[0][3].ToString());
                productModel.AcademicStatus = dtblProfile.Rows[0][4].ToString();
                productModel.EmploymentStatus = dtblProfile.Rows[0][5].ToString();
                productModel.JobTitle = dtblProfile.Rows[0][6].ToString();
                productModel.JobIndustry = dtblProfile.Rows[0][7].ToString();
                productModel.Institution = dtblProfile.Rows[0][8].ToString();
                productModel.AnticipatedGraduationDate = dtblProfile.Rows[0][9].ToString();
                productModel.Major = dtblProfile.Rows[0][10].ToString();
                return View(productModel);
            }
            else
                return RedirectToAction("Dashboard");
        }

        //show data on profileEdit page
        public ActionResult ProfileEdit(String emailid)
        {
            ProfileModel profileModel = new ProfileModel();
            DataTable dtblProfile = new DataTable();
            using (SqlConnection sqlCon = new SqlConnection(connectionString))
            {
                sqlCon.Open();
                string query = "SELECT UserID, FirstName, LastName, DateOfBirth, AcademicStatus," +
                                                          "EmploymentStatus, JobTitle, JobIndustry, Institution," +
                                                          "AnticipatedGraduationDate, Major FROM Users where EmailID =@EmailID";
                SqlDataAdapter sqlDa = new SqlDataAdapter(query, sqlCon);
                sqlDa.SelectCommand.Parameters.AddWithValue("@EmailID", emailid);
                sqlDa.Fill(dtblProfile);
            }
            if (dtblProfile.Rows.Count == 1)
            {
                profileModel.UserID = Convert.ToInt32(dtblProfile.Rows[0][0].ToString());
                profileModel.FirstName = dtblProfile.Rows[0][1].ToString();
                profileModel.LastName = dtblProfile.Rows[0][2].ToString();
                profileModel.DateOfBirth = Convert.ToDateTime(dtblProfile.Rows[0][3].ToString());
                profileModel.AcademicStatus = dtblProfile.Rows[0][4].ToString();
                profileModel.EmploymentStatus = dtblProfile.Rows[0][5].ToString();
                profileModel.JobTitle = dtblProfile.Rows[0][6].ToString();
                profileModel.JobIndustry = dtblProfile.Rows[0][7].ToString();
                profileModel.Institution = dtblProfile.Rows[0][8].ToString();
                profileModel.AnticipatedGraduationDate = dtblProfile.Rows[0][9].ToString();
                profileModel.Major = dtblProfile.Rows[0][10].ToString();
                return View(profileModel);
            }
            else
                return RedirectToAction("Dashboard");
        }

        // GET: SurveyInfo
        public ActionResult SurveyInfo()
        {
            SurveyInfoModel productModel = new SurveyInfoModel();
            DataTable dtblProfile = new DataTable();
            using (SqlConnection sqlCon = new SqlConnection(connectionString))
            {
                sqlCon.Open();
                string query = "select count(*), (select count(*) from[dbo].[SurveyResponses$] where Q2#16_1 = 'Bachelor''s Degree'),(select count(*) from[dbo].[SurveyResponses$] WHERE  GETDATE() - EndDate > 0 AND GETDATE() - EndDate < 7)from[dbo].[SurveyResponses$]";
                SqlDataAdapter sqlDa = new SqlDataAdapter(query, sqlCon);
                sqlDa.Fill(dtblProfile);
            }
            if (dtblProfile.Rows.Count == 1)
            {
                productModel.NumberOfSurveyTakenOverall = Convert.ToInt32(dtblProfile.Rows[0][0].ToString());
                productModel.NumberOfUndergradautes = Convert.ToInt32(dtblProfile.Rows[0][1].ToString());
                productModel.NumberOfSurveyTakenLastWeek = Convert.ToInt32(dtblProfile.Rows[0][2].ToString());
                return View(productModel);
            }
            else
                return RedirectToAction("SurveyInfo");
        }

        //edit data on profileEdit page
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ProfileEdit(ProfileModel profileModel)
        {
            using (SqlConnection sqlCon = new SqlConnection(connectionString))
            {
                sqlCon.Open();
                string query = "UPDATE Users SET FirstName = @FirstName , LastName= @LastName , DateOfBirth = @DateOfBirth ," +
                                "AcademicStatus = @AcademicStatus, EmploymentStatus = @EmploymentStatus, JobTitle = @JobTitle," +
                               "JobIndustry = @JobIndustry, Institution = @Institution, AnticipatedGraduationDate = @AnticipatedGraduationDate, " +
                               "Major = @Major WHere UserID = @UserID";
                SqlCommand sqlCmd = new SqlCommand(query, sqlCon);
                sqlCmd.Parameters.AddWithValue("@UserID", profileModel.UserID);
                sqlCmd.Parameters.AddWithValue("@FirstName", profileModel.FirstName);
                sqlCmd.Parameters.AddWithValue("@LastName", profileModel.LastName);
                sqlCmd.Parameters.AddWithValue("@DateOfBirth", profileModel.DateOfBirth);
                sqlCmd.Parameters.AddWithValue("@AcademicStatus", profileModel.AcademicStatus);
                sqlCmd.Parameters.AddWithValue("@EmploymentStatus", profileModel.EmploymentStatus);
                sqlCmd.Parameters.AddWithValue("@JobTitle", profileModel.JobTitle);
                sqlCmd.Parameters.AddWithValue("@JobIndustry", profileModel.JobIndustry);
                sqlCmd.Parameters.AddWithValue("@Institution", profileModel.Institution);
                sqlCmd.Parameters.AddWithValue("@AnticipatedGraduationDate", profileModel.AnticipatedGraduationDate);
                sqlCmd.Parameters.AddWithValue("@Major", profileModel.Major);
                sqlCmd.ExecuteNonQuery();
            }
            return RedirectToAction("Dashboard");
        }

        //This action gets the admindashboard view
        [HttpGet]
        public ActionResult AdminDashboard()
        {
            var entities = new uamanagementEntities();

            return View(entities.Users.ToList());

        }
        
        //This method is used to get the type of user ex. Undergraduate or Admin
        [NonAction]
        public String getUserType(string emailID)
        {
            UserTypeModel userTypeModel = new UserTypeModel(); DataTable dtblProfile = new DataTable();
            using (SqlConnection sqlCon = new SqlConnection(connectionString))
            {
                sqlCon.Open();
                string query = "SELECT UTID FROM Users where EmailID =@EmailID";
                SqlDataAdapter sqlDa = new SqlDataAdapter(query, sqlCon);
                sqlDa.SelectCommand.Parameters.AddWithValue("@EmailID", emailID);
                sqlDa.Fill(dtblProfile);
            }
            String UserType = dtblProfile.Rows[0][0].ToString();
            return UserType;
        }

        //This action uses the users email id to pull up the report view with the information taken from the survey
        [HttpGet]
        public ActionResult Reports(string emailid)
        {
            ReportModel reportModel = new ReportModel();
            //part 1 academic info
            DataTable dtblReportAcademicInfo = new DataTable();
            using (SqlConnection sqlCon = new SqlConnection(connectionString))
            {
                sqlCon.Open();
                string query = "SELECT RecipientFirstName, RecipientLastName,Q2#13,Q2581,Q2601,Q193,Q272,Q499 FROM SurveyResponses where RecipientEmail =@EmailID";
                SqlDataAdapter sqlDa = new SqlDataAdapter(query, sqlCon);
                sqlDa.SelectCommand.Parameters.AddWithValue("@EmailID", emailid);
                sqlDa.Fill(dtblReportAcademicInfo);
            }
            if (dtblReportAcademicInfo.Rows.Count == 1)
            {
                reportModel.FirstName = dtblReportAcademicInfo.Rows[0][0].ToString();
                reportModel.LastName = dtblReportAcademicInfo.Rows[0][1].ToString();
                reportModel.UndergraduateGPA = dtblReportAcademicInfo.Rows[0][2].ToString();
                reportModel.FirstUndergraduateMajor = dtblReportAcademicInfo.Rows[0][3].ToString();
                reportModel.SecondUndergraduateMajor = dtblReportAcademicInfo.Rows[0][4].ToString();
                reportModel.GraduateGPA = dtblReportAcademicInfo.Rows[0][5].ToString();
                reportModel.FirstGraduateMajor = dtblReportAcademicInfo.Rows[0][6].ToString();
                reportModel.SecondGraduateMajor = dtblReportAcademicInfo.Rows[0][7].ToString();
            }
            //part 2skillset info
            DataTable dtblReportSkillsetInfo = new DataTable();
            using (SqlConnection sqlCon = new SqlConnection(connectionString))
            {
                sqlCon.Open();
                string query = "SELECT Q20#2_38,Q20#2_39,Q20#2_40,Q20#2_44 FROM SurveyResponses1 where RecipientEmail =@EmailID";
                SqlDataAdapter sqlDa = new SqlDataAdapter(query, sqlCon);
                sqlDa.SelectCommand.Parameters.AddWithValue("@EmailID", emailid);
                sqlDa.Fill(dtblReportSkillsetInfo);
            }
            if (dtblReportSkillsetInfo.Rows.Count == 1)
            {
                reportModel.ManagementSkillLevel = dtblReportSkillsetInfo.Rows[0][0].ToString();
                reportModel.EntrepreneurshipLevel = dtblReportSkillsetInfo.Rows[0][1].ToString();
                reportModel.MarketingLevel = dtblReportSkillsetInfo.Rows[0][2].ToString();
                reportModel.TechnicalLevel = dtblReportSkillsetInfo.Rows[0][3].ToString();
            }
            //part 3
            //top 3 majors based on high academic performance
            DataTable dtblReportTop3HighAcademic = new DataTable();
            using (SqlConnection sqlCon = new SqlConnection(connectionString))
            {
                sqlCon.Open();
                string query = "select TOP 3 avg(Q2#13), Q2581 from SurveyResponses group by  Q2581 order by  avg(Q2#13) desc";
                SqlDataAdapter sqlDa = new SqlDataAdapter(query, sqlCon);
                sqlDa.Fill(dtblReportTop3HighAcademic);
            }
            if (dtblReportTop3HighAcademic.Rows.Count >= 1)
            {
                reportModel.Top3MajorsWithHighAcademicPerformers1 = dtblReportTop3HighAcademic.Rows[0][1].ToString();
                reportModel.Top3MajorsWithHighAcademicPerformers2 = dtblReportTop3HighAcademic.Rows[1][1].ToString();
                reportModel.Top3MajorsWithHighAcademicPerformers3 = dtblReportTop3HighAcademic.Rows[2][1].ToString();
            }
            //top 3 majors based on high career performance
            DataTable dtblReportTop3HighCareer = new DataTable();
            using (SqlConnection sqlCon = new SqlConnection(connectionString))
            {
                sqlCon.Open();
                string query = "select TOP 3 avg(Q21#9),Q2581 from SurveyResponses as SR inner join SurveyResponses1 as SR1 on SR.RecipientEmail = SR1.RecipientEmail group by  Q2581 order by  count(Q21#9) asc";
                SqlDataAdapter sqlDa = new SqlDataAdapter(query, sqlCon);
                sqlDa.Fill(dtblReportTop3HighCareer);
            }
            if (dtblReportTop3HighCareer.Rows.Count >= 1)
            {
                reportModel.Top3MajorsWithHighCareerPerformers1 = dtblReportTop3HighCareer.Rows[0][1].ToString();
                reportModel.Top3MajorsWithHighCareerPerformers2 = dtblReportTop3HighCareer.Rows[1][1].ToString();
                reportModel.Top3MajorsWithHighCareerPerformers3 = dtblReportTop3HighCareer.Rows[2][1].ToString();
            }
            //top 3 majors based on low academic performance
            DataTable dtblReportTop3LowAcademic = new DataTable();
            using (SqlConnection sqlCon = new SqlConnection(connectionString))
            {
                sqlCon.Open();
                string query = "select TOP 3 avg(Q2#13), Q2581 from SurveyResponses group by  Q2581 order by  avg(Q2#13) asc";
                SqlDataAdapter sqlDa = new SqlDataAdapter(query, sqlCon);
                sqlDa.Fill(dtblReportTop3LowAcademic);
            }
            if (dtblReportTop3LowAcademic.Rows.Count >= 1)
            {
                reportModel.Top3MajorsWithLowAcademicPerformers1 = dtblReportTop3LowAcademic.Rows[0][1].ToString();
                reportModel.Top3MajorsWithLowAcademicPerformers2 = dtblReportTop3LowAcademic.Rows[1][1].ToString();
                reportModel.Top3MajorsWithLowAcademicPerformers3 = dtblReportTop3LowAcademic.Rows[2][1].ToString();
            }
            //top 3 majors based on low career performance
            DataTable dtblReportTop3LowCareer = new DataTable();
            using (SqlConnection sqlCon = new SqlConnection(connectionString))
            {
                sqlCon.Open();
                string query = "select TOP 3 avg(Q21#9),Q2581 from SurveyResponses as SR inner join SurveyResponses1 as SR1 on SR.RecipientEmail = SR1.RecipientEmail group by  Q2581 order by  count(Q21#9) desc";
                SqlDataAdapter sqlDa = new SqlDataAdapter(query, sqlCon);
                sqlDa.Fill(dtblReportTop3LowCareer);
            }
            if (dtblReportTop3LowCareer.Rows.Count >= 1)
            {
                reportModel.Top3MajorsWithLowCareerPerformers1 = dtblReportTop3LowCareer.Rows[0][1].ToString();
                reportModel.Top3MajorsWithLowCareerPerformers2 = dtblReportTop3LowCareer.Rows[1][1].ToString();
                reportModel.Top3MajorsWithLowCareerPerformers3 = dtblReportTop3LowCareer.Rows[2][1].ToString();
            }

            //part 4
            //top 3 most enjoyable majors
            DataTable dtblReportTop3EnjoyableMajors = new DataTable();
            using (SqlConnection sqlCon = new SqlConnection(connectionString))
            {
                sqlCon.Open();
                string query = "select TOP 3 count(Q342_5),Q2581 from SurveyResponses where (Q342_5 = 'Agree' or Q342_5 ='Somewhat Agree') and (Q342_8 != 'Agree' or Q342_8 !='Somewhat Agree') group by  Q2581 order by  count(Q342_5) desc";
                SqlDataAdapter sqlDa = new SqlDataAdapter(query, sqlCon);
                sqlDa.Fill(dtblReportTop3EnjoyableMajors);
            }
            if (dtblReportTop3EnjoyableMajors.Rows.Count >= 1)
            {
                reportModel.Top3MostEnjoyableMajors1 = dtblReportTop3EnjoyableMajors.Rows[0][1].ToString();
                reportModel.Top3MostEnjoyableMajors2 = dtblReportTop3EnjoyableMajors.Rows[1][1].ToString();
                reportModel.Top3MostEnjoyableMajors3 = dtblReportTop3EnjoyableMajors.Rows[2][1].ToString();
            }
            //top 3 majors with most satisfied Employees
            DataTable dtblReportTop3SatisfiedEmployee = new DataTable();
            using (SqlConnection sqlCon = new SqlConnection(connectionString))
            {
                sqlCon.Open();
                string query = "select TOP 3 avg(Q21#9),Q2581 from SurveyResponses as SR inner join SurveyResponses1 as SR1 on SR.RecipientEmail = SR1.RecipientEmail group by  Q2581 order by  count(Q21#9) asc";
                SqlDataAdapter sqlDa = new SqlDataAdapter(query, sqlCon);
                sqlDa.Fill(dtblReportTop3HighCareer);
            }
            if (dtblReportTop3HighCareer.Rows.Count >= 1)
            {
                reportModel.Top3MajorsWithMostSatisfiedEmployees1 = dtblReportTop3HighCareer.Rows[0][1].ToString();
                reportModel.Top3MajorsWithMostSatisfiedEmployees2 = dtblReportTop3HighCareer.Rows[1][1].ToString();
                reportModel.Top3MajorsWithMostSatisfiedEmployees3 = dtblReportTop3HighCareer.Rows[2][1].ToString();
            }
            //top 3 hated majors
            DataTable dtblReportTop3HatedMajors = new DataTable();
            using (SqlConnection sqlCon = new SqlConnection(connectionString))
            {
                sqlCon.Open();
                string query = "select TOP 3 avg(Q2#13), Q2581 from SurveyResponses group by  Q2581 order by  avg(Q2#13) asc";
                SqlDataAdapter sqlDa = new SqlDataAdapter(query, sqlCon);
                sqlDa.Fill(dtblReportTop3HatedMajors);
            }
            if (dtblReportTop3LowAcademic.Rows.Count >= 1)
            {
                reportModel.Top3MostHatedMajors1 = dtblReportTop3HatedMajors.Rows[0][1].ToString();
                reportModel.Top3MostHatedMajors2 = dtblReportTop3HatedMajors.Rows[1][1].ToString();
                reportModel.Top3MostHatedMajors3 = dtblReportTop3HatedMajors.Rows[2][1].ToString();
            }
            //top 3 majors with most unsatisfied Employees
            DataTable dtblReportTop3UnsatisfiedEmployee = new DataTable();
            using (SqlConnection sqlCon = new SqlConnection(connectionString))
            {
                sqlCon.Open();
                string query = "select TOP 3 avg(Q21#9),Q2581 from SurveyResponses as SR inner join SurveyResponses1 as SR1 on SR.RecipientEmail = SR1.RecipientEmail group by  Q2581 order by  count(Q21#9) desc";
                SqlDataAdapter sqlDa = new SqlDataAdapter(query, sqlCon);
                sqlDa.Fill(dtblReportTop3UnsatisfiedEmployee);
            }
            if (dtblReportTop3UnsatisfiedEmployee.Rows.Count >= 1)
            {
                reportModel.Top3MajorsWithMostUnsatisfiedEmployees1 = dtblReportTop3UnsatisfiedEmployee.Rows[0][1].ToString();
                reportModel.Top3MajorsWithMostUnsatisfiedEmployees2 = dtblReportTop3UnsatisfiedEmployee.Rows[1][1].ToString();
                reportModel.Top3MajorsWithMostUnsatisfiedEmployees3 = dtblReportTop3UnsatisfiedEmployee.Rows[2][1].ToString();
            }
            //part4 recommended 3 majors based on personal skillsets similarities
            DataTable dtblReportTop3Recommended = new DataTable();
            using (SqlConnection sqlCon = new SqlConnection(connectionString))
            {
                sqlCon.Open();
                string query =  "select count(SR.Q2581), SR.Q2581 " +
                                "from SurveyResponses as SR " +
                                "inner join SurveyResponses1 as SR1 " +
                                "on SR.RecipientEmail = SR1.RecipientEmail " +
                                "where Q20#2_1 = (select Q20#2_1 " +
                                "from SurveyResponses1 " +
                                "where RecipientEmail  =@EmailID) " +
                                "and " +
                                "Q20#2_2 = (select Q20#2_2 " +
                                "from SurveyResponses1 " +
                                "where RecipientEmail  =@EmailID) " +
                                "and " +
                                "Q20#2_3 = (select Q20#2_3 " +
                                "from SurveyResponses1 " +
                                "where RecipientEmail  =@EmailID) " +
                                "group by SR.Q2581 " +
                                "order by count(SR.Q2581) desc ";
                SqlDataAdapter sqlDa = new SqlDataAdapter(query, sqlCon);
                sqlDa.SelectCommand.Parameters.AddWithValue("@EmailID", emailid);
                sqlDa.Fill(dtblReportTop3Recommended);
            }
            if (dtblReportTop3Recommended.Rows.Count >= 1)
            {
                reportModel.Top3RecommendedMajors1 = dtblReportTop3Recommended.Rows[0][1].ToString();
                reportModel.Top3RecommendedMajors2 = dtblReportTop3Recommended.Rows[1][1].ToString();
                reportModel.Top3RecommendedMajors3 = dtblReportTop3Recommended.Rows[2][1].ToString();
                return View(reportModel);
            }
            else
            return RedirectToAction("Survey");
        }

        //This action pulls up the survey view
        [HttpGet]
        public ActionResult Survey()
        {
            return View("Survey");
        }

        //This action pulls up the contactUs view
        [HttpGet]
        public ActionResult ContactUs()
        {
            return View();
        }

        //This method sends the message that the user wrote on the contact us page to the admin 
        [NonAction]
        public void SendContactUsEmail(string emailID, string contactUsMessage)
        {
            
            var fromEmail = new MailAddress("careeranalytics2018@gmail.com", "Testing");
            var toEmail = new MailAddress("careeranalytics2018@gmail.com", "Testing");
            var fromEmailPassword = "yujie-1276"; // Replace with actual password 

            string subject = "";
            string body = "";
            if (contactUsMessage != null)
            {
                subject = "Contact Us Email From ";
                body = contactUsMessage;
                ViewBag.Message = "Email Sucessfully Sent";
            }


            var smtp = new SmtpClient
            {
                Host = "smtp.gmail.com",
                Port = 587,
                EnableSsl = true,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(fromEmail.Address, fromEmailPassword)
            };

            using (var message = new MailMessage(fromEmail, toEmail)
            {
                Subject = subject,
                Body = body,
                IsBodyHtml = true
            })
                smtp.Send(message);
        }

        //This method sends the message that the user wrote on the contact us page to the users email
        [NonAction]
        public void SendContactUsEmailCopyToUserEmail(string emailID, string contactUsMessage)
        {

            var fromEmail = new MailAddress("careeranalytics2018@gmail.com", "Testing");
            var toEmail = new MailAddress(emailID, "Testing");
            var fromEmailPassword = "yujie-1276"; // Replace with actual password 

            string subject = "";
            string body = "";
            if (contactUsMessage != null)
            {
                subject = "Contact Us Email From ";
                body = contactUsMessage;
                ViewBag.Message = "Email Sucessfully Sent";
            }


            var smtp = new SmtpClient
            {
                Host = "smtp.gmail.com",
                Port = 587,
                EnableSsl = true,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(fromEmail.Address, fromEmailPassword)
            };

            using (var message = new MailMessage(fromEmail, toEmail)
            {
                Subject = subject,
                Body = body,
                IsBodyHtml = true
            })
                smtp.Send(message);
        }

        //This action uses the above metods to send the message from the contact page
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ContactUs(User user)
        {
            string message = "";
            

                SendContactUsEmail(user.EmailID, user.Message);
            SendContactUsEmailCopyToUserEmail(user.EmailID, user.Message);
                message = "Your Email has been sent.";
               
                
                
            return View();
        }
    }
}