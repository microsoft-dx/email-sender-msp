using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Hosting;
using System.Web.Http;
using MSWeekRaffle.Models;
using MSWeekRaffle.Sendgrid;
using MSWeekRaffle.Services;
using Newtonsoft.Json;

namespace MSWeekRaffle.Controllers
{
    public class EmailController : ApiController
    {
        public IEmailSender EmailSender { get; set; } = new SendgridEmailSender();

        [HttpGet]
        public async Task<string> SendEmails()
        {
            var results = new List<ResultModel>();
            var emails = File.ReadAllLines(HostingEnvironment.MapPath("~/emailsToProcess.txt"));

            foreach (var email in emails)
            {
                var message = File.ReadAllText(HostingEnvironment.MapPath("~/Templates/TemplateJunioriAcceptatiTotal.html"));

                var result = await EmailSender.SendMailAsync(new EmailData()
                {
                    FromEmailAddress = "YOUR_MSP_EMAIL",
                    FromName = "Recrutari MSP",
                    ToEmailAddress = email,
                    Subject = "Recrutări MSP UPB [pas 03]",
                    Content = message,
                });

                if (result != HttpStatusCode.Accepted)
                {
                    results.Add(new ResultModel()
                    {
                        EmailAddress = email,
                        Message = "There was a problem sending the email. Try manually !"
                    });

                    continue;
                }

                results.Add(new ResultModel()
                {
                    EmailAddress = email,
                    Message = "Success !"
                });

                await Task.Delay(100);
            }

            File.WriteAllLines(HostingEnvironment.MapPath("~/emailsToProcess.txt"), new string[] { });

            return JsonConvert.SerializeObject(results);
        }
    }
}