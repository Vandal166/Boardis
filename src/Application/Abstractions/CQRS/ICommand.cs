namespace Application.Abstractions.CQRS;

/// <summary>
/// An command in CQRS represents an intention to perform an action that changes the state of the system. Such as creating, updating;
/// </summary>
public interface ICommand;

/// <summary>
/// An command in CQRS represents an intention to perform an action that changes the state of the system. Such as creating, updating;
/// </summary>
/// <typeparam name="TResponse">The type of the response that the command will return.</typeparam>
public interface ICommand<TResponse>;