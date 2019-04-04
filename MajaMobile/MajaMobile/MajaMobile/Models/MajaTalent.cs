using BiExcellence.OpenBi.Api.Commands.MajaAi;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace MajaMobile.Models
{

    public class MajaTalent : IMajaTalent, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public string Id { get; }
        public string Name { get; }
        public bool IsPublic { get; }
        public string ImagePath { get; }
        public IMajaTalentCategory Category { get; }
        public string Description { get; }
        public string OrganisationId { get; }
        public IList<IMajaGrammar> ShowcaseGrammars { get; }
        private bool _selected;
        public bool Selected
        {
            get => _selected;
            set
            {
                _selected = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Selected)));
            }
        }

        public MajaTalent(IMajaTalent talent, MajaCategory category)
        {
            Id = talent.Id;
            Name = talent.Name;
            ImagePath = talent.ImagePath;
            IsPublic = talent.IsPublic;
            if (string.IsNullOrEmpty(ImagePath))
                ImagePath = "maja.png";
            Category = category;
            Description = talent.Description;
            OrganisationId = talent.OrganisationId;
            ShowcaseGrammars = talent.ShowcaseGrammars.ToList();
        }
    }

    public class MajaCategory : IComparable<MajaCategory>, IComparable, IMajaTalentCategory
    {
        public string Id { get; }
        public string Name { get; }
        public string ParentId { get; }

        public MajaCategory(IMajaTalentCategory category)
        {
            Id = category.Id;
            Name = category.Name;
            ParentId = category.ParentId;
        }

        public int CompareTo(MajaCategory other)
        {
            return Name.CompareTo(other.Name);
        }

        //Necessary for SfListView grouping
        public int CompareTo(object obj)
        {
            if (obj is MajaCategory cat)
            {
                return CompareTo(cat);
            }
            return -1;
        }
    }
}