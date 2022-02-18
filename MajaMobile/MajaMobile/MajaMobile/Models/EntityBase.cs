using BiExcellence.OpenBi.Api.Commands.Entities;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace MajaMobile.Models
{
    public abstract class EntityBase : Entity, INotifyPropertyChanged
    {
        public EntityBase(IEntity entity)
        {
            Name = entity.Name;
            Description = entity.Description;
            Id = entity.Id;
            Created = entity.Created;
            Changed = entity.Changed;
            CustomAttributes = entity.CustomAttributes;
        }

        public EntityBase() : base() { }

        public event PropertyChangedEventHandler PropertyChanged;

        internal void OnPropertyChanged([CallerMemberName] string caller = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(caller));
        }
    }
}

namespace MajaMobile.Models.Extensions
{
    public static class EntityExtensions
    {
        internal static T GetDynamicProperty<T>(this EntityBase entity, [CallerMemberName] string caller = "")
        {
            object value = null;
            if (entity.CustomAttributes.TryGetValue(caller, out value))
            {
                return (T)value;
            }
            return default(T);
        }

        internal static void SetDynamicProperty(this EntityBase entity, object value, [CallerMemberName] string caller = "")
        {
            entity.CustomAttributes[caller] = value;
            entity.OnPropertyChanged(caller);
        }
    }
}