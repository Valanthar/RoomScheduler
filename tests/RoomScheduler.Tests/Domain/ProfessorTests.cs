using FluentAssertions;
using RoomScheduler.Domain.Entities;
using RoomScheduler.Domain.Enums;
using RoomScheduler.Domain.Exceptions;

namespace RoomScheduler.Tests.Domain;

public class ProfessorTests
{
    [Fact]
    public void Create_ShouldSucceed_WithValidData()
    {
        var prof = Professor.Create("Ana Souza", "ana@escola.com", "hash123", UserRole.Professor);

        prof.Id.Should().NotBeEmpty();
        prof.Name.Should().Be("Ana Souza");
        prof.Email.Should().Be("ana@escola.com");
        prof.Role.Should().Be(UserRole.Professor);
        prof.PasswordHash.Should().Be("hash123");
    }

    [Fact]
    public void Create_ShouldNormalizeEmail_ToLowercase()
    {
        var prof = Professor.Create("Carlos Lima", "CARLOS@ESCOLA.COM", "hash123");

        prof.Email.Should().Be("carlos@escola.com");
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    [InlineData("A")]   // too short
    public void Create_ShouldThrow_WhenNameIsInvalid(string name)
    {
        var act = () => Professor.Create(name, "valid@email.com", "hash123");
        act.Should().Throw<DomainException>();
    }

    [Theory]
    [InlineData("")]
    [InlineData("notanemail")]
    public void Create_ShouldThrow_WhenEmailIsInvalid(string email)
    {
        var act = () => Professor.Create("Valid Name", email, "hash123");
        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Create_ShouldThrow_WhenPasswordHashIsEmpty()
    {
        var act = () => Professor.Create("Valid Name", "valid@email.com", "");
        act.Should().Throw<DomainException>()
           .WithMessage("*hash*");
    }
}
