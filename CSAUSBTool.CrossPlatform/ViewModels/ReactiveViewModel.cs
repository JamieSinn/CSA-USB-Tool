using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSAUSBTool.CrossPlatform.ViewModels
{
    // Instead of implementing "INotifyPropertyChanged" on our own we use "ReactiveObject" as 
    // our base class. Read more about it here: https://www.reactiveui.net
    public class ReactiveViewModel : ReactiveObject
    {
        public ReactiveViewModel()
        {
            // We can listen to any property changes with "WhenAnyValue" and do whatever we want in "Subscribe".
            this.WhenAnyValue(o => o.Name)
                .Subscribe(o => this.RaisePropertyChanged(nameof(Greeting)));
        }

        private string? _name; // This is our backing field for Name

        public string? Name
        {
            get => _name;
            set =>
                // We can use "RaiseAndSetIfChanged" to check if the value changed and automatically notify the UI
                this.RaiseAndSetIfChanged(ref _name, value);
        }

        // Greeting will change based on a Name.
        public string Greeting =>
            string.IsNullOrEmpty(Name) ?
                // If no Name is provided, use a default Greeting
                "Hello World from Avalonia.Samples" :
                // else Greet the User.
                $"Hello {Name}";

    }
}
