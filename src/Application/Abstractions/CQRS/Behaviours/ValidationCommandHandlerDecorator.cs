using FluentResults;
using FluentValidation;

namespace Application.Abstractions.CQRS.Behaviours;

/// <summary>
/// A decorator for command handlers that handles registered validators for request validation.
/// If a validator is registered for the command type, it will be invoked before the inner handler
/// </summary>
public class ValidationCommandHandlerDecorator<TCommand, TResponse> : ICommandHandler<TCommand, TResponse>
    where TCommand : ICommand<TResponse>
    where TResponse : class
{
    private readonly ICommandHandler<TCommand, TResponse> _innerHandler;
    private readonly IValidator<TCommand>? _validator;

    public ValidationCommandHandlerDecorator(ICommandHandler<TCommand, TResponse> innerHandler, IValidator<TCommand>? validator = null)
    {
        _innerHandler = innerHandler ?? throw new ArgumentNullException(nameof(innerHandler));
        _validator = validator; // optional, null if no validator registered
    }

    public async Task<Result<TResponse>> Handle(TCommand command, CancellationToken ct = default)
    {
        if (_validator is null)
            return await _innerHandler.Handle(command, ct);
        
        
        var validationResult = await _validator.ValidateAsync(command, ct);
        if (!validationResult.IsValid)
        {
            // mapping FluentValidation errors to FluentResults errors with PropertyName metadata
            var errors = validationResult.Errors
                .Select(e => new Error(e.ErrorMessage).WithMetadata("PropertyName", e.PropertyName))
                .ToList();
            return Result.Fail<TResponse>(errors);
        }

        // passed or no validator; proceed to inner handler
        return await _innerHandler.Handle(command, ct);
    }
}

// non-generic ICommandHandler<TCommand>
public class ValidationCommandHandlerDecorator<TCommand> : ICommandHandler<TCommand>
    where TCommand : ICommand
{
    private readonly ICommandHandler<TCommand> _innerHandler;
    private readonly IValidator<TCommand>? _validator;

    public ValidationCommandHandlerDecorator(ICommandHandler<TCommand> innerHandler, IValidator<TCommand>? validator = null)
    {
        _innerHandler = innerHandler ?? throw new ArgumentNullException(nameof(innerHandler));
        _validator = validator;
    }

    public async Task<Result> Handle(TCommand command, CancellationToken ct = default)
    {
        if (_validator is null) 
            return await _innerHandler.Handle(command, ct);
        
        
        var validationResult = await _validator.ValidateAsync(command, ct);
        if (!validationResult.IsValid)
        {
            var errors = validationResult.Errors
                .Select(e => new Error(e.ErrorMessage).WithMetadata("PropertyName", e.PropertyName))
                .ToList();
            return Result.Fail(errors);
        }

        return await _innerHandler.Handle(command, ct);
    }
}