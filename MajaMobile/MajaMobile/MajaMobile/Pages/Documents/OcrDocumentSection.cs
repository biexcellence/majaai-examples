using BiExcellence.OpenBi.Api.Commands.Entities;
using MajaMobile.Models;
using MajaMobile.Models.Extensions;

namespace MajaMobile.Pages.Documents
{
    public class OcrDocumentSection : EntityBase
    {

        public string BaseEntity
        {
            get => this.GetDynamicProperty<string>();
            set => this.SetDynamicProperty(value);
        }

        public string EntityId
        {
            get => this.GetDynamicProperty<string>();
            set => this.SetDynamicProperty(value);
        }

        public string OcrDocument
        {
            get => this.GetDynamicProperty<string>();
            set => this.SetDynamicProperty(value);
        }

        public OcrDocumentSection(IEntity entity) : base(entity)
        {

        }

    }
}