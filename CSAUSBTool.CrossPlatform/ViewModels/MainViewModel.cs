using System.Collections.Generic;

namespace CSAUSBTool.CrossPlatform.ViewModels;

public class MainViewModel : ViewModelBase
{
    // Add our SimpleViewModel.
    // Note: We need at least a get-accessor for our Properties.
    public ProgramYear ProgramYear { get; } = new ProgramYear(null, null);
}
