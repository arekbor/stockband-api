using System.Net;
using FlueFlame.Http.Modules;
using Shouldly;
using Stockband.Api.E2E.Builders;
using Stockband.Application.Features.ProjectFeatures.Commands.RemoveProject;
using Stockband.Domain.Enums;
using Stockband.Domain.Common;

namespace Stockband.Api.E2E.Controllers.ProjectController;

[TestFixture]
public class RemoveProjectTests:BaseTest
{
    private ProjectBuilder _projectBuilder = null!;
    private UserBuilder _userBuilder = null!;
    private ProjectMemberBuilder _projectMemberBuilder = null!;
    
    private const string TestingUri = "/project";
    
    [SetUp]
    public void SetUp()
    {
        _projectBuilder = new ProjectBuilder(Context);
        _userBuilder = new UserBuilder(Context);
        _projectMemberBuilder = new ProjectMemberBuilder(Context);
    }

    [Test]
    public async Task RemoveProject_BaseResponse_Success_ShouldBeTrue()
    {
        //Arrange
        const int testingRequestedUserId = 5000;
        await _userBuilder
            .Build(userId: testingRequestedUserId);
        
        const int testingProjectId = 5443;
        await _projectBuilder
            .Build(projectId: testingProjectId, ownerProjectId:testingRequestedUserId);

        RemoveProjectCommand command = new RemoveProjectCommand(testingProjectId);
        
        //Act
        HttpResponseModule responseModule = ActResponseModule(command, GetUserJwtToken(testingRequestedUserId));

        //Assert
        responseModule.AssertStatusCode(HttpStatusCode.OK);
        responseModule.AsJson.AssertThat<BaseResponse>(response =>
        {
            response.Success.ShouldBe(true);
            response.Errors.Count.ShouldBe(0);
        });
    }

    [Test]
    public async Task RemoveProject_ProvidedProjectIdNotExists_BaseErrorCodeShouldBe_ProjectNotExists()
    {
        //Arrange
        const int testingRequestedUserId = 9120;
        await _userBuilder
            .Build(userId: testingRequestedUserId);
        
        const int testingProjectId = 400;

        RemoveProjectCommand command = new RemoveProjectCommand(testingProjectId);
        
        //Act
        HttpResponseModule responseModule = ActResponseModule(command, GetUserJwtToken(testingRequestedUserId));

        //Assert
        responseModule.AssertStatusCode(HttpStatusCode.BadRequest);
        responseModule.AsJson.AssertThat<BaseResponse>(response =>
        {
            response.Success.ShouldBe(false);
            response.Errors.Count.ShouldBe(1);
            response.Errors.First().Code.ShouldBe(BaseErrorCode.ProjectNotFound);
        });
    }

    [Test]
    public async Task RemoveProject_RequestedUserIdIsNotOwner_BaseErrorCodeShouldBe_UserUnauthorizedOperation()
    {
        //Arrange
        const int testingRequestedUserId = 2500;
        await _userBuilder
            .Build(userId: testingRequestedUserId);
        
        const int testingProjectId = 6000;
        await _projectBuilder
            .Build(projectId: testingProjectId);

        RemoveProjectCommand command = new RemoveProjectCommand(testingProjectId);
        
        //Act
        HttpResponseModule responseModule = ActResponseModule(command, GetUserJwtToken(testingRequestedUserId));

        //Assert
        responseModule.AssertStatusCode(HttpStatusCode.BadRequest);
        responseModule.AsJson.AssertThat<BaseResponse>(response =>
        {
            response.Success.ShouldBe(false);
            response.Errors.Count.ShouldBe(1);
            response.Errors.First().Code.ShouldBe(BaseErrorCode.UserUnauthorizedOperation);
        });
    }

    [Test]
    public async Task RemoveProject_ProjectHasSomeMembers_BaseErrorCodeShouldBe_UserOperationRestricted()
    {
        //Arrange
        const int testingRequestedUserId = 5000;
        await _userBuilder
            .Build(userId: testingRequestedUserId);
        
        const int testingProjectId = 5443;
        await _projectBuilder
            .Build(projectId: testingProjectId, ownerProjectId:testingRequestedUserId);

        await _projectMemberBuilder
            .Build(500, testingProjectId, testingRequestedUserId);

        RemoveProjectCommand command = new RemoveProjectCommand(testingProjectId);
        
        //Act
        HttpResponseModule responseModule = ActResponseModule(command, GetUserJwtToken(testingRequestedUserId));

        //Assert
        responseModule.AssertStatusCode(HttpStatusCode.BadRequest);
        responseModule.AsJson.AssertThat<BaseResponse>(response =>
        {
            response.Success.ShouldBe(false);
            response.Errors.Count.ShouldBe(1);
            response.Errors.First().Code.ShouldBe(BaseErrorCode.UserOperationRestricted);
        });
    }
    
    private HttpResponseModule ActResponseModule(RemoveProjectCommand command, string jwtToken)
    {
        return HttpHost
            .Delete
            .WithJwtToken(jwtToken)
            .Url(TestingUri)
            .Json(command)
            .Send()
            .Response;
    }
}