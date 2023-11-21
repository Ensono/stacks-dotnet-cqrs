using TestStack.BDDfy;
using xxAMIDOxx.xxSTACKSxx.CQRS.ApplicationEvents;
using Xunit;
using xxAMIDOxx.xxSTACKSxx.API.ComponentTests.Fixtures;

namespace xxAMIDOxx.xxSTACKSxx.API.ComponentTests.Features;

[Trait("TestType", "ComponentTests")]
public class CreateMenuFeature
{
    /* SCENARIOS: Create a menu

         Examples:
         -----------------------------------------------------------------
        | AsRole              | Menu Condition     | Outcome              |
        |---------------------|--------------------|----------------------|
        | Admin               | Valid Menu         | 201 Resource Create  |
        | Admin               | Invalid Menu       | 400 Bad  Request     |
        | Employee, Customer,                                             |
        | UnauthenticatedUser | Valid Menu         | 403 Forbidden        |

    */

    [Theory, CustomAutoData]
    public void CreateMenuAsAdminForValidMenuShouldSucceed(CreateMenuFixture fixture)
    {
        this.Given(_ => fixture.GivenTheUserIsAnAuthenticatedAdministrator(), "Given the user is authenticated and has the Admin role")
                .And(_ => fixture.GivenAMenuDoesNotExist(), "And the menu does not does not exist")
            .When(_ => fixture.WhenTheMenuCreationIsSubmitted(), "When the menu is submitted")
            .Then(_ => fixture.ThenASuccessfulResponseIsReturned(), "Then a successful response is returned")
                .And(_ => fixture.ThenACreatedResponseIsReturned(), "And the response code is CREATED")
                .And(_ => fixture.ThenTheResourceCreatedResponseIsReturned(), "And the id of the new menu is returned")
                .And(_ => fixture.ThenTheMenuIsSubmittedToDatabase(), "And the menu data is submitted correctly to the database")
                .And(_ => fixture.ThenAMenuCreatedEventIsRaised(), $"And an event of type {nameof(MenuCreatedEvent)} is raised")
            .BDDfy();
    }


    [Theory, CustomAutoData]
    public void CreateMenuAsAdminForInvalidMenuShouldFail(CreateMenuFixture fixture)
    {
        this.Given(_ => fixture.GivenTheUserIsAnAuthenticatedAdministrator(), "Given the user is authenticated and has the Admin role")
                .And(_ => fixture.GivenAInvalidMenu(), "And a valid menu being submitted")
                .And(_ => fixture.GivenAMenuDoesNotExist(), "And the menu does not does not exist")
            .When(_ => fixture.WhenTheMenuCreationIsSubmitted(), "When the menu is submitted")
            .Then(_ => fixture.ThenAFailureResponseIsReturned(), "Then a failure response is returned")
                .And(_ => fixture.ThenTheMenuIsNotSubmittedToDatabase(), "And the menu is not submitted to the database")
                .And(_ => fixture.ThenAMenuCreatedEventIsNotRaised(), $"And an event of type {nameof(MenuCreatedEvent)} should not be raised")
            .BDDfy();
    }


    [Theory(Skip = "Only works when Auth is implemented")]
    [CustomInlineAutoData("Employee")]
    [CustomInlineAutoData("Customer")]
    [CustomInlineAutoData("UnauthenticatedUser")]
    public void CreateMenuAsNonAdminForValidMenuShouldFail(string role, CreateMenuFixture fixture)
    {
        this.Given(_ => fixture.GivenTheUserIsAuthenticatedAndHasRole(role), $"Given the user is authenticated and has the {role} role")
                .And(_ => fixture.GivenAMenuDoesNotExist(), "And the menu does not does not exist")
            .When(_ => fixture.WhenTheMenuCreationIsSubmitted(), "When the menu is submitted")
            .Then(_ => fixture.ThenAForbiddenResponseIsReturned(), "Then a Forbidden response is returned")
                .And(_ => fixture.ThenTheMenuIsNotSubmittedToDatabase(), "And the menu is not submitted to the database")
                .And(_ => fixture.ThenAMenuCreatedEventIsNotRaised(), $"And an event of type {nameof(MenuCreatedEvent)} should not be raised")
            .BDDfy();
    }
}
