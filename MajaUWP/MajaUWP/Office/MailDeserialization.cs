using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MajaUWP.Office
{

    public class Mail

    {
        public DateTime receivedDateTime { get; set; }
        public DateTime sentDateTime { get; set; }
        public bool hasAttachments { get; set; }
        public string internetMessageId { get; set; }
        public string subject { get; set; }
        public Body body { get; set; }
        public string bodyPreview { get; set; }
        public string importance { get; set; }
        public string parentFolderId { get; set; }
        public Sender sender { get; set; }
        public From from { get; set; }
        public Torecipient[] toRecipients { get; set; }
        public object[] ccRecipients { get; set; }
        public object[] bccRecipients { get; set; }
        public object[] replyTo { get; set; }
        public string conversationId { get; set; }
        public bool isDeliveryReceiptRequested { get; set; }
        public bool isReadReceiptRequested { get; set; }
        public bool isRead { get; set; }
        public bool isDraft { get; set; }
        public string webLink { get; set; }
        public string inferenceClassification { get; set; }
        public Flag flag { get; set; }
        public DateTime createdDateTime { get; set; }
        public DateTime lastModifiedDateTime { get; set; }
        public string changeKey { get; set; }
        public object[] categories { get; set; }
        public string id { get; set; }
        public string odataetag { get; set; }
    }
  
    public class Sender
    {
        public Emailaddress emailAddress { get; set; }
    }

    public class From
    {
        public Emailaddress1 emailAddress { get; set; }
    }

    public class Flag
    {
        public string flagStatus { get; set; }
    }

    public class Torecipient
    {
        public Emailaddress2 emailAddress { get; set; }
    }

    public class Emailaddress2
    {
        public string name { get; set; }
        public string address { get; set; }
    }

    class MailDeserialization
    {
        public static Mail DeserializeMail(string json)
        {
            try
            {
                return JsonConvert.DeserializeObject<Mail>(json);
            }
            catch (Exception)
            {

                return null;
            }


        }
    }
}
