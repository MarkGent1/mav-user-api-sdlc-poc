namespace Mav.UserMgmt.Api.Data;

public interface IDatabaseConnectionChecker
{
    Task<bool> IsConnectedAsync(CancellationToken cancellationToken = default);
}
