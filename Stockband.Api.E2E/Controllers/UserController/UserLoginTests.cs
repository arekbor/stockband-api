using System.Net;
using FlueFlame.Http.Modules;
using Shouldly;
using Stockband.Api.E2E.Builders;
using Stockband.Application.Features.UserFeatures.Commands.LoginUser;
using Stockband.Domain;
using Stockband.Domain.Common;

namespace Stockband.Api.E2E.Controllers.UserController;

[TestFixture]
public class UserLoginTests:BaseTest
{
    private const string TestingUri = "/user/login";
    private UserBuilder _userBuilder = null!;

    [SetUp]
    public void SetUp()
    {
        _userBuilder = new UserBuilder(Context);
    }

    [Test]
    public async Task UserLogin_BaseResponse_Success_ShouldBeTrue()
    {
        //Arrange
        const string testingEmail = "test@gmail.com";
        const string testingPassword = "AbcDf@#!1233";

        await _userBuilder
            .Build(userId:500, email:testingEmail, password: testingPassword);

        LoginUserCommand command = new LoginUserCommand
            (testingEmail, testingPassword);
        
        //Act
        HttpResponseModule responseModule = ActResponseModule(command);

        //Assert
        responseModule.AssertStatusCode(HttpStatusCode.OK);

        responseModule.AsJson.AssertThat<BaseResponse>(response =>
        {
            response.Errors.Count.ShouldBe(0);
            response.Success.ShouldBe(true);
        });
    }

    [Test]
    [TestCase("test@gmail.com","test_wrong@com.pl","Addds12@3#1d","Addds12@3#1d")]
    [TestCase("test@gmail.com","test@gmail.com","Addds12@3#1d","test_wrong@3#1d")]
    public async Task UserLogin_WrongEmailOrPassword_BaseErrorCodeShouldBe_WrongEmailOrPasswordLogin
        (string emailCreate, string emailLogin, string passwordCreate, string passwordLogin)
    {
        //Arrange
        await _userBuilder
            .Build(userId:500, email:emailCreate, password: passwordCreate);
        
        LoginUserCommand command = new LoginUserCommand
            (emailLogin, passwordLogin);
        
        //Act
        HttpResponseModule responseModule = ActResponseModule(command);
        
        //Assert
        responseModule.AssertStatusCode(HttpStatusCode.BadRequest);

        responseModule.AsJson.AssertThat<BaseResponse>(response =>
        {
            response.Errors.Count.ShouldBe(1);
            response.Success.ShouldBe(false);
            response.Errors.First().Code.ShouldBe(BaseErrorCode.WrongEmailOrPasswordLogin);
        });
    }

    [Test]
    [TestCase("", "Test@3@#123")]
    [TestCase("test@gmail.com", "")]
    public void UserLogin_EmptyEmailOrPassword_BaseErrorCodeShouldBe_FluentValidationCode
        (string email, string password)
    {
        //Arrange
        LoginUserCommand command = new LoginUserCommand
            (email, password);
        
        //Act
        HttpResponseModule responseModule = ActResponseModule(command);
        
        //Assert
        responseModule.AssertStatusCode(HttpStatusCode.BadRequest);
        
        responseModule.AsJson.AssertThat<BaseResponse>(response =>
        {
            response.Errors.Count.ShouldBe(1);
            response.Success.ShouldBe(false);
            response.Errors.First().Code.ShouldBe(BaseErrorCode.FluentValidationCode);
        });
    }
    
    private HttpResponseModule ActResponseModule(LoginUserCommand command)
    {
        return HttpHost
            .Post
            .Url(TestingUri)
            .Json(command)
            .Send()
            .Response;
    }
}