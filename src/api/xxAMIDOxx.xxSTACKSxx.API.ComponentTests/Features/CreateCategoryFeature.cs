using TestStack.BDDfy;
using Xunit;
using xxAMIDOxx.xxSTACKSxx.API.ComponentTests.Fixtures;

namespace xxAMIDOxx.xxSTACKSxx.API.ComponentTests.Features;

[Trait("TestType", "ComponentTests")]
public class CreateCategoryFeature
{
    /* SCENARIOS: Create a category in the menu

         Examples:
         ---------------------------------------------------------------------------------
        | AsRole              | Existing Menu | Existing Category  | Outcome              |
        |---------------------|---------------|--------------------|----------------------|
        | Admin               | Yes           | No                 | 200 OK               |
        | Employee            | Yes           | No                 | 200 OK               |
        | Admin               | No            | No                 | 404 Not Found        |
        | Admin               | Yes           | Yes                | 409 Conflict         |
        | Customer            | Yes           | No                 | 403 Forbidden        |
        | UnauthenticatedUser | Yes           | No                 | 403 Forbidden        |

    */

    [Theory]
    [CustomInlineAutoData("Admin")]
    [CustomInlineAutoData("Employee")]
    public void CreateCategoryShouldSucceed(string role, CreateCategoryFixture fixture)
    {
        this.Given(_ => fixture.GivenTheUserIsAuthenticatedAndHasRole(role), $"Given the user is authenticated and has the {role} role")
                .And(_ => fixture.GivenAnExistingMenu(), "And an existing menu")
                .And(_ => fixture.GivenTheMenuBelongsToUserRestaurant(), "And the menu belongs to the user restaurant")
                .And(_ => fixture.GivenTheCategoryDoesNotExist(), "And the category being created does not exist in the menu")
            .When(_ => fixture.WhenTheCategoryIsSubmitted(), "When a new category is submitted")
            .Then(_ => fixture.ThenASuccessfulResponseIsReturned(), "Then a successful response is returned")
                .And(_ => fixture.ThenMenuIsLoadedFromStorage(), "And the menu is loaded from the storage")
                .And(_ => fixture.ThenTheResourceCreatedResponseIsReturned(), "And the id of the new category is returned")
                .And(_ => fixture.ThenTheCategoryIsAddedToMenu(), "And the category is added to the menu")
                .And(_ => fixture.ThenTheMenuShouldBePersisted(), "And the menu is persisted to the storage")
                .And(_ => fixture.ThenAMenuUpdatedEventIsRaised(), "And the event MenuUpdate is Raised")
                .And(_ => fixture.ThenACategoryCreatedEventIsRaised(), "And the event CategoryCreated is Raised.")
            .BDDfy();
    }


    [Theory]
    [CustomInlineAutoData("Admin")]
    [CustomInlineAutoData("Employee")]
    public void CreateCategoryShouldFailWhenMenuDoesNotExist(string role, CreateCategoryFixture fixture)
    {
        this.Given(_ => fixture.GivenTheUserIsAuthenticatedAndHasRole(role), $"Given the user is authenticated and has the {role} role")
                .And(_ => fixture.GivenAMenuDoesNotExist(), "And a menu does not exist")
            .When(_ => fixture.WhenTheCategoryIsSubmitted(), "When a new category is submitted")
            .Then(_ => fixture.ThenAFailureResponseIsReturned(), "Then a failure response is returned")
                .And(_ => fixture.ThenANotFoundResponseIsReturned(), "And the response code is NotFound")
                .And(_ => fixture.ThenTheMenuShouldNotBePersisted(), "And the menu is not persisted to the storage")
                .And(_ => fixture.ThenAMenuUpdatedEventIsNotRaised(), "And the event MenuUpdate should NOT be raised")
                .And(_ => fixture.ThenACategoryCreatedEventIsNotRaised(), "And the event CategoryCreated should not be Raised")
            .BDDfy();
    }


