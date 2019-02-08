using BiExcellence.OpenBi.Api.Commands.MajaAi;
using System.Collections.Generic;

namespace MajaMobile.Messages
{
    public class MajaConversationMessageImmo : MajaConversationMessage
    {
        private IList<RealEstateObject> _immos;
        public IList<RealEstateObject> Immos
        {
            get
            {
                if (_immos == null && !string.IsNullOrEmpty(MajaQueryAnswer.Data))
                    _immos = (IList<RealEstateObject>)MajaQueryAnswer.DeserializeData();
                return _immos;
            }
        }

        public MajaConversationMessageImmo(IMajaQueryAnswer queryAnswer) : base(queryAnswer)
        {

        }
    }
}