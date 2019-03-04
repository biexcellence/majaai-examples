using BiExcellence.OpenBi.Api.Commands.MajaAi;
using System;

namespace MajaMobile.Models
{

    public class MajaTalent:IMajaTalent
    {
        public string Id { get; }
        public string Name { get; }
        public bool IsPublic { get; }
        public string ImagePath { get; }
        public IMajaTalentCategory Category { get; }

        public MajaTalent(IMajaTalent talent, MajaCategory category)
        {
            Id = talent.Id;
            Name = talent.Name;
            ImagePath = talent.ImagePath;
            IsPublic = talent.IsPublic;
            if (string.IsNullOrEmpty(ImagePath))
                ImagePath = "maja.png";
            Category = category;
        }
    }

    public class MajaCategory : IComparable<MajaCategory>, IComparable, IMajaTalentCategory
    {
        public string Id { get; }
        public string Name { get; }

        public MajaCategory(IMajaTalentCategory category)
        {
            Id = category.Id;
            Name = category.Name;
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