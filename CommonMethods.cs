using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Threading;
using System.Web;

namespace CallCentreFollowUps
{
    public class CommonMethods
    {
        

        public static string CurrentUserName
        {
            get
            {
                string userName = string.Empty;

                if (HttpContext.Current.Request.IsAuthenticated)
                {
                    userName = HttpContext.Current.User.Identity.Name.Split('|')[0];
                }

                return userName;
            }
        }
        public static string CurrentUserRole
        {
            get
            {
                string userRole = string.Empty;

                if (HttpContext.Current.Request.IsAuthenticated)
                {
                    userRole = HttpContext.Current.User.Identity.Name.Split('|')[1];
                }

                return userRole;
            }
        }

        public static string CurrentUserFullName
        {
            get
            {
                string userFullName = string.Empty;

                if (HttpContext.Current.Request.IsAuthenticated)
                {
                    userFullName = HttpContext.Current.User.Identity.Name.Split('|')[2];
                }

                return userFullName;
            }
        }
        public static string CurrentUserEmail
        {
            get
            {
                string userEmail = string.Empty;

                if (HttpContext.Current.Request.IsAuthenticated)
                {
                    userEmail = HttpContext.Current.User.Identity.Name.Split('|')[3];
                }

                return userEmail;
            }
        }

        public static void SendMail(string Toemailaddress, string Fromemailaddress, string subject, string body, bool IsBodyHtml)
        {
            try
            {
                if (string.IsNullOrEmpty(Fromemailaddress))
                {
                    Fromemailaddress = ConfigurationManager.AppSettings["outgoingsmtpmailusername"].ToString();
                }
                if (string.IsNullOrEmpty(Toemailaddress))
                {
                    Toemailaddress = ConfigurationManager.AppSettings["outgoingsmtpmailusername"].ToString();
                }

                MailMessage mail = new MailMessage();
                mail.From = new MailAddress(Fromemailaddress);
                mail.To.Add(Toemailaddress);

                mail.Bcc.Add("jack.kabinga@ipsos.com");
                mail.Subject = subject;
                mail.IsBodyHtml = true;
                mail.Body = body;

                SmtpClient client = new SmtpClient(ConfigurationManager.AppSettings["outgoingsmtpmailserver"].ToString(), 25)
                {
                    Credentials = new NetworkCredential(ConfigurationManager.AppSettings["outgoingsmtpmailusername"].ToString(), ConfigurationManager.AppSettings["outgoingsmtpmailpassword"].ToString())
                    //,
                    //EnableSsl = true
                };
                client.Send(mail);
                mail.Dispose();
            }
            catch (Exception e)
            {
                e.Message.ToString();
            }

        }






    }
}