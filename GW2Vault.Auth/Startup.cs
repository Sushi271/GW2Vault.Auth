using System;
using System.Linq;
using System.Reflection;
using GW2Vault.Auth.ActionFilters;
using GW2Vault.Auth.Data;
using GW2Vault.Auth.Helpers;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace GW2Vault.Auth
{
    public class Startup
    {
        public const bool ProdBuild = true;
        public const string ConnectionString_Dev = "Dev";
        public const string ConnectionString_Prod = "Prod";

        public Startup(IConfiguration configuration)
            => Configuration = configuration;

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers(options =>
                options.Filters.Add<EncryptionAuthenticationFilter>());

#pragma warning disable CS0162 // Unreachable code detected
            if (ProdBuild)
            {
                var connectionString = Configuration.GetConnectionString(ConnectionString_Prod);
                services.AddDbContextPool<AuthContext>(options =>
                   options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));
            }
            else
            {
                var connectionString = Configuration.GetConnectionString(ConnectionString_Dev);
                services.AddDbContext<AuthContext>(options => options.UseSqlServer(connectionString));
            }
#pragma warning restore CS0162 // Unreachable code detected

            AddAllEnabledServices(services);
        }

        void AddAllEnabledServices(IServiceCollection services)
        {
            var types = Assembly.GetExecutingAssembly().GetTypes();
            var implementationTypes = types.Where(t => t.GetCustomAttribute<EnableDependencyInjectionAttribute>() != null);
            foreach (var implementationType in implementationTypes)
            {
                var serviceType = implementationType.GetInterface($"I{implementationType.Name}");
                if (serviceType == null)
                    throw new InvalidOperationException($"Type {implementationType.FullName} has {nameof(EnableDependencyInjectionAttribute)}, " +
                        $"but it does not implement an interface with name 'I{implementationType.Name}'");
                services.AddScoped(serviceType, implementationType);
            }
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
#pragma warning disable CS0162 // Unreachable code detected
            if (ProdBuild)
                app.UseForwardedHeaders(new ForwardedHeadersOptions
                {
                    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
                });
#pragma warning restore CS0162 // Unreachable code detected


            if (env.IsDevelopment())
                app.UseDeveloperExceptionPage();

            app.UseHttpsRedirection();
            app.UseRouting();
#pragma warning disable CS0162 // Unreachable code detected
            if (ProdBuild)
            {
                app.UseStaticFiles();
                app.UseAuthorization();
            }
#pragma warning restore CS0162 // Unreachable code detected

            app.UseEndpoints(endpoints => endpoints.MapControllers());
        }
    }
}
