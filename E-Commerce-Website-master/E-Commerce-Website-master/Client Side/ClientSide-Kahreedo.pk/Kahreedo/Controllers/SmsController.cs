using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Twilio;
using Twilio.AspNet.Mvc;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;

namespace Khareedo.Controllers
{
    public class SmsController : TwilioController
    {
        //
        // GET: /Sms/
        public void SendSMS()
        {

            var accountSid = ConfigurationManager.AppSettings["TwilioAccountSid"];
            var authToken = ConfigurationManager.AppSettings["TwilioAuthToken"];
            TwilioClient.Init(accountSid, authToken);

            var to = new PhoneNumber(ConfigurationManager.AppSettings["myPhone"]);
            var from = new PhoneNumber("+12017332120");

            var msg = MessageResource.Create(to: to, from: from, body: "Test Message");

            var res = msg.Sid;
        }
	}
}