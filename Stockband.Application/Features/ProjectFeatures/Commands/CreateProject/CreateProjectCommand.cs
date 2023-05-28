using MediatR;
using Stockband.Domain;

namespace Stockband.Application.Features.ProjectFeatures.Commands.CreateProject;

public class CreateProjectCommand:IRequest<BaseResponse>
{
    public int OwnerId { get; set; }
    public string ProjectName { get; set; }
    public string ProjectDescription { get; set; }
}