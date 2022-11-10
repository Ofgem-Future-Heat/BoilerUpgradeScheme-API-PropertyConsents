using FluentValidation.AspNetCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Notify.Client;
using Notify.Interfaces;
using Ofgem.API.BUS.PropertyConsents.Core.Configuration;
using Ofgem.API.BUS.PropertyConsents.Core.FluentValidation;
using Ofgem.API.BUS.PropertyConsents.Core.Interfaces;

namespace Ofgem.API.BUS.PropertyConsents.Core
{
    /// <summary>
    /// Service Extensions to add IOwnerConsentService implementation to WebApp DI
    /// </summary>
    public static class ServiceExtensions
    {
        /// <summary>
        /// Configuration to add IOwnerConsentService implementation to WebApp DI
        /// </summary>
        /// <param name="services"></param>
        /// <param name="configuration"></param>
        /// <returns></returns>
        public static IServiceCollection AddServiceConfigurations(this IServiceCollection services, IConfiguration config)
        {
            AddFluentValidationConfiguration(services, config);

            var govNotifyConfig = config.GetSection("GovNotify");
            string apiKey = govNotifyConfig["APIKey"];
            string templateId = govNotifyConfig["ConsentEmailTemplateId"];
            string ownerConfirmEmailTemplateId = govNotifyConfig["ConsentOwnerConfirmEmailTemplateID"];
            string installerNotChosenEmailTemplateId = govNotifyConfig["ConsentInstallerNotChosenEmailTemplateID"];
            string installerConfirmEmailTemplateId = govNotifyConfig["ConsentInstallerConfirmEmailTemplateID"];
            string consentInstallerNotChosenEmailTemplateId = govNotifyConfig["ConsentInstallerNotChosenEmailTemplateID"];
            string consentTokenSecret = govNotifyConfig["ConsentTokenSecret"];

            services.AddTransient<IAsyncNotificationClient>(s => new NotificationClient(apiKey));
            services.AddTransient<IOwnerConsentServiceOptions>(s => new OwnerConsentServiceOptions(templateId, consentOwnerConfirmEmailTemplateID: ownerConfirmEmailTemplateId,consentInstallerConfirmEmailTemplateID: installerConfirmEmailTemplateId, consentInstallerNotChosenEmailTemplateID: installerNotChosenEmailTemplateId, consentTokenSecret));
            services.AddTransient<IOwnerConsentService, OwnerConsentService>();
            services.AddTransient<IApplicationsAPIService, ApplicationsAPIService>();

            return services;
        }

        /// <summary>
        /// Adds the configuration for Fluent validation
        /// </summary>
        /// <param name="services"></param>
        /// <param name="config"></param>
        /// <returns></returns>
        private static IServiceCollection AddFluentValidationConfiguration(this IServiceCollection services, IConfiguration config)
        {
            services.AddFluentValidation(fv =>
            {
                fv.DisableDataAnnotationsValidation = true;
                fv.RegisterValidatorsFromAssemblyContaining<SendConsentEmailRequestValidator>();
            });

            return services;
        }
    }
}
