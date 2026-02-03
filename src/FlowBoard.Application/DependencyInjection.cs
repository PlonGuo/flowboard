using FlowBoard.Application.Behaviors;
using FlowBoard.Application.Services;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace FlowBoard.Application;

/// <summary>
/// Extension methods for registering Application layer services.
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        var assembly = typeof(DependencyInjection).Assembly;

        // Register MediatR with pipeline behaviors
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(assembly);
            cfg.AddBehavior(typeof(MediatR.IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
            cfg.AddBehavior(typeof(MediatR.IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
        });

        // Register FluentValidation validators
        services.AddValidatorsFromAssembly(assembly);

        // Register AutoMapper (scans assembly for profiles)
        services.AddAutoMapper(assembly);

        // Register application services
        services.AddScoped<IMentionService, MentionService>();

        return services;
    }
}
