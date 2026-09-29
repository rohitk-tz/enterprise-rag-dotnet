using Application.Users.Commands.CreateUserFromWebhook;
using FluentAssertions;
using Xunit;

namespace Application.Tests;

public class CreateUserFromWebhookCommandValidatorTests
{
    private readonly CreateUserFromWebhookCommandValidator _validator = new();

    [Fact]
    public void Fails_when_event_type_is_not_user_created()
    {
        var command = new CreateUserFromWebhookCommand("user.updated", "user_2abc");

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void Fails_when_clerk_id_is_missing()
    {
        var command = new CreateUserFromWebhookCommand("user.created", null);

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void Passes_for_a_valid_user_created_event()
    {
        var command = new CreateUserFromWebhookCommand("user.created", "user_2abc");

        var result = _validator.Validate(command);

        result.IsValid.Should().BeTrue();
    }
}
