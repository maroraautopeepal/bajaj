using Bajaj.ViewModel;

namespace Bajaj.Model
{
    public class LanguageModel : BaseViewModel
    {
        public int id { get; set; }
        public string Language { get; set; }

        private bool _is_checked;
        public bool is_checked
        {
            get => _is_checked;
            set
            {
                _is_checked = value;
                OnPropertyChanged("is_checked");
            }
        }
    }
}
