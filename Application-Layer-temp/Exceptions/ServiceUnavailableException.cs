namespace Application_Layer_temp.Exceptions;

public class ServiceUnavailableException : Exception
{
    public ServiceUnavailableException(string message)
        : base(message) { }

    public ServiceUnavailableException(string serviceName)
        : base($"Service '{serviceName}' is currently unavailable. Please try again later.") { }
}
