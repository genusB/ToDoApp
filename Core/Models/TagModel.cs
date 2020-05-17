using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Core.Models
{
    public class TagModel : INotifyPropertyChanged
    {
        private int TagId;

        public int Id
        {
            get => TagId;
            set
            {
                if (TagId == value) return;

                TagId = value;
                OnPropertyChanged();
            }
        }

        private string TagText;

        public string Text
        {
            get => TagText;
            set
            {
                if (TagText == value) return;

                TagText = value;
                OnPropertyChanged();
            }
        }

        public int? ProjectId { get; set; }
        public string TagTextColor { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private bool Equals(TagModel other)
        {
            return Id == other.Id && string.Equals(Text, other.Text);
        }

        public override bool Equals(object obj)
        {
            return ReferenceEquals(this, obj) || obj is TagModel other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (Id.GetHashCode() * 397) ^ (Text != null ? Text.GetHashCode() : 0);
            }
        }
    }
}