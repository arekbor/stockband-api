using System.Net;
using FlueFlame.Http.Modules;
using Shouldly;
using Stockband.Api.Dtos.ProjectMember;
using Stockband.Api.E2E.Builders;
using Stockband.Domain;
using Stockband.Domain.Common;

namespace Stockband.Api.E2E.Controllers.ProjectMemberController;

[TestFixture]
public class AddProjectMemberToProjectTests:BaseTest
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
    public async Task AddProjectMemberToProject_BaseResponse_Success_ShouldBeTrue()
    {
        //Arrange
        const int testingRequestedUserId = 2500;
        await _userBuilder
            .Build(userId: testingRequestedUserId);
        
        const int testingProjectId = 5000;
        await _projectBuilder
            .Build(projectId: testingProjectId, ownerProjectId:testingRequestedUserId);

        const int testingNewMemberId = 6500;
        await _userBuilder
            .Build(userId: testingNewMemberId);

        AddProjectMemberDto dto = new AddProjectMemberDto(testingProjectId, testingNewMemberId);

        //Act
        HttpResponseModule responseModule = ActResponseModule(dto, GetUserJwtToken(testingRequestedUserId));

        //Assert
        responseModule.AssertStatusCode(HttpStatusCode.OK);
        responseModule.AsJson.AssertThat<BaseResponse>(response =>
        {
            response.Success.ShouldBe(true);
            response.Errors.Count.ShouldBe(0);
        });
    }

    [Test]
    public void AddProjectMemberToProject_ProvidedProjectIdNotExists_BaseErrorCodeShouldBe_ProjectNotExists()
    {
        //Arrange
        AddProjectMemberDto dto = new AddProjectMemberDto(3200, 6500);
        
        //Act
        HttpResponseModule responseModule = ActResponseModule(dto, GetUserJwtToken(6000));
        
        //Assert
        responseModule.AssertStatusCode(HttpStatusCode.BadRequest);
        responseModule.AsJson.AssertThat<BaseResponse>(response =>
        {
            response.Success.ShouldBe(false);
            response.Errors.Count.ShouldBe(1);
            response.Errors.First().Code.ShouldBe(BaseErrorCode.ProjectNotExists);
        });
    }

    [Test]
    public async Task AddProjectMemberToProject_RequestedUserIsNotOwner_BaseErrorCodeShouldBe_UserUnauthorizedOperation()
    {
        //Arrange
        const int testingRequestedUserId = 1255;
        await _userBuilder
            .Build(userId: testingRequestedUserId);
        
        const int testingProjectId = 8700;
        await _projectBuilder
            .Build(projectId: testingProjectId);
        
        AddProjectMemberDto dto = new AddProjectMemberDto(testingProjectId, 7000);

        //Act
        HttpResponseModule responseModule = ActResponseModule(dto, GetUserJwtToken(testingRequestedUserId));

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
    public async Task
        AddProjectMemberToProject_ProvidedMemberIdNotExists_BaseErrorCodeShouldBe_MemberForProjectMemberNotExists()
    {
        //Arrange
        const int testingRequestedUserId = 3200;
        await _userBuilder
            .Build(userId: testingRequestedUserId);
        
        const int testingProjectId = 1200;
        await _projectBuilder
            .Build(projectId: testingProjectId, ownerProjectId:testingRequestedUserId);
        
        AddProjectMemberDto dto = new AddProjectMemberDto(testingProjectId, 7000);

        //Act
        HttpResponseModule responseModule = ActResponseModule(dto, GetUserJwtToken(testingRequestedUserId));

        //Assert
        responseModule.AssertStatusCode(HttpStatusCode.BadRequest);
        responseModule.AsJson.AssertThat<BaseResponse>(response =>
        {
            response.Success.ShouldBe(false);
            response.Errors.Count.ShouldBe(1);
            response.Errors.First().Code.ShouldBe(BaseErrorCode.MemberForProjectMemberNotExists);
        });
    }

    [Test]
    public async Task
        AddProjectMemberToProject_ProvidedMemberIdIsSameAsOwnerId_BaseErrorCodeShouldBe_UserOperationRestricted()
    {
        //Arrange
        const int testingRequestedUserId = 6500;
        await _userBuilder
            .Build(userId: testingRequestedUserId);
        
        const int testingProjectId = 6200;
        await _projectBuilder
            .Build(projectId: testingProjectId, ownerProjectId:testingRequestedUserId);
        
        AddProjectMemberDto dto = new AddProjectMemberDto(testingProjectId, testingRequestedUserId);

        //Act
        HttpResponseModule responseModule = ActResponseModule(dto, GetUserJwtToken(testingRequestedUserId));

        //Assert
        responseModule.AssertStatusCode(HttpStatusCode.BadRequest);
        responseModule.AsJson.AssertThat<BaseResponse>(response =>
        {
            response.Success.ShouldBe(false);
            response.Errors.Count.ShouldBe(1);
            response.Errors.First().Code.ShouldBe(BaseErrorCode.UserOperationRestricted);
        });
    }
    
    [Test]
    public async Task
        AddProjectMemberToProject_ProvidedMemberIdIsAlreadyInProject_BaseErrorCodeShouldBe_ProjectMemberAlreadyCreated()
    {
        //Arrange
        const int testingRequestedUserId = 6500;
        await _userBuilder
            .Build(userId: testingRequestedUserId);
        
        const int testingProjectId = 6200;
        await _projectBuilder
            .Build(projectId: testingProjectId, ownerProjectId:testingRequestedUserId);

        const int testingMemberId = 8620;
        await _userBuilder
            .Build(userId: testingMemberId);
        
        await _projectMemberBuilder
            .Build(2500, testingProjectId, testingMemberId);
        
        AddProjectMemberDto dto = new AddProjectMemberDto(testingProjectId, testingMemberId);

        //Act
        HttpResponseModule responseModule = ActResponseModule(dto, GetUserJwtToken(testingRequestedUserId));

        //Assert
        responseModule.AssertStatusCode(HttpStatusCode.BadRequest);
        responseModule.AsJson.AssertThat<BaseResponse>(response =>
        {
            response.Success.ShouldBe(false);
            response.Errors.Count.ShouldBe(1);
            response.Errors.First().Code.ShouldBe(BaseErrorCode.ProjectMemberAlreadyCreated);
        });
    }

    private HttpResponseModule ActResponseModule(AddProjectMemberDto dto, string jwtToken)
    {
        return HttpHost
            .Post
            .WithJwtToken(jwtToken)
            .Url(TestingUri)
            .Json(dto)
            .Send()
            .Response;
    }
}