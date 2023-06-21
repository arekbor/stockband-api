using Bogus;
using FizzWare.NBuilder;
using NUnit.Framework;
using Shouldly;
using Stockband.Application.Features.UserFeatures.Queries.GetLoggedUser;
using Stockband.Application.Interfaces.Repositories;
using Stockband.Domain;
using Stockband.Domain.Common;
using Stockband.Domain.Entities;
using Stockband.Domain.Exceptions;
using Stockband.Infrastructure.Repositories;

namespace Stockband.Application.IntegrationTests.Features.UserFeatures.Queries;

public class GetLoggedUserQueryTest:BaseTest
{
    private IUserRepository _userRepository = null!;
    [SetUp]
    public void SetUp()
    {
        _userRepository = new UserRepository(Context);
    }
    
    [Test]
    public async Task GetLoggedUserQuery_ResponseShouldBeSuccess()
    {
        //Arrange
        string testingEmail = new Faker().Person.Email;
        string testingPassword = new Faker().Internet.Password(20);

        string? hashedPassword = BCrypt.Net.BCrypt.HashPassword(testingPassword);
        if (hashedPassword == null)
        {
            throw new ObjectNotFound(nameof(hashedPassword));
        }

        GetLoggedUserQuery query = new GetLoggedUserQuery
        {
            Email = testingEmail,
            Password = testingPassword
        };

        User userForTest = Builder<User>
            .CreateNew()
            .With(x => x.Deleted = false)
            .With(x => x.Role = UserRole.User)
            .With(x => x.Email = testingEmail)
            .With(x => x.Password = hashedPassword)
            .Build();
        await _userRepository.AddAsync(userForTest);

        GetLoggedUserQueryHandler handler = new GetLoggedUserQueryHandler(_userRepository);
        
        //Act
        BaseResponse<GetLoggedUserQueryViewModel> response = await handler.Handle(query, CancellationToken.None);
        
        //Assert
        response.Success.ShouldBe(true);
        response.Errors.Count.ShouldBe(0);
    }

    [Test]
    [TestCaseSource(typeof(UserFeaturesTestDataCases), nameof(UserFeaturesTestDataCases.InvalidEmailOrPasswordCases))]
    public async Task GetLoggedUserQuery_InvalidEmail_ResponseShouldBeNotSuccess
        (string testingEmail, string testingPassword)
    {
        //Arrange
        GetLoggedUserQuery query = new GetLoggedUserQuery
        {
            Email = testingEmail,
            Password = testingPassword
        };
        
        GetLoggedUserQueryHandler handler = new GetLoggedUserQueryHandler(_userRepository);
        
        //Act
        BaseResponse<GetLoggedUserQueryViewModel> response = await handler.Handle(query, CancellationToken.None);
        
        //Assert
        response.Success.ShouldBe(false);
        response.Errors.Count.ShouldBe(1);
        response.Result.ShouldBeNull();
        response.Errors.First().Code.ShouldBe(BaseErrorCode.FluentValidationCode);
    }
}