using System.ServiceProcess;

namespace GitAutoCommitService
{
  internal static class Program
  {
    /// <summary>
    /// The main entry point for the application.
    /// </summary>
    static void Main()
    {
      ServiceBase[] ServicesToRun;
      ServicesToRun = new ServiceBase[]
      {
                new AutoCommitService()
      };

#if DEBUG
      var dbgCommitService = new AutoCommitService();
      dbgCommitService.Debug();
#else
      ServiceBase.Run(ServicesToRun);
#endif
    }
  }
}
