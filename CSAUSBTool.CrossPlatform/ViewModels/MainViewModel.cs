﻿namespace CSAUSBTool.CrossPlatform.ViewModels;

public class MainViewModel : ViewModelBase
{
    // Add our SimpleViewModel.
    // Note: We need at least a get-accessor for our Properties.
    public SimpleViewModel SimpleViewModel { get; } = new SimpleViewModel();


    // Add our ReactiveViewModel
    public ReactiveViewModel ReactiveViewModel { get; } = new ReactiveViewModel();

    public ControlSystemSoftwareGroup ControlSystemSoftwareTab { get; } = new ControlSystemSoftwareGroup();
}
