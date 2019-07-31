using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MajaUWP.Office
{







    public class SimpleContact
    {
        public string displayName { get; set; }
        public string id { get; set; }
        public string mailAdress { get; set; }
    }




    class ContactsDeserialization
    {
        public static SimpleContact[] DeserializeContacts(string json)
        {
            try
            {
                return JsonConvert.DeserializeObject<SimpleContact[]>(json, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore, MissingMemberHandling = MissingMemberHandling.Ignore });
            }
            catch (Exception)
            {

                return null;
            }


        }
    }
}
