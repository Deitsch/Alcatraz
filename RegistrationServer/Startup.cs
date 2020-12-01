using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using RegistrationServer.Listener;
using RegistrationServer.Repositories;
using RegistrationServer.Services;
using RegistrationServer.Spread;
using RegistrationServer.Spread.Interface;
using RegistrationServer.Spread.Operations;
using System;
using System.Threading;

namespace RegistrationServer
{
    public class Startup
    {
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddGrpc();
            services.AddSingleton<ISpreadService, SpreadService>();
            services.AddSingleton<ISpreadConnectionWrapper, SpreadConnectionWrapper>();
            services.AddSingleton<IOperationManager, OperationManager>();
            services.AddSingleton<MessageListener>();
            services.AddSingleton<GameService>();
            services.AddSingleton<LobbyService>();
            services.AddSingleton<LobbyRepository>();
            services.AddSingleton<CreateLobbyOperation>();
            services.AddSingleton<GetLobbiesOperation>();
            services.AddSingleton<JoinLobbyOperation>();
            services.AddSingleton<LeaveLobbyOperation>();
            services.AddSingleton<RequestGameStartOperation>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ISpreadConnectionWrapper connection, ISpreadService spreadService, IOperationManager operationManager)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapGrpcService<LobbyService>();
                endpoints.MapGrpcService<GameService>();

                endpoints.MapGet("/", async context =>
                {
                    await context.Response.WriteAsync("Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");
                });
            });

            connection.Connect(ConfigFile.SPREAD_ADDRESS, ConfigFile.SPREAD_PORT, Guid.NewGuid().ToString(), ConfigFile.SPREAD_PRIORITY, ConfigFile.SPREAD_GROUP_MEMBERSHIP);
            operationManager.AddOperationListeners();
            Thread thread = new Thread(() => spreadService.Run());
            thread.Start();
        }
    }
}
