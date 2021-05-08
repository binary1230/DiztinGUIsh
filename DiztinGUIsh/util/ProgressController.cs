using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows.Forms;
using DiztinGUIsh.controller;
using DiztinGUIsh.window.dialog;
using JetBrains.Annotations;

namespace DiztinGUIsh.util
{
    public interface IProgressReporterController
    {
        IProgressBarView View { get; }
        
        IProgress<ProgressReport> Begin();
        void End();
        void OnTaskStarted();
        void Close();
        Task ShowDialogAsync();
    }

    public interface IProgressBarView : IFormViewer
    {
        public bool IsMarquee { get; set; }
        void UpdateProgress(ProgressReport report);
    }
    
    public class ProgressReportController : IProgressReporterController
    {
        public IProgressBarView View { get; private set; }
        public IProgress<ProgressReport> Begin()
        {
            if (View == null)
                CreateForm();

            return new Progress<ProgressReport>(UpdateProgress);
        }
        
        private void CreateForm()
        {
            Debug.Assert(View == null);
            View = new ProgressDialog();
        }

        public void OnTaskStarted()
        {
            // View?.ShowDialog();
        }

        public void Close() => View?.Close();
        public Task ShowDialogAsync()
        {
            // HACK, fix. add ShowDialogAsync() to interface
            var form = View as Form;
            Debug.Assert(form != null);
            return form.ShowDialogAsync(); // fine.
        }

        public void End()
        {
            // View?.Close();
        }

        private void UpdateProgress(ProgressReport report) => View?.UpdateProgress(report);
    }
    
    internal static class DialogExt
    {
        public static Task<bool> ShowDialogAsync(this Form @this)
        {
            if (@this == null) 
                throw new ArgumentNullException(nameof(@this));

            var completion = new TaskCompletionSource<bool>();
            @this.InvokeIfRequired(action: () =>
            {
                var dialogResult = @this.ShowDialog();
                completion.SetResult(dialogResult == DialogResult.OK);
            });

            return completion.Task;
        }

        /*public static async Task<DialogResult> ShowDialogAsync(this IFormViewer @this)
        {
            await Task.Yield();
            return @this.IsDisposed ? DialogResult.OK : @this.ShowDialog();
        }*/
    }
    
    public delegate Task ActionWithReporting(IProgress<ProgressReport> progressReporter);
    
    public interface IProgressReportingTaskRunner
    {
        Task Run(ActionWithReporting createTaskAction);
    }

    public interface IDizTask
    { 
        [CanBeNull] IProgress<ProgressReport> Progress { get; set; }
        Task Run();
    }

    public static class ProgressReportingExtensions
    {
        public static ActionWithReporting DecorateWithProgressReporting(this IDizTask @this) => @this.RunWithProgress;

        private static Task RunWithProgress(this IDizTask @this, IProgress<ProgressReport> progress)
        {
            @this.Progress = progress;
            return @this.Run();
        }

        public static void Report(this IProgress<ProgressReport> @this, int progressValue) => 
            @this.Report(new ProgressReport {Progress = progressValue});

        public static void ReportIfNeeded(this IProgress<ProgressReport> @this, int newValue, ref int previousValue)
        {
            if (newValue <= previousValue)
                return;
            
            @this.Report(newValue);
            previousValue = newValue;
        }
        
        public static void ReportIfNeeded(this IProgress<ProgressReport> @this, float getCurrentProgressPercent, ref int lastReportedProgress) => 
            @this.ReportIfNeeded((int)getCurrentProgressPercent * 100, ref lastReportedProgress);
    }

    public static class ProgressReportingGuiUtils
    {
        [Obsolete("Use Run() or RunWithGui() instead")]
        public static Task DoLongRunningTask(Action action, string description) => 
            CreateGuiRunInstance().Run(_ => new Task(action));

        public static Task RunWithGui(this IDizTask @this) => @this.DecorateWithProgressReporting().RunWithGui();

