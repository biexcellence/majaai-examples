using BiExcellence.OpenBi.Api.Commands.Entities;
using MajaMobile.Models;
using MajaMobile.Models.Extensions;
using System.Collections.Generic;
using System.Linq;

namespace MajaMobile.Pages.Documents
{
    public class OcrDocument : EntityBase
    {
        private bool _selected;
        public bool Selected
        {
            get => _selected;
            set
            {
                _selected =value;
                OnPropertyChanged();
            }
        }

        private IList<string> _tags;
        public IList<string> Tags
        {
            get
            {
                if (_tags == null)
                {
                    _tags = this.GetDynamicProperty<IEnumerable<object>>()?.Cast<string>().ToList() ?? new List<string>(0);
                }
                return _tags;
            }
            set
            {
                _tags = value;
                this.SetDynamicProperty(new List<object>(value.Cast<object>()));
            }
        }

        public string OcrDocumentType
        {
            get => this.GetDynamicProperty<string>();
            set => this.SetDynamicProperty(value);
        }

        public OcrDocument(IEntity entity) : base(entity)
        {

        }

    }
}