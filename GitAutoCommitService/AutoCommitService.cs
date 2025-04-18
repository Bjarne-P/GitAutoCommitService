using System;
using System.Diagnostics;
using System.ServiceProcess;
using System.Timers;

namespace GitAutoCommitService
{
  public partial class AutoCommitService : ServiceBase
  {
    private Timer _timer;

    public AutoCommitService()
    {
      InitializeComponent();
    }

    public void Debug()
    {
      OnStart(null);
      Console.ReadKey();
    }

    protected override void OnStart(string[] args)
    {
      if (!EventLog.SourceExists("GitAutoCommitService"))
      {
        EventLog.CreateEventSource("GitAutoCommitService", "Application");
      }
      EventLog.Source = "GitAutoCommitService";
      EventLog.Log = "Application";

      _timer = new Timer(Properties.Settings.Default.UpdateIntervallInSec * 1000); // Timer every 5 minutes
      _timer.Elapsed += TimerElapsed;
      _timer.Start();
#if DEBUG
      Console.WriteLine($"AutoCommitService started.");
#else          
      EventLog.WriteEntry("AutoCommitService started.");
#endif
    }

    protected override void OnStop()
    {
      _timer?.Stop();
      _timer?.Dispose();
#if DEBUG
      Console.WriteLine($"AutoCommitService stopped.");
#else          
      EventLog.WriteEntry("AutoCommitService stopped.");
#endif
    }

    private void TimerElapsed(object sender, ElapsedEventArgs e)
    {
      try
      {
        string gitStatusOutput = ExecuteGitCommand("status --porcelain");

        if (!string.IsNullOrEmpty(gitStatusOutput))
        {
          ExecuteGitCommand("add -A");
          ExecuteGitCommand("commit -m \"auto update\"");
          ExecuteGitCommand("push origin");

#if DEBUG
          Console.WriteLine($"Changes detected and pushed to remote repository.");
#else          
          EventLog.WriteEntry("Changes detected and pushed to remote repository.");
#endif
        }
        else
        {
#if DEBUG
          Console.WriteLine($"No changes detected.");
#else          
          EventLog.WriteEntry("No changes detected.", EventLogEntryType.Information);
#endif
        }
      }
      catch (Exception ex)
      {
#if DEBUG
        Console.WriteLine($"Error: {ex.Message}");
#else
        EventLog.WriteEntry($"Error: {ex.Message}", EventLogEntryType.Error);
#endif
      }
    }

    private string ExecuteGitCommand(string arguments)
    {
      var processStartInfo = new ProcessStartInfo
      {
        FileName = "git",
        Arguments = arguments,
        WorkingDirectory = Properties.Settings.Default.RepoPath,
        RedirectStandardOutput = true,
        RedirectStandardError = true,
        UseShellExecute = false,
        CreateNoWindow = true
      };

      using (var process = new Process { StartInfo = processStartInfo })
      {
        process.Start();
        string output = process.StandardOutput.ReadToEnd();
        string error = process.StandardError.ReadToEnd();
        process.WaitForExit();

        if (process.ExitCode != 0)
        {
          throw new Exception($"Git command failed: {error.Trim()}");
        }

        return output.Trim();
      }
    }
  }
}
