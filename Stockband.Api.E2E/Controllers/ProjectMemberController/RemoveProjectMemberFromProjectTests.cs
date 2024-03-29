using System.Net;
using FlueFlame.Http.Modules;
using Shouldly;
using Stockband.Api.E2E.Builders;
using Stockband.Application.Features.ProjectMemberFeatures.Commands.RemoveMemberFromProject;
using Stockband.Domain.Enums;
using Stockband.Domain.Common;

namespace Stockband.Api.E2E.Controllers.ProjectMemberController;

[TestFixture]
public class RemoveProjectMemberFromProjectTests:BaseTest
{
    private ProjectBuilder _projectBuilder = null!;
    private UserBuilder _userBuilder = null!;
    private ProjectMemberBuilder _projectMemberBuilder = null!;
    
    private const string TestingUri = "/projectMember";
    
    [SetUp]
    public void SetUp()
    {
        _projectBuilder = new ProjectBuilder(Context);
        _userBuilder = new UserBuilder(Context);
        _projectMemberBuilder = new ProjectMemberBuilder(Context);
    }

    [Test]
    public async Task RemoveProjectMemberFromProject_BaseResponse_Success_ShouldBeTrue()
    {
        //Arrange
        const int testingRequestedUserId = 2300;
        await _userBuilder
            .Build(userId: testingRequestedUserId);
        
        const int testingProjectId = 1200;
        await _projectBuilder
            .Build(projectId: testingProjectId, testingRequestedUserId);

        const int testingMemberIdToDelete = 3400;
        await _userBuilder
            .Build(userId: testingMemberIdToDelete);

        await _projectMemberBuilder
            .Build(6500, projectId: testingProjectId, memberId: testingMemberIdToDelete);

        RemoveMemberFromProjectCommand command = new RemoveMemberFromProjectCommand
            (testingProjectId, testingMemberIdToDelete);

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
    public async Task RemoveProjectMemberFromProject_ProjectMemberNotFound_BaseErrorCodeShouldBe_ProjectMemberNotExists()
    {
        //Arrange
        const int testingRequestedUserId = 200;
        await _userBuilder
            .Build(userId: testingRequestedUserId);
        
        RemoveMemberFromProjectCommand command = new RemoveMemberFromProjectCommand
            (200, 3400);

        //Act
        HttpResponseModule responseModule = ActResponseModule(command, GetUserJwtToken(testingRequestedUserId));

        //Assert
        responseModule.AssertStatusCode(HttpStatusCode.BadRequest);
        responseModule.AsJson.AssertThat<BaseResponse>(response =>
        {
            response.Success.ShouldBe(false);
            response.Errors.Count.ShouldBe(1);
            response.Errors.First().Code.ShouldBe(BaseErrorCode.ProjectMemberNotFound);
        });
    }

    [Test]
    public async Task RemoveProjectMemberFromProject_RequestedUserIsNotOwner_BaseErrorCodeShouldBe_UserUnauthorizedOperation()
    {
        //Arrange
        const int testingRequestedUserId = 7500;
        await _userBuilder
            .Build(userId: testingRequestedUserId);
        
        const int testingProjectId = 8900;
        await _projectBuilder
            .Build(projectId: testingProjectId);

        const int testingMemberIdToDelete = 5122;
        await _userBuilder
            .Build(userId: testingMemberIdToDelete);

        await _projectMemberBuilder
            .Build(6500, projectId: testingProjectId, memberId: testingMemberIdToDelete);

        RemoveMemberFromProjectCommand command = new RemoveMemberFromProjectCommand
            (testingProjectId, testingMemberIdToDelete);

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
    
    private HttpResponseModule ActResponseModule(RemoveMemberFromProjectCommand command, string jwtToken)
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