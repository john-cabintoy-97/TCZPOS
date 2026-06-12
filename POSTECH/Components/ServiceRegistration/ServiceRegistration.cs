using FluentValidation;
using Plugin.Maui.Audio;
using TCZPOS.Components.AI;
using TCZPOS.Components.DTOs;
using TCZPOS.Components.Guard;
using TCZPOS.Components.Guard.Interface;
using TCZPOS.Components.Models;
using TCZPOS.Components.Providers;
using TCZPOS.Components.Repositories.Interfaces;
using TCZPOS.Components.Services;
using TCZPOS.Components.Services.Hardware;
using TCZPOS.Components.Validation;
using System.Reflection;
using TCZPOS.Components.Extension;


namespace TCZPOS.Components.ServiceRegistration
{
    public static class ServiceRegistration
    {
        public static IServiceCollection AddPOSServices(this IServiceCollection services)
        {
            // Repositories
            RegisterImplementations(services, "TCZPOS.Components.Repositories", "TCZPOS.Components.Repositories.Interfaces");

            // Services
            RegisterImplementations(services, "TCZPOS.Components.Services");
            services.AddScoped<CustomerCreditService>();
            //services.AddSingleton<UserServices>();
            //services.AddSingleton<CategoryServices>();
            // services.AddSingleton<ProductServices>();
            services.AddSingleton<QRCodeServices>();
            services.AddSingleton<HardwareServices>();
            services.AddSingleton<ScannerNativeServices>();
            services.AddSingleton<ScannerServices>();
            services.AddSingleton<PairingListenerServices>();
            services.AddSingleton<TCPClientServices>();
            services.AddSingleton<ProductAIService>();
            services.AddSingleton<FileLauncherExt>();

            services.AddSingleton<IGatekeeperService, GatekeeperService>();

            // Providers
            services.AddSingleton<ConnectionStateProvider>();

            // Pages
            services.AddSingleton<MainPage>();

            // Data Transfer Objects
            services.AddSingleton<ProductViewDTO>();

            // Validators
            services.AddScoped<IValidator<CategoryModels>, CategoryValidator>();
            services.AddScoped<IValidator<ProductModels>, ProductValidator>();
            services.AddScoped<IValidator<BrandModels>, BrandValidator>();
            //services.AddScoped<IValidator<CancelModels>, CancelValidator>();
            //services.AddScoped<IValidator<CartModels>, CartValidator>();
            //services.AddScoped<IValidator<SaleDetailModels>, SaleDetailValidator>();
            //services.AddScoped<IValidator<SaleModels>, SaleValidator>();
            //services.AddScoped<IValidator<StockInModels>, StockInValidator>();
            //services.AddScoped<IValidator<VatModels>, VatValidator>();
            services.AddScoped<IValidator<VendorModels>, VendorValidator>();

            services.AddSingleton(AudioManager.Current);
            return services;
        }


        private static void RegisterImplementations(IServiceCollection services, string implementationNamespace, string interfaceNamespace = null!)
        {
            var assembly = Assembly.GetExecutingAssembly();

            // Get all public, non-abstract classes in the namespace
            var types = assembly.GetTypes()
                                .Where(t => t.IsClass && !t.IsAbstract && t.Namespace == implementationNamespace);

            foreach (var impl in types)
            {
                if (!string.IsNullOrEmpty(interfaceNamespace))
                {
                    // Try to find a matching interface by convention: I{ClassName}
                    var iface = assembly.GetTypes()
                                        .FirstOrDefault(i => i.IsInterface
                                                          && i.Namespace == interfaceNamespace
                                                          && i.Name == $"I{impl.Name}");
                    if (iface != null)
                    {
                        services.AddSingleton(iface, impl);
                        continue;
                    }
                }

                // Otherwise, just register the class itself
                services.AddSingleton(impl);
            }
        }
    }
}
