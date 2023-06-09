namespace Stockband.Application.Interfaces.FeatureServices;

public interface IProjectFeaturesService
{
    Task<bool> IsProjectsLimitExceeded(int projectOwnerId);
    Task<bool> IsProjectNameAlreadyExists(string projectName);
    
}