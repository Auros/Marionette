using CommunityToolkit.Mvvm.ComponentModel;
using Marionette.Views;
using System.Collections.ObjectModel;

namespace Marionette.ViewModels;

[View(typeof(SettingsView))]
public class SettingsViewModel : ObservableObject
{
    public string Hello { get; set; } = "Hello";

    public ObservableCollection<Test> Tests { get; set; } = new ObservableCollection<Test>()
    {
        new Test("A"),
        new Test("B"),
        new Test("C"),
    };

    public class Test
    {
        public string Name { get; }

        public Test(string name)
        {
            Name = name;
        }
    }
}