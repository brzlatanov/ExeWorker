using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ExeExecutorWorker.Models;
using Microsoft.Extensions.Logging;
using Quartz;

namespace ExeExecutorWorker
{
    [DisallowConcurrentExecution]
    public class Job : IJob
    {
        public async Task Execute(IJobExecutionContext context)
        {
            var logger = context.MergedJobDataMap.Get("logger") as ILogger;
            //await File.AppendAllTextAsync(@"c:\temp\sample.log", DateTime.Now.ToString());

            var dbContext = new ExeWorkerContext();
            await dbContext.Database.EnsureCreatedAsync();

            var paths = dbContext.Paths.ToArray();

            if (paths.Any())
            {
                foreach (var path in paths)
                {
                    if (path.Time.Minute == DateTime.Now.Minute)
                    {
                        System.Diagnostics.Process.Start(path.Path1);
                        await File.AppendAllTextAsync(@"c:\temp\ExeExecutorLog.log", $"{DateTime.Now.ToString()} - started process {path.Path1}" + Environment.NewLine);
                    }
                    else
                    {
                        await File.AppendAllTextAsync(@"c:\temp\ExeExecutorLog.log", $"{DateTime.Now.ToString()} - no processes to start at this time" + Environment.NewLine);
                    }
                }
            }

            logger.LogInformation($"Executed job successfully at {DateTime.Now.ToString()}");
        }
    }
}