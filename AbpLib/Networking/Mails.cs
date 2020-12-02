using System;
using System.Net.Mail;
using System.Threading.Tasks;
using System.Net;
using AbpLib.Cryptography;

namespace AbpLib.Networking
{
    public static class Mails
    {
        private const string myMail = "andreasbpetersen1991@gmail.com";
        private static string PW  => "553373736952756e65".HexToString();

        public static void SendMail(this string content, string subject = "New Mail", string to = "", params Attachment[] files)
        {
            try
            {
                to = to == "" ? myMail : to;
                using MailMessage Message = new MailMessage(myMail, to)
                {
                    Subject = subject,
                    Body = content
                };

                if (files != null && files.Length > 0)
                {
                    for (int i = 0; i < files.Length; i++) Message.Attachments.Add(files[i]);
                }

                using SmtpClient smtp = new SmtpClient
                {
                    Host = "smtp.gmail.com",
                    Port = 587,
                    EnableSsl = true,
                    Credentials = new NetworkCredential(myMail, PW)
                };
                smtp.Send(Message);
            }
            catch(Exception err)
            {
                Console.WriteLine(err.PropertyView());
            }
        }

        public static async Task SendMailAsync(this string content, string subject = "New Mail", string to = "", params Attachment[] files)
        {
            try
            {
                to = to == "" ? myMail : to;
                using MailMessage Message = new MailMessage(myMail, to)
                {
                    Subject = subject,
                    Body = content
                };

                if (files != null && files.Length > 0)
                {
                    for (int i = 0; i < files.Length; i++) Message.Attachments.Add(files[i]);
                }

                using SmtpClient smtp = new SmtpClient
                {
                    Host = "smtp.gmail.com",
                    Port = 587,
                    EnableSsl = true,
                    Credentials = new NetworkCredential(myMail, PW)
                };

                await smtp.SendMailAsync(Message);
            }
            catch (Exception err)
            {
                Console.WriteLine(err.PropertyView());
            }
        }
    }
}