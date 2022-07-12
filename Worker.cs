using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Quartz;
using Quartz.Impl;

namespace ExeExecutorWorker
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> logger;
        private IScheduler scheduler;
        private StdSchedulerFactory stdSchedulerFactory;

        public Worker(ILogger<Worker> logger)
        {
            this.logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await ScheduleJob(stoppingToken);

            while (!stoppingToken.IsCancellationRequested)
            {
                this.logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
                await Task.Delay(1000, stoppingToken);
            }

            await this.scheduler.Shutdown();
        }

        private async Task ScheduleJob(CancellationToken stoppingToken)
        {
            this.stdSchedulerFactory = new StdSchedulerFactory();
            this.scheduler = await stdSchedulerFactory.GetScheduler();
            await this.scheduler.Start();

            IJobDetail sample = JobBuilder.Create<Job>().WithIdentity("Job1", "Group1").Build();

            sample.JobDataMap.Add("logger", this.logger);

            ITrigger trigger = TriggerBuilder.Create().WithIdentity("1mintrigger", "Group1").StartNow()
                .WithSimpleSchedule(config =>
                    config.WithIntervalInSeconds(60).WithMisfireHandlingInstructionIgnoreMisfires().RepeatForever())
                .Build();

            await scheduler.ScheduleJob(sample, trigger, stoppingToken);


        }
    }
}
