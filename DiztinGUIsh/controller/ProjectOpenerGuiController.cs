using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Diz.Core.model;
using Diz.Core.serialization;
using DiztinGUIsh.util;

namespace DiztinGUIsh.controller
{
    public class ProjectOpenerGuiController
    {
        public IProjectOpenerHandler Handler { get; init; }

        public Project OpenProject(string filename)
        {
            Project project = null;
            var errorMsg = "";
            var warningMsg = "";

            ProgressReportingGuiUtils.DoLongRunningTask(() =>
            {
                try
                {
                    var (project1, warning) = new ProjectFileManager()
                    {
                        RomPromptFn = Handler.AskToSelectNewRomFilename
                    }.Open(filename);

                    project = project1;
                    warningMsg = warning;
                }
                catch (AggregateException ex)
                {
                    project = null;
                    errorMsg = ex.InnerExceptions.Select(e => e.Message).Aggregate((line, val) => line += val + "\n");
                }
                catch (Exception ex)
                {
                    project = null;
                    errorMsg = ex.Message;
                }
            }, $"Opening {Path.GetFileName(filename)}...");
            
            if (project == null)
            {
                Handler.OnProjectOpenFail(errorMsg);
                return null;
            }

            if (warningMsg != "")
                Handler.OnProjectOpenWarning(warningMsg);
            
            Handler.OnProjectOpenSuccess(filename, project);
            return project;
        }
    }
}