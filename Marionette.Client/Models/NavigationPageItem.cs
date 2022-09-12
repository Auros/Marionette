using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Reflection;

namespace Marionette.Models;

public class NavigationPageItem : ObservableObject
{
	private Type? _viewType;

    public string Name { get; }

	public object ViewModel { get; }

	public object View
	{
		get
		{
			if (_viewType is null)
			{
				var viewAttribute = ViewModel.GetType().GetCustomAttribute<ViewAttribute>();
				if (viewAttribute is null)
				    throw new Exception("Could not find View attribute.");
				_viewType = viewAttribute.ViewType;
			}
            return Activator.CreateInstance(_viewType)!;
		}
	}

	public NavigationPageItem(string name, object viewModel)
	{
		Name = name;
		ViewModel = viewModel;
	}
}