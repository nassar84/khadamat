using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using FluentValidation;
using MediatR;

namespace Khadamat.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // Add application services, validators, etc.
        services.AddAutoMapper(Assembly.GetExecutingAssembly());
        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblies(Assembly.GetExecutingAssembly()));
        
        // Legacy service - can be removed once full migration is complete
        // services.AddScoped<Khadamat.Application.Interfaces.IServiceService, Khadamat.Application.Services.ServiceService>();
        
        return services;
    }
}
