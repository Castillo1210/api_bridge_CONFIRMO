namespace Confirmo.Api.Services;

public interface IPythonWorkerClient
{
    Task EnqueueProcessAsync(string depositId);
}