﻿using System.Reactive.Disposables;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using Diz.Gui.Avalonia.ViewModels;
using ReactiveUI;

namespace Diz.Gui.Avalonia.Views.UserControls
{
    public class MainGridUserControl : ReactiveUserControl<ByteEntriesViewModel>
    {
        public DataGrid MainGrid => this.FindControl<DataGrid>("MainGrid");
        
        public MainGridUserControl()
        {
            ViewModel = new ByteEntriesViewModel();

            this.WhenActivated(disposables =>
            {
                this.OneWayBind(ViewModel,
                    viewmodel => viewmodel.ByteEntries,
                    view => view.MainGrid.Items
                ).DisposeWith(disposables);
            });

            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}