    [Theory]
    [CustomInlineAutoData("Admin")]
    [CustomInlineAutoData("Employee")]
    public void CreateCategoryShouldFailWhenCategoryAlreadyExists(string role, CreateCategoryFixture fixture)
    {
        this.Given(_ => fixture.GivenTheUserIsAuthenticatedAndHasRole(role), $"Given the user is authenticated and has the {role} role")
                .And(_ => fixture.GivenAnExistingMenu(), "And an existing menu")
                .And(_ => fixture.GivenTheMenuBelongsToUserRestaurant(), "And the menu belongs to the user restaurant")
                .And(_ => fixture.GivenTheCategoryAlreadyExist(), "And the category being created already exist in the menu")
            .When(_ => fixture.WhenTheCategoryIsSubmitted(), "When a new category is submitted")
            .Then(_ => fixture.ThenAFailureResponseIsReturned(), "Then a failure response is returned")
                .And(_ => fixture.ThenAConflictResponseIsReturned(), "And the response code is Conflict")
                .And(_ => fixture.ThenTheMenuShouldNotBePersisted(), "And the menu is NOT persisted to the storage")
                .And(_ => fixture.ThenAMenuUpdatedEventIsNotRaised(), "And the event MenuUpdate should NOT be raised")
                .And(_ => fixture.ThenACategoryCreatedEventIsNotRaised(), "And the event CategoryCreated is NOT Raised")
            .BDDfy();
    }


    [Theory(Skip = "Disabled until security is implemented")]
    [CustomInlineAutoData("Customer")]
    [CustomInlineAutoData("UnauthenticatedUser")]
    public void CreateCategoryShouldFailWithForbidden(string role, CreateCategoryFixture fixture)
    {
        this.Given(_ => fixture.GivenTheUserIsAuthenticatedAndHasRole(role), $"Given the user is authenticated and has the {role} role")
                .And(_ => fixture.GivenAnExistingMenu(), "And an existing menu")
                .And(_ => fixture.GivenTheCategoryDoesNotExist(), "And the category being created does not exist in the menu")
            .When(_ => fixture.WhenTheCategoryIsSubmitted(), "When a new category is submitted")
            .Then(_ => fixture.ThenAForbiddenResponseIsReturned(), "Then a Forbidden response is returned")
                .And(_ => fixture.ThenTheMenuShouldNotBePersisted(), "And the menu is not persisted to the storage")
                .And(_ => fixture.ThenAMenuUpdatedEventIsNotRaised(), "And the event MenuUpdate is NOT Raised")
                .And(_ => fixture.ThenACategoryCreatedEventIsNotRaised(), "And the event CategoryCreated is NOT Raised")
            .BDDfy();
    }


    [Theory(Skip = "Disabled until security is implemented")]
    [CustomInlineAutoData("Admin")]
    [CustomInlineAutoData("Employee")]
    public void CreateCategoryShouldFailWhenMenuDoesNotBelongToUser(string role, CreateCategoryFixture fixture)
    {
        this.Given(_ => fixture.GivenTheUserIsAuthenticatedAndHasRole(role), $"Given the user is authenticated and has the {role} role")
                .And(_ => fixture.GivenAnExistingMenu(), "And an existing menu")
                .And(_ => fixture.GivenTheMenuDoesNotBelongToUserRestaurant(), "And the menu does not belong to users restaurant")
            .When(_ => fixture.WhenTheCategoryIsSubmitted(), "When a new category is submitted")
            .Then(_ => fixture.ThenAFailureResponseIsReturned(), "Then a failure response is returned")
                .And(_ => fixture.ThenANotFoundResponseIsReturned(), "And the response code is NotFound")
                .And(_ => fixture.ThenMenuIsLoadedFromStorage(), "And the menu is loaded from the storage")
                .And(_ => fixture.ThenTheMenuShouldNotBePersisted(), "And the menu is not persisted to the storage")
                .And(_ => fixture.ThenAMenuUpdatedEventIsNotRaised(), "And the event MenuUpdate should not be Raised")
                .And(_ => fixture.ThenACategoryCreatedEventIsNotRaised(), "And the event CategoryCreated should not be Raised")
            .BDDfy();
    }
}
