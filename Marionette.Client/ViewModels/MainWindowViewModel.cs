using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Marionette.Models;
using Marionette.Views;
using System.Collections.ObjectModel;

namespace Marionette.ViewModels;

[View(typeof(MainWindow))]
public partial class MainWindowViewModel : ObservableObject
{
    [ObservableProperty]
    private string _hello = "Hello!";

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CoolText))]
    [NotifyPropertyChangedFor(nameof(ActiveView))]
    [NotifyPropertyChangedFor(nameof(ActiveViewModel))]
    private NavigationPageItem? _activePage;

    public object? ActiveView => _activePage?.View;

    public object? ActiveViewModel => _activePage?.ViewModel;

    public string CoolText => _activePage?.View?.GetType().FullName ?? "Not set";

    public ObservableCollection<NavigationPageItem> NavigationLocations { get; } = new();

    public MainWindowViewModel() { }

    public MainWindowViewModel(InfoViewModel infoViewModel, SettingsViewModel settingsViewModel)
    {
        NavigationLocations.Add(new NavigationPageItem("Info", infoViewModel));
        NavigationLocations.Add(new NavigationPageItem("Settings", settingsViewModel));
    }

    [RelayCommand]
    public void Test()
    {
        Hello = "Hi!";
    }
}