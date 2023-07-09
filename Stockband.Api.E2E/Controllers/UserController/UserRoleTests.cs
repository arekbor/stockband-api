using System.Net;
using FlueFlame.Http.Modules;
using Shouldly;
using Stockband.Api.Dtos.User;
using Stockband.Api.E2E.Builders;
using Stockband.Domain;
using Stockband.Domain.Common;

namespace Stockband.Api.E2E.Controllers.UserController;

[TestFixture]
public class UserRoleTests:BaseTest
{
    private UserBuilder _userBuilder = null!;
    private const string TestingUri = "/user/role";

    [SetUp]
    public void SetUp()
    {
        _userBuilder = new UserBuilder(Context);
    }

    [Test]
    public async Task UserRole_BaseResponse_Success_ShouldBeTrue()
    {
        //Arrange
        const int testingUserId = 45530;
        const UserRole testingUserRole = UserRole.User;
        await _userBuilder
            .Build(userId:testingUserId, userRole: testingUserRole);
        
        const int testingRequestedUserId = 43350;
        const UserRole testingRequestedUserRole = UserRole.Admin;
        await _userBuilder
            .Build(userId:testingRequestedUserId, userRole: testingRequestedUserRole);
        
        UpdateRoleDto dto = new UpdateRoleDto(testingUserId, testingUserRole);

        //Act
        HttpResponseModule responseModule =
            ActResponseModule(dto, GetAdminJwtToken(testingRequestedUserId));
        
        //Assert
        responseModule.AssertStatusCode(HttpStatusCode.OK);
        responseModule.AsJson.AssertThat<BaseResponse>(response =>
        {
            response.Success.ShouldBe(true);
            response.Errors.Count.ShouldBe(0);
        });
    }

    [Test]
    public async Task UserRole_OutsideOfTheUserRoleEnum_BaseErrorCodeShouldBe_FluentValidationCode()
    {
        //Arrange
        const int testingUserId = 45530;
        const UserRole testingUserRole = (UserRole)9000;
        
        UpdateRoleDto dto = new UpdateRoleDto(testingUserId, testingUserRole);
        
        //Act
        HttpResponseModule responseModule =
            ActResponseModule(dto, GetAdminJwtToken(5000));
        
        //Assert
        responseModule.AssertStatusCode(HttpStatusCode.BadRequest);
        responseModule.AsJson.AssertThat<BaseResponse>(response =>
        {
            response.Success.ShouldBe(false);
            response.Errors.Count.ShouldBe(1);
            response.Errors.First().Code.ShouldBe(BaseErrorCode.FluentValidationCode);
        });
    }

    [Test]
    public void UserRole_RequestedUserIsNotAdmin_BaseErrorCodeShouldBe_UserUnauthorizedOperation()
    {
        //Arrange
        const int testingUserId = 3224;
        const UserRole testingUserRole = UserRole.Admin;
        
        UpdateRoleDto dto = new UpdateRoleDto(testingUserId, testingUserRole);

        //Act
        HttpResponseModule responseModule =
            ActResponseModule(dto, GetUserJwtToken(54435));

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
    public async Task UserRole_ProvidedUserNotExists_BaseErrorCodeShouldBe_UserNotExists()
    {
        //Arrange
        const int testingUserId = 5127;
        const UserRole testingUserRole = UserRole.User;

        const int testingRequestedUserId = 5400;
        
        await _userBuilder
            .Build(userId:testingRequestedUserId, userRole: UserRole.Admin);
        
        UpdateRoleDto dto = new UpdateRoleDto(testingUserId, testingUserRole);
        
        //Act
        HttpResponseModule responseModule =
            ActResponseModule(dto, GetAdminJwtToken(testingRequestedUserId));
        
        //Assert
        responseModule.AssertStatusCode(HttpStatusCode.BadRequest);
        responseModule.AsJson.AssertThat<BaseResponse>(response =>
        {
            response.Errors.Count.ShouldBe(1);
            response.Success.ShouldBe(false);
            response.Errors.First().Code.ShouldBe(BaseErrorCode.UserNotExists);
        });
    }
    
    [Test]
    public void UserRole_ProvidedRequestedUserNotExists_BaseErrorCodeShouldBe_RequestedUserNotExists()
    {
        //Arrange
        UpdateRoleDto dto = new UpdateRoleDto(5985, UserRole.Admin);
        
        //Act
        HttpResponseModule responseModule = 
            ActResponseModule(dto, GetAdminJwtToken(8000));
        
        //Assert
        responseModule.AssertStatusCode(HttpStatusCode.BadRequest);
        responseModule.AsJson.AssertThat<BaseResponse>(response =>
        {
            response.Success.ShouldBe(false);
            response.Errors.Count.ShouldBe(1);
            response.Errors.First().Code.ShouldBe(BaseErrorCode.RequestedUserNotExists);
        });
    }

    private HttpResponseModule ActResponseModule
        (UpdateRoleDto dto, string jwtToken)
    {
        return HttpHost
            .Put
            .Url(TestingUri)
            .WithJwtToken(jwtToken)
            .Json(dto)
            .Send()
            .Response;
    }
}