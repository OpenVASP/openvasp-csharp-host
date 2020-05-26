using Autofac;
using OpenVASP.Host.Core.Repositories;

namespace OpenVASP.Host.Persistence.Implementation
{
    public static class AutofacPersistenceExtensions
    {
        public static void RegisterInMemoryRepositories(this ContainerBuilder builder)
        {
            builder
                .RegisterType<SessionsRepository>()
                .As<ISessionsRepository>()
                .SingleInstance();
            
            builder
                .RegisterType<TransactionsRepository>()
                .As<ITransactionsRepository>()
                .SingleInstance();
        }
    }
}