        public static Task RunWithGui(this ActionWithReporting @this) => 
            CreateGuiRunInstance().Run(@this);

        private static IProgressReportingTaskRunner CreateGuiRunInstance()
        {
            // TODO: dependency inject this stuff.
            return new RunTaskWithProgressGui
            {
                Controller = new ProgressReportController()
            };
        }
    }
    
    public class RunTaskWithProgressGui : IProgressReportingTaskRunner
    {
        public IProgressReporterController Controller { get; init; }

        private class NopReporter : IProgress<ProgressReport> { public void Report(ProgressReport value) {} } // NOP
        
        private IProgress<ProgressReport> InitAndGetProgressReporter()
        {
            return Controller?.Begin() ?? new NopReporter();
        }
        
        public static bool InProgress { get; set; }
        
        public async Task Run(ActionWithReporting createTaskAction)
        {
            Debug.Assert(!InProgress);
            InProgress = true;
            
            Debug.Assert(createTaskAction != null);

            // IMPORTANT: Never create the progress reporter inside of the Task.Run(), always do it out here
            // it's important to capture the thread context of the main thread in this enclosure.
            var progress = InitAndGetProgressReporter();
            
            // await Run(createTaskAction, progress); // original
            
            // https://stackoverflow.com/questions/33406939/async-showdialog
            Console.WriteLine("Main: ** 1: ------------ PROGRESS BAR START ----------");
            Console.WriteLine("Main: ** 2: about to ShowDialogAsync()");
            // var progressFormTask = Controller?.ShowDialogAsync();

            Console.WriteLine("Main: ** 3: await createTaskAction(progress)");
            
            var taskToRun = createTaskAction(progress);
            // /*var data =*/ await taskToRun;
            
            // taskToRun.Start();

            // await Task.WhenAll(new List<Task>
            // {
            //     Controller.ShowDialogAsync(),
            //     taskToRun,
            // }).ContinueWith(task =>
            // {
            //     Console.WriteLine("Main: ** 4: In ContinueWith");
            //     Controller?.Close();
            // });


            // Debug.Assert(progressFormTask != null);
            Console.WriteLine("Main: ** 5: Back in here!");
            // await progressFormTask;
            
            Console.WriteLine("Main: ** 6: ------------ PROGRESS BAR FULLY DONE! ----------");

            // Controller?.End();
            // Controller?.OnTaskStarted();
            
            InProgress = false;
        }

        // private static async Task Run(ProgressTask createTaskAction, IProgress<ProgressReport> progress) => 
        //     await Task.Run(() => createTaskAction(progress));
    }

    #region Dumb stuff
    public class ReallyDumbTask : IDizTask
    {
        public IProgress<ProgressReport> Progress { get; set; }

        public int InputNum { get; init; }
        
        public async Task Run()
        {
            Progress?.Report(new ProgressReport {Progress = 0, Text = "Dumb task GOING! Hang 1s"});

            // this should wait for 7 seconds max.
            await Task.WhenAll(new List<Task>
            {
                JunkWaitFor(5000),
                JunkWaitFor(6000),
                JunkWaitFor(7000),
            });

            var output = InputNum.ToString();
            Progress?.Report(new ProgressReport
                {Progress = 99, Text = "Finished all! Waitiing 1more sec"});
            
            await Task.Delay(1000);
        }

        private static int _step = 0;
        private static readonly object StepLock = new();

        private async Task JunkWaitFor(int ms)
        {
            Console.WriteLine($"waitiing {ms / 1000} seconds...");
            await Task.Delay(ms);
            var txt = $"Done waiting! ({ms / 1000} seconds!)";
            Console.WriteLine(txt);
            int currentStep;
            lock (StepLock)
            {
                currentStep = _step;
                _step++;
            }

            Progress?.Report(new ProgressReport {Progress = currentStep * 25, Text = txt});
        }
    }
    #endregion
}