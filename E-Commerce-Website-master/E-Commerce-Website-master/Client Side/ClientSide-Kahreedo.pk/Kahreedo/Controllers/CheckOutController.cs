using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Khareedo.Models;
using System.Data;
using Nexmo.Api;
using System.Net;
using Twilio;
using Twilio.AspNet.Mvc;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;
using System.Configuration;

namespace Khareedo.Controllers
{
    public class CheckOutController : Controller
    {
        KhareedoEntities db = new KhareedoEntities();
        // GET: CheckOut
        public ActionResult Index()
        {
            ViewBag.PayMethod = new SelectList(db.PaymentTypes, "PayTypeID", "TypeName");
            
               
            var data=this.GetDefaultData();
         
            return View(data);
        }

        
        //PLACE ORDER--LAST STEP
        public ActionResult PlaceOrder(FormCollection getCheckoutDetails)
        {

            try
            {
                int shpID = 1;
                if (db.ShippingDetails.Count() > 0)
                {
                    shpID = db.ShippingDetails.Max(x => x.ShippingID) + 1;
                }
                int payID = 1;
                if (db.Payments.Count() > 0)
                {
                    payID = db.Payments.Max(x => x.PaymentID) + 1;
                }
                int orderID = 1;
                if (db.Orders.Count() > 0)
                {
                    orderID = db.Orders.Max(x => x.OrderID) + 1;
                }



                ShippingDetail shpDetails = new ShippingDetail();
                shpDetails.ShippingID = shpID;
                shpDetails.FirstName = getCheckoutDetails["FirstName"];
                shpDetails.LastName = getCheckoutDetails["LastName"];
                shpDetails.Email = getCheckoutDetails["Email"];
                shpDetails.Mobile = getCheckoutDetails["Mobile"];
                shpDetails.Address = getCheckoutDetails["Address"];
                shpDetails.Province = getCheckoutDetails["Province"];
                shpDetails.City = getCheckoutDetails["City"];
                shpDetails.PostCode = getCheckoutDetails["PostCode"];
                db.ShippingDetails.Add(shpDetails);
                db.SaveChanges();

                Payment pay = new Payment();
                pay.PaymentID = payID;
                pay.Type = Convert.ToInt32(getCheckoutDetails["PayMethod"]);
                pay.PaymentDateTime = DateTime.UtcNow;
                db.Payments.Add(pay);
                db.SaveChanges();

                Order o = new Order();
                o.OrderID = orderID;
                o.CustomerID = TempShpData.UserID;
                o.PaymentID = payID;
                o.ShippingID = shpID;
                o.Discount = Convert.ToInt32(getCheckoutDetails["discount"]);
                o.TotalAmount = Convert.ToInt32(getCheckoutDetails["totalAmount"]);
                o.isCompleted = true;
                o.OrderDate = DateTime.Now;
                db.Orders.Add(o);
                db.SaveChanges();

                foreach (var OD in TempShpData.items)
                {
                    OD.OrderID = orderID;
                    OD.Order = db.Orders.Find(orderID);
                    OD.Product = db.Products.Find(OD.ProductID);
                    db.OrderDetails.Add(OD);
                    db.SaveChanges();
                }


                //SMS - Costing 0.001$/SMS 69 Paisa
                //ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                //sendSMS(orderID);





                return RedirectToAction("Index", "ThankYou");
            }
            catch (Exception ex)
            {
                
                throw ex;
            }
            
        }

        public static void sendSMS(int OrderId)
        {

            try
            {
                var accountSid = ConfigurationManager.AppSettings["TwilioAccountSid"];
                var authToken = ConfigurationManager.AppSettings["TwilioAuthToken"];
                TwilioClient.Init(accountSid, authToken);

                var to = new PhoneNumber(ConfigurationManager.AppSettings["myPhone"]);
                var from = new PhoneNumber("+12017332120");

                var msg = MessageResource.Create(to: to, from: from, body: String.Format("An order with OrderID:{0} has been placed successfully", OrderId));

                var res = msg.Sid;
            }
            catch (Exception ex)
            {
                
                throw ex;
            }


        }

    }
}