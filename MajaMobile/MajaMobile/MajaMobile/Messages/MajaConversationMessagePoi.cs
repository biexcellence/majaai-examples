using BiExcellence.OpenBi.Api.Commands.MajaAi;
using MajaMobile.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using Xamarin.Forms;

namespace MajaMobile.Messages
{
    public class MajaConversationMessagePoi : MajaConversationMessage
    {
        public ICommand PoisTappedCommand { get; }

        public List<PointOfInterest> Pois { get; } = new List<PointOfInterest>();

        public string LinkText => $"{Pois.Count} Sehenswürdigkeiten gefunden";

        public MajaConversationMessagePoi(IMajaQueryAnswer queryAnswer) : base(queryAnswer)
        {
            PoisTappedCommand = new Command(() => MessagingCenter.Send(this, ConversationMessageTappedMessage));
            foreach (var entity in queryAnswer.Entities.Where(e => string.Equals(e.EntityProvider, "poi", StringComparison.OrdinalIgnoreCase)))
            {
                Pois.Add(new PointOfInterest(entity));
            }
        }
    }
}