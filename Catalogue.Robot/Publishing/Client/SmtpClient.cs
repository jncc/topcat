using System.Net.Mail;

namespace Catalogue.Robot.Publishing.Client
{
    public interface ISmtpClient
    {
        void SendEmail(string from, string to, string subject, string body);
    }

    public class SmtpClient : ISmtpClient
    {
        private readonly Env env;

        public SmtpClient(Env env)
        {
            this.env = env;
        }

        public void SendEmail(string from, string to, string subject, string body)
        {
            MailMessage mail = new MailMessage(from, to);
            using (System.Net.Mail.SmtpClient client = new System.Net.Mail.SmtpClient())
            {
                client.Port = 25;
                client.DeliveryMethod = SmtpDeliveryMethod.Network;
                client.UseDefaultCredentials = false;
                client.Host = env.SMTP_HOST;
                mail.Subject = subject;
                mail.Body = body;
                client.Send(mail);
            }
        }
    }
}
