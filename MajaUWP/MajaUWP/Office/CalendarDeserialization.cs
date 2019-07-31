using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MajaUWP.Office
{
    //Deserialization Stuff
    public class CalendarEvent
    {
        public string originalStartTimeZone { get; set; }
        public string originalEndTimeZone { get; set; }
        public Responsestatus responseStatus { get; set; }
        public string iCalUId { get; set; }
        public int reminderMinutesBeforeStart { get; set; }
        public bool isReminderOn { get; set; }
        public bool hasAttachments { get; set; }
        public string subject { get; set; }
        public Body body { get; set; }
        public string bodyPreview { get; set; }
        public string importance { get; set; }
        public string sensitivity { get; set; }
        public Start start { get; set; }
        public End end { get; set; }
        public Location location { get; set; }
        public Location1[] locations { get; set; }
        public bool isAllDay { get; set; }
        public bool isCancelled { get; set; }
        public bool isOrganizer { get; set; }
        public bool responseRequested { get; set; }
        public string showAs { get; set; }
        public string type { get; set; }
        public Attendee[] attendees { get; set; }
        public Organizer organizer { get; set; }
        public string webLink { get; set; }
        public DateTime createdDateTime { get; set; }
        public DateTime lastModifiedDateTime { get; set; }
        public string changeKey { get; set; }
        public object[] categories { get; set; }
        public string id { get; set; }
        public string odataetag { get; set; }
    }

    public class Responsestatus
    {
        public string response { get; set; }
        public DateTime time { get; set; }
    }

    public class Body
    {
        public string contentType { get; set; }
        public string content { get; set; }
    }

    public class Start
    {
        public DateTime dateTime { get; set; }
        public string timeZone { get; set; }
    }

    public class End
    {
        public DateTime dateTime { get; set; }
        public string timeZone { get; set; }
    }

    public class Location
    {
        public string displayName { get; set; }
        public string locationType { get; set; }
        public string uniqueId { get; set; }
        public string uniqueIdType { get; set; }
    }

    public class Organizer
    {
        public Emailaddress emailAddress { get; set; }
    }

    public class Emailaddress
    {
        public string name { get; set; }
        public string address { get; set; }
    }

    public class Location1
    {
        public string displayName { get; set; }
        public string locationType { get; set; }
        public string uniqueId { get; set; }
        public string uniqueIdType { get; set; }
    }

    public class Attendee
    {
        public Status status { get; set; }
        public string type { get; set; }
        public Emailaddress1 emailAddress { get; set; }
    }

    public class Status
    {
        public string response { get; set; }
        public DateTime time { get; set; }
    }

    public class Emailaddress1
    {
        public string name { get; set; }
        public string address { get; set; }
    }

    
    class CalendarDeserialization
    {
        public static CalendarEvent DeserializeCalendarEvent(string json) {
            try
            {
                return JsonConvert.DeserializeObject<CalendarEvent>(json);
            }
            catch (Exception)
            {

                return null;
            }


        }
    }
}
