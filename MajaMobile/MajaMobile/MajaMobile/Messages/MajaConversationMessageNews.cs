using BiExcellence.OpenBi.Api.Commands.MajaAi;
using System.Collections.Generic;

namespace MajaMobile.Messages
{
    public class MajaConversationMessageNews : MajaConversationMessage
    {
        private IList<News> _news;
        public IList<News> News
        {
            get
            {
                if (_news == null && !string.IsNullOrEmpty(MajaQueryAnswer.Data))
                    _news = (IList<News>)MajaQueryAnswer.DeserializeData();
                return _news;
            }
        }

        public MajaConversationMessageNews(IMajaQueryAnswer queryAnswer) : base(queryAnswer)
        {

        }
    }
}