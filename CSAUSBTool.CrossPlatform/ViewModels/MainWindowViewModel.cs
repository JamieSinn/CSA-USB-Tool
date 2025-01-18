using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reactive;
using System.Text;
using System.Threading.Tasks;
using Avalonia.Controls;
using ReactiveUI;

namespace CSAUSBTool.CrossPlatform.ViewModels
{
    internal class MainWindowViewModel : ViewModelBase
    {
        public List<ProgramYearViewModel> ProgramYears { get; set; } = [];
        public ObservableCollection<MenuItemViewModel> Programs { get; }

        public MainWindowViewModel()
        {
            InitializeProgramLists();
            Programs = new ObservableCollection<MenuItemViewModel>(GetPrograms());

        }

        private List<MenuItemViewModel> GetPrograms()
        {
            return ProgramYears.Select(py => py.Program).Distinct().Select(program => new MenuItemViewModel()
            {
                Header = program,
                MenuItems = new ObservableCollection<MenuItemViewModel>(GetProgramYears(program))
            }).ToList();
        }

        private List<MenuItemViewModel> GetProgramYears(string program)
        {
            return ProgramYears.Where(py => py.Program == program).Select(py => new MenuItemViewModel()
            {
                Header = py.Year.ToString(),
                Command = ReactiveCommand.Create(() => HandleYearSelection(py)),
            }).ToList();
        }

        private void HandleYearSelection(ProgramYearViewModel yearViewModel)
        {
            System.Diagnostics.Debug.WriteLine($"{yearViewModel.Year} was selected with the program of {yearViewModel.Program}");
        }

        public void InitializeProgramLists()
        {
            using var client = new HttpClient();
            var years = client
                .GetStringAsync(
                    "https://raw.githubusercontent.com/JamieSinn/CSA-USB-Tool/refs/heads/main/Years.txt").Result;

            var yearList = years.Split("\n").ToList();
            yearList.ForEach(line =>
            {
                var program = line[..3];
                var year = line[3..7];
                var programYear = new ProgramYearViewModel(int.Parse(year), program);
                if (programYear.SoftwareGroups.Count > 0)
                {
                    ProgramYears.Add(programYear);
                }
            });
        }
    }
}