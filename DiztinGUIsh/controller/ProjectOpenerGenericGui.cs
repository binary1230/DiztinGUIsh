﻿using System.Windows.Forms;
using Diz.Core.model;
using DiztinGUIsh.util;

namespace DiztinGUIsh.controller
{
    public class ProjectOpenerGenericView : IProjectOpener
    {
        public ILongRunningTaskHandler.LongRunningTaskHandler TaskHandler =>
            ProgressBarJob.RunAndWaitForCompletion;

        public void OnProjectOpenSuccess(string filename, Project project)
        {
            MessageBox.Show("project file opened!");
        }

        public void OnProjectOpenWarning(string warnings)
        {
            MessageBox.Show($"project file opened, with warnings:\n {warnings}");
        }

        public void OnProjectOpenFail(string fatalError)
        {
            MessageBox.Show($"project file failed to open:\n {fatalError}");
        }

        public string AskToSelectNewRomFilename(string error)
        {
            string initialDir = null; // TODO: Project.ProjectFileName
            return GuiUtil.PromptToConfirmAction("Error", $"{error} Link a new ROM now?", 
                () => GuiUtil.PromptToSelectFile(initialDir)
            );
        }

        public static Project OpenProjectWithGui(string filename) => 
            new ProjectOpenerGuiController { Gui = new ProjectOpenerGenericView() }.OpenProject(filename);
    }
}