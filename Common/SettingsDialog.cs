﻿/* SettingsDialog.cs - Implementation of SettingsDialog class, which allows the user to view and update configuration settings.
 * Note that this file is shared across applications.
 *
 * Copyright (c) 2017-20 Jeffrey Paul Bourdier
 *
 * Licensed under the MIT License.  This file may be used only in compliance with this License.
 * Software distributed under this License is provided "AS IS", WITHOUT WARRANTY OF ANY KIND.
 * For more information, see the accompanying License file or the following URL:
 *
 *   https://opensource.org/licenses/MIT
 */


/* OpenFileDialog */
using Microsoft.Win32;

/* Exception */
using System;

/* List */
using System.Collections.Generic;

/* CancelEventArgs */
using System.ComponentModel;

/* StreamWriter */
using System.IO;

/* ResizeMode, RoutedEventArgs, SizeToContent, Thickness, UIElement, Window, WindowStyle */
using System.Windows;

/* Button, CheckBox, Dock, DockPanel, GroupBox, Label, StackPanel, TextBox, TextChangedEventArgs */
using System.Windows.Controls;


namespace JeffBourdier
{
    /// <summary>Represents a dialog that allows the user to view and update configuration settings.</summary>
    public class SettingsDialog : StandardDialog
    {
        /****************
         * Constructors *
         ****************/

        #region Public Constructors

        /// <summary>Initializes a new instance of the SettingsDialog class.</summary>
        /// <param name="uiElements">If non-null, a list of UI elements to add.</param>
        /// <param name="initialElement">
        /// If non-null, the element to which logical focus is given when the dialog is shown.
        /// </param>
        /// <param name="initialTabIndex">Initial value for the tab index following the UI elements to add.</param>
        public SettingsDialog(List<UIElement> uiElements, IInputElement initialElement, int initialTabIndex)
        {
            /* Process the list of UI elements (if applicable). */
            MarginType marginType;
            if (uiElements == null)
            {
                marginType = MarginType.Top;
                this.ElementCount = 0;
            }
            else
            {
                marginType = MarginType.Standard;
                this.ElementCount = uiElements.Count;
                foreach (UIElement element in uiElements) this.AddUIElement(element);
            }

            /* Initialize the "Write messages to log file" check box. */
            int i = initialTabIndex;
            this.LogCheckBox = UI.CreateCheckBox(++i, marginType, Common.Resources.WriteToLog, Common.Settings.Default.Log);
            this.InitialElement = (initialElement == null) ? this.LogCheckBox : initialElement;

            /* Initialize the "Log file path" label. */
            this.LogPathLabel = UI.CreateLabel(MarginType.Standard, Common.Resources.LogFilePath, true);

            /* Initialize the log path text box. */
            this.LogPathTextBox = new TextBox();
            this.LogPathTextBox.TabIndex = ++i;
            this.LogPathTextBox.Margin = new Thickness(0, 0, UI.HalfSpace, 0);
            this.LogPathTextBox.Text = Common.Settings.Default.LogPath;
            this.LogPathTextBox.GotFocus += UI.TextBox_GotFocus;
            this.LogPathTextBox.TextChanged += this.LogPathTextBox_TextChanged;
            this.LogPathTextBox.LostFocus += this.LogPathTextBox_LostFocus;
            this.LogPathLabel.Target = this.LogPathTextBox;

            /* Initialize the browse button. */
            Button button = new Button();
            button.TabIndex = ++i;
            button.Margin = new Thickness(UI.HalfSpace, 0, 0, 0);
            button.Content = "...";
            button.Width = 20;
            button.Click += this.BrowseButton_Click;

            /* Build out the browse (dock) panel, which contains the file/
             * path and browse controls, and add it to the stack panel.
             */
            DockPanel.SetDock(button, Dock.Right);
            this.BrowsePanel = new DockPanel();
            this.BrowsePanel.Children.Add(button);
            this.BrowsePanel.Children.Add(this.LogPathTextBox);
            this.BrowsePanel.Margin = new Thickness(UI.TripleSpace, UI.HalfSpace, UI.TripleSpace, UI.UnitSpace);

            /* Initialize the option check boxes. */
            this.AllCheckBox = UI.CreateCheckBox(++i, MarginType.Top, Common.Resources.All, null);
            this.TimestampsCheckBox = UI.CreateCheckBox(++i, MarginType.Standard,
                Common.Resources.Timestamps, Common.Settings.Default.LogTimestamp);
            this.ProcedureNamesCheckBox = UI.CreateCheckBox(++i, MarginType.Standard,
                Common.Resources.ProcedureNames, Common.Settings.Default.LogProcedureName);
            this.IndentsCheckBox = UI.CreateCheckBox(++i, MarginType.Standard,
                Common.Resources.Indents, Common.Settings.Default.LogIndent);
            this.ExceptionDetailCheckBox = UI.CreateCheckBox(++i, MarginType.Standard,
                Common.Resources.ExceptionDetail, Common.Settings.Default.LogExceptionDetail);

            /* Build out the options stack panel, which contains the
             * option check boxes and will serve as the group box content.
             */
            StackPanel stackPanel = new StackPanel();
            stackPanel.Children.Add(this.AllCheckBox);
            stackPanel.Children.Add(this.TimestampsCheckBox);
            stackPanel.Children.Add(this.ProcedureNamesCheckBox);
            stackPanel.Children.Add(this.IndentsCheckBox);
            stackPanel.Children.Add(this.ExceptionDetailCheckBox);

            /* Build out the group box, which contains the options stack panel. */
            this.OptionsGroupBox = new GroupBox();
            this.OptionsGroupBox.Header = Common.Resources.LoggingOptions;
            this.OptionsGroupBox.Content = stackPanel;
            this.OptionsGroupBox.Margin = new Thickness(UI.TripleSpace, UI.UnitSpace, UI.TripleSpace, UI.UnitSpace);

            /* Build out the window and its content. */
            this.AddUIElement(this.LogCheckBox);
            this.AddUIElement(this.LogPathLabel);
            this.AddUIElement(this.BrowsePanel);
            this.AddUIElement(this.OptionsGroupBox);
            this.BuildOut(UI.ClientWidth, Common.Resources.Settings);
            this.Closing += this.SettingsDialog_Closing;

            /* Since the Checked handlers refer to the other controls, they are not assigned (and should not be
             * called) until all controls have been instantiated (in order to avoid null reference exceptions).
             */
            this.LogCheckBox.Checked += this.LogCheckBox_Checked;
            this.LogCheckBox.Unchecked += this.LogCheckBox_Checked;
            this.AllCheckBox.Checked += this.AllCheckBox_Checked;
            this.AllCheckBox.Unchecked += this.AllCheckBox_Checked;
            this.TimestampsCheckBox.Checked += this.OptionCheckBox_Checked;
            this.TimestampsCheckBox.Unchecked += this.OptionCheckBox_Checked;
            this.ProcedureNamesCheckBox.Checked += this.OptionCheckBox_Checked;
            this.ProcedureNamesCheckBox.Unchecked += this.OptionCheckBox_Checked;
            this.IndentsCheckBox.Checked += this.OptionCheckBox_Checked;
            this.IndentsCheckBox.Unchecked += this.OptionCheckBox_Checked;
            this.ExceptionDetailCheckBox.Checked += this.OptionCheckBox_Checked;
            this.ExceptionDetailCheckBox.Unchecked += this.OptionCheckBox_Checked;

            /* Now that it's safe, call the Checked handler for the "Write
             * messages to log file" and option check boxes to set visual cues.
             */
            this.LogCheckBox_Checked(null, null);
            this.OptionCheckBox_Checked(null, null);
        }

        #endregion

        /**********
         * Fields *
         **********/

        #region Private Fields

        private CheckBox LogCheckBox;
        private Label LogPathLabel;
        private TextBox LogPathTextBox;
        private DockPanel BrowsePanel;
        private CheckBox AllCheckBox;
        private CheckBox TimestampsCheckBox;
        private CheckBox ProcedureNamesCheckBox;
        private CheckBox IndentsCheckBox;
        private CheckBox ExceptionDetailCheckBox;
        private GroupBox OptionsGroupBox;
        private bool ItemCheckInProgress;
        private int ElementCount;

        #endregion

        /**************
         * Properties *
         **************/

        #region Public Properties

        /// <summary>Gets whether or not to write messages to the log file.</summary>
        public bool Log { get { return (bool)this.LogCheckBox.IsChecked; } }

        /// <summary>Gets a string containing the full path of the log file.</summary>
        public string LogPath { get { return this.LogPathTextBox.Text; } }

