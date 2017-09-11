using Ninject;
using Ninject.Syntax;
using Quartz;
using Quartz.Spi;

namespace Catalogue.Robot.Injection
{
    public class NinjectJobFactory : IJobFactory
    {
        private readonly IResolutionRoot _resolutionRoot;

        public NinjectJobFactory(IResolutionRoot resolutionRoot)
        {
            _resolutionRoot = resolutionRoot;
        }

        public IJob NewJob(TriggerFiredBundle bundle, IScheduler scheduler)
        {
            return _resolutionRoot.Get(bundle.JobDetail.JobType) as IJob;
        }

        public void ReturnJob(IJob job)
        {
            _resolutionRoot.Release(job);
        }
    }
}
