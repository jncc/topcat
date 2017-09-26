using Ninject;
using Ninject.Syntax;
using Quartz;
using Quartz.Spi;

namespace Catalogue.Robot.Injection
{
    public class NinjectJobFactory : IJobFactory
    {
        private readonly IResolutionRoot resolutionRoot;

        public NinjectJobFactory(IResolutionRoot resolutionRoot)
        {
            this.resolutionRoot = resolutionRoot;
        }

        public IJob NewJob(TriggerFiredBundle bundle, IScheduler scheduler)
        {
            return resolutionRoot.Get(bundle.JobDetail.JobType) as IJob;
        }

        public void ReturnJob(IJob job)
        {
            resolutionRoot.Release(job);
        }
    }
}