        /// <summary>Gets whether or not to write timestamps to the log file.</summary>
        public bool LogTimestamps { get { return (bool)this.TimestampsCheckBox.IsChecked; } }

        /// <summary>Gets whether or not to write procedure names to the log file.</summary>
        public bool LogProcedureNames { get { return (bool)this.ProcedureNamesCheckBox.IsChecked; } }

        /// <summary>Gets whether or not to indent messages (based on call stack) when writing to the log file.</summary>
        public bool LogIndents { get { return (bool)this.IndentsCheckBox.IsChecked; } }

        /// <summary>Gets whether or not to write exception detail to the log file.</summary>
        public bool LogExceptionDetail { get { return (bool)this.ExceptionDetailCheckBox.IsChecked; } }

        #endregion

        /***********
         * Methods *
         ***********/

        #region Protected Methods

        protected override bool CheckRequiredInput()
        {
            /* If the "Write messages to log file" box is checked, a log path is required. */
            return (!this.Log || !string.IsNullOrEmpty(this.LogPath));
        }

        #endregion

        #region Private Methods

        #region Event Handlers

        private void LogCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            this.ValidateLogFile();

            /* Enable or disable other controls based on whether or not the "Write messages to log file" box is checked. */
            bool b = this.Log;
            this.LogPathLabel.IsEnabled = b;
            this.BrowsePanel.IsEnabled = b;
            this.OptionsGroupBox.IsEnabled = b;
            this.EnableOkButton();
        }

        private void LogPathTextBox_TextChanged(object sender, TextChangedEventArgs e) { this.EnableOkButton(); }

        private void LogPathTextBox_LostFocus(object sender, RoutedEventArgs e) { this.ValidateLogFile(); }

        private void BrowseButton_Click(object sender, RoutedEventArgs e)
        {
            /* Prompt the user for a file whose path to copy into the log path text box. */
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = Common.Resources.LogFiles;
            bool? result = dialog.ShowDialog(this);
            if (result != true) return;
            this.LogPathTextBox.Text = dialog.FileName;
            this.ValidateLogFile();
        }

        private void AllCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            /* This prevents endless loops when boxes are checked programmatically. */
            if (this.ItemCheckInProgress) return;
            this.ItemCheckInProgress = true;

            /* Check or uncheck all options. */
            bool b = (bool)this.AllCheckBox.IsChecked;
            this.TimestampsCheckBox.IsChecked = b;
            this.ProcedureNamesCheckBox.IsChecked = b;
            this.IndentsCheckBox.IsChecked = b;
            this.ExceptionDetailCheckBox.IsChecked = b;
            this.ItemCheckInProgress = false;
        }

        private void OptionCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            /* This prevents endless loops when boxes are checked programmatically. */
            if (this.ItemCheckInProgress) return;
            this.ItemCheckInProgress = true;

            /* If all options are checked, check the "All" box; otherwise, uncheck it. */
            this.AllCheckBox.IsChecked = this.AreAllOptionsChecked();
            this.ItemCheckInProgress = false;
        }

        /* Remove any extra UI elements that were added. */
        private void SettingsDialog_Closing(object sender, CancelEventArgs e)
        { for (int i = 0; i < this.ElementCount; ++i) this.RemoveUIElement(0); }

        #endregion

        /// <summary>Determines whether or not all logging options are checked.</summary>
        /// <returns>True if all logging options are checked; otherwise, false.</returns>
        private bool AreAllOptionsChecked()
        {
            /* If any one of the options is not checked, return false. */
            if (this.TimestampsCheckBox.IsChecked != true) return false;
            if (this.ProcedureNamesCheckBox.IsChecked != true) return false;
            if (this.IndentsCheckBox.IsChecked != true) return false;
            if (this.ExceptionDetailCheckBox.IsChecked != true) return false;

            /* We made it all the way through, so they must all be checked. */
            return true;
        }

        /* If applicable, test the log file to make sure it can be written to. */
        private void ValidateLogFile()
        {
            if (!this.Log || string.IsNullOrEmpty(this.LogPath)) return;
            try { using (StreamWriter writer = new StreamWriter(this.LogPath, true)) { } }
            catch (Exception ex)
            {
                string s = Text.FormatErrorMessage(Common.Resources.LogNotOpen, ex);
                MessageBox.Show(this, s, Meta.Name, MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        #endregion
    }
}
