﻿using CrossCutting.Core.Bootstrapping;
using CrossCutting.Core.Configuration;
using CrossCutting.Core.Configuration.CommandLine;
using CrossCutting.Core.Configuration.ConfigObjects;
using CrossCutting.Core.Configuration.File;
using CrossCutting.Core.Contract.Bootstrapping;
using CrossCutting.Core.Contract.Configuration;
using CrossCutting.Core.Contract.DependencyInjection;
using CrossCutting.Core.Contract.DependencyInjection.DataClasses;
using CrossCutting.Core.Contract.EventBrokerage;
using CrossCutting.Core.Contract.Logging;
using CrossCutting.Core.Contract.Scheduling;
using CrossCutting.Core.Contract.Serialization;
using CrossCutting.Core.Contract.Settings;
using CrossCutting.Core.DI.AutofacAdapter;
using CrossCutting.Core.EventBrokerage;
using CrossCutting.Core.Logging.NLogAdapter;
using CrossCutting.Core.Scheduling.QuartzAdapter;
using CrossCutting.Core.Serialization.JsonAdapter;
using CrossCutting.Core.Settings;
using Logic.Business.InjectionManagement;
using Logic.Business.TimeTravelersManagement;
using Logic.Business.TranslationManagement;
using Logic.Domain.GoogleSheetsManagement;
using Logic.Domain.Kuriimu2.KomponentAdapter;
using Logic.Domain.Kuriimu2.KompressionAdapter;
using Logic.Domain.Kuriimu2.KryptographyAdapter;
using Logic.Domain.Level5Management;

namespace Mappings.TimeTravelersSheetBridge
{
    public class KernelInitializer : IKernelInitializer
    {
        private IKernelContainer _kernelContainer;

        public IKernelContainer CreateKernelContainer()
        {
            if (_kernelContainer == null)
            {
                _kernelContainer = new KernelContainer();
            }
            return _kernelContainer;
        }

        public void Initialize()
        {
            RegisterCoreComponents(_kernelContainer.Kernel);
            ActivateComponents(_kernelContainer.Kernel);
        }

        private void RegisterCoreComponents(ICoCoKernel kernel)
        {
            kernel.Register<IBootstrapper, Bootstrapper>(ActivationScope.Unique);
            kernel.Register<IEventBroker, EventBroker>(ActivationScope.Unique);
            kernel.Register<IConfigurationRepository, FileConfigurationRepository>();
            kernel.Register<IConfigurationRepository, CommandLineConfigurationRepository>();
            kernel.Register<IConfigurator, Configurator>(ActivationScope.Unique);
            kernel.Register<IConfigObjectProvider, ConfigObjectProvider>(ActivationScope.Unique);
            kernel.Register<ILogger, Logger>(ActivationScope.Unique);
            kernel.Register<ISerializer, JsonSerializer>();
            kernel.Register<IScheduler, Scheduler>(ActivationScope.Unique);
            kernel.Register<ISettingsProvider, SettingsProvider>(ActivationScope.Unique);
        }

        private void ActivateComponents(ICoCoKernel kernel)
        {
            kernel.RegisterComponent<InjectionManagementActivator>();
            kernel.RegisterComponent<TimeTravelersManagementActivator>();
            kernel.RegisterComponent<TranslationManagementActivator>();

            kernel.RegisterComponent<GoogleSheetsManagementActivator>();
            kernel.RegisterComponent<Kuriimu2KomponentActivator>();
            kernel.RegisterComponent<Kuriimu2KompressionActivator>();
            kernel.RegisterComponent<Kuriimu2KryptographyActivator>();
            kernel.RegisterComponent<Level5ManagementActivator>();
        }
    }
}