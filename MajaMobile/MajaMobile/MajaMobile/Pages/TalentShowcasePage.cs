using BiExcellence.OpenBi.Api.Commands.MajaAi;
using MajaMobile.Utilities;
using MajaMobile.ViewModels;
using System.Collections.Generic;

namespace MajaMobile.Pages
{
    public class TalentShowcasePage : MainPage
    {
        public TalentShowcasePage(IMajaTalent talent) : base(new TalentShowcaseViewModel(talent))
        {
            Title = "Talent Showcase";
        }
    }
}

namespace MajaMobile.ViewModels
{
    public class TalentShowcaseViewModel : MainPageViewModel
    {
        public IMajaTalent Talent { get; }

        public TalentShowcaseViewModel(IMajaTalent talent) : base(new SessionHandler(new[] { talent.Id }))
        {
            Talent = talent;
            TextSent();
        }

        protected override void TextSent()
        {
            if (!DialogActive)
            {
                foreach (var grammar in Talent.ShowcaseGrammars)
                {
                    PossibleUserReplies.Add(new ShowcaseUserReply(grammar.Phrase));
                }
            }
            UserCanChat = DialogActive;
        }

        public override void Dispose()
        {
            base.Dispose();
            SessionHandler.Dispose();
        }

        private class ShowcaseUserReply : IPossibleUserReply
        {
            public string Text { get; }

            public string Type { get; }

            public string Value { get; }

            public string ControlType { get; } = PossibleUserReplyControlType.Button;

            public IDictionary<string, object> ControlOptions { get; } = new Dictionary<string, object>();

            public ShowcaseUserReply(string text)
            {
                Value = Text = text;
            }
        }
    }
}