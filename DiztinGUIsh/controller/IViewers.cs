﻿using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Diz.Core.util;

namespace DiztinGUIsh.controller
{
    public interface IViewer
    {
        
    }

    public interface IFormViewer : IViewer, ICloseHandler
    {
        public DialogResult ShowDialog();
        void Show();
        void Close();
        bool IsDisposed { get; }
    }

    public interface IBytesGridViewer<TByteItem> : IViewer
    {
        // get the number base that will be used to display certain items in the grid
        public Util.NumberBase NumberBaseToShow { get; }
        TByteItem SelectedByteOffset { get; }
        public List<TByteItem> DataSource { get; set; }
        int TargetNumberOfRowsToShow { get; }

        void SelectRow(int row);

        void BeginEditingSelectionComment();
        void BeginEditingSelectionLabel();
        
        public class SelectedOffsetChangedEventArgs : EventArgs
        {
            public TByteItem Row { get; init; }
            public int RowIndex { get; init; }
        }

        public delegate void SelectedOffsetChange(object sender, SelectedOffsetChangedEventArgs e);

        public event SelectedOffsetChange SelectedOffsetChanged;
    }
    
    public interface ILabelEditorView
    {
        string PromptForCsvFilename();
        void RepopulateFromData();
        void ShowLineItemError(string exMessage, int errLine);
    }
}