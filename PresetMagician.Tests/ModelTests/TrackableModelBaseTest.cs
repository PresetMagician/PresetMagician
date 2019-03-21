using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using Catel.Collections;
using FluentAssertions;
using PresetMagician.Core.Collections;
using PresetMagician.Tests.TestEntities;
using Xunit;
using Xunit.Abstractions;

namespace PresetMagician.Tests.ModelTests
{
    public class ModelBaseTest : BaseTest
    {
        public ModelBaseTest(ITestOutputHelper output, DataFixture fixture) : base(output, fixture)
        {
        }

        [Fact]
        public void TestUserModifiedProperties()
        {
            var company = new Company();

            company.IsUserModified.Should()
                .BeFalse("No modification outside of the edit mode should result in IsUserModified to be false");
            ((IEditableObject) company).BeginEdit();
            company.Name = "bla";
            company.IsUserModified.Should()
                .BeTrue("Modifying the company name while in edit mode should cause IsUserModified to be true");

            company.Name = "";
            company.IsUserModified.Should()
                .BeFalse("Reverting the change to the company name should result in IsUserModified to be false");

            company.Name = "bla";
            ((IEditableObject) company).CancelEdit();

            company.IsUserModified.Should()
                .BeFalse("Cancelling edit should result in IsUserModified to be false");
        }

        [Fact]
        public void TestFastObservableCollectionSpeed()
        {
            var foo = new FastObservableCollection<Company>();
            for (int i = 0; i < 10000; i++)
            {
                foo.Add(new Company());
            }
        }

        [Fact]
        public void TestObservableCollectionSpeed()
        {
            var foo = new ObservableCollection<Company>();
            for (int i = 0; i < 10000; i++)
            {
                foo.Add(new Company());
            }
        }

        [Fact]
        public void TestListSpeed()
        {
            var foo = new List<Company>();
            for (int i = 0; i < 10000; i++)
            {
                foo.Add(new Company());
            }
        }

        [Fact]
        public void TestTrackedCollectionSpeed()
        {
            var foo = new EditableCollection<Company>();
            for (int i = 0; i < 10000; i++)
            {
                foo.Add(new Company());
            }
        }

        [Fact]
        public void TestTrackedCollectionSpeedWithUsers()
        {
            var company = new Company();
            for (int i = 0; i < 10000; i++)
            {
                company.Users.Add(new User());
            }
        }

        [Fact]
        public void TestTrackedCollectionSpeedWithUsersAndBeginEdit()
        {
            var company = new Company();
            for (int i = 0; i < 10000; i++)
            {
                company.Users.Add(new User());
            }

            company.BeginEdit();
            company.CancelEdit();
        }

        [Fact]
        public void TestIncrementalUserModified()
        {
            var company = new Company();
            ((IEditableObject) company).BeginEdit();
            company.Name = "schorsch";
            company.EndEdit();

            company.IsUserModified.Should().BeFalse("User modified should be false after edit");

            company.BeginEdit();
            company.CancelEdit();

            company.IsUserModified.Should().BeFalse("User modified should still be false after edit");
            company.UserModifiedProperties.Should().BeNullOrEmpty();

            company.BeginEdit();
            company.Users.Add(new User());
            company.UserModifiedProperties.Should().NotContain(nameof(company.Name));
            company.UserModifiedProperties.Should().Contain(nameof(company.Users));
            company.CancelEdit();

            company.UserModifiedProperties.Should().BeEmpty();
        }

        [Fact]
        public void TestUserModifiedCollectionProperties()
        {
            var firstUser = new User();
            var secondUser = new User();
            var company = new Company();
            company.Users.Add(firstUser);
            company.Users.Add(secondUser);

            var initialCompanyName = "schorsch";
            company.Name = initialCompanyName;

            ((IEditableObject) company).BeginEdit();
            company.Name = "hans";
            company.IsUserModified.Should()
                .BeTrue("Changing the company name should result in IsUserModified to be true");
            company.Name = initialCompanyName;
            company.IsUserModified.Should()
                .BeFalse("Reverting the company name should result in IsUserModified to be false");
            ((IEditableObject) company).CancelEdit();

            ((IEditableObject) company).BeginEdit();
            company.Users.Remove(firstUser);
            company.IsUserModified.Should()
                .BeTrue("Removing a user from the company's users should result in IsUserModified to be true");

            company.Users.Add(firstUser);
            company.IsUserModified.Should()
                .BeFalse("Adding the user again to the company's users should result in IsUserModified to be false");

            company.Users.Remove(secondUser);
            company.IsUserModified.Should()
                .BeTrue("Removing the second user from the company's users should result in IsUserModified to be true");

            ((IEditableObject) company).CancelEdit();
            company.IsUserModified.Should()
                .BeFalse("Cancelling the edit should result in IsUserModified to be false");

            ((IEditableObject) company).BeginEdit();

            company.IsUserModified.Should()
                .BeFalse("Calling BeginEdit after CancelEdit should result in IsUserModified = false");

            firstUser.Name = "yups";

            firstUser.IsUserModified.Should()
                .BeTrue("changing the first user's name should result in the user's IsUserModified = true");
            company.Users.IsUserModified.Should()
                .BeTrue(
                    "changing the first user's name should result in the company's users collection to be IsUserModified = true");
            company.IsUserModified.Should()
                .BeTrue("changing the first user's name should result in the company's IsUserModified = true");

            company.IsEditing.Should().BeTrue("Company's IsEditing should be true inside of the edit mode ");
            company.Users.IsEditing.Should()
                .BeTrue("Company's user collection IsEditing should be true inside of the edit mode ");
            firstUser.IsEditing.Should().BeTrue("User's IsEditing should be true inside of the edit mode ");

            ((IEditableObject) company).CancelEdit();

            firstUser.Name.Should().NotBe("yups", "reverting the change should also revert the name yups");
            firstUser.IsUserModified.Should()
                .BeFalse(
                    "cancelling the edit on the company should result in the user's name to be reverted as well, resulting in IsUserModified = false");
            company.Users.IsUserModified.Should()
                .BeFalse(
                    "cancelling the edit on the company should result in the user collection to be IsUserModified = false");
            company.IsUserModified.Should()
                .BeFalse("cancelling the edit on the company should result in the company's IsUserModified = false");

            company.IsEditing.Should().BeFalse("Company's IsEditing should be false outside of the edit mode ");
            company.Users.IsEditing.Should()
                .BeFalse("Company's user collection IsEditing should be false outside of the edit mode ");
            firstUser.IsEditing.Should().BeFalse("User's IsEditing should be false outside of the edit mode ");

            company.Users = new EditableCollection<User>();
            company.Users.Add(firstUser);
            company.Users.Remove(firstUser);

            company.BeginEdit();
            firstUser.Name = "dingdong";

            company.IsEditing.Should().BeTrue("Company's IsEditing should be true in edit mode ");
            company.Users.IsEditing.Should().BeTrue("Company's user collection IsEditing should be true in edit mode ");
            firstUser.IsEditing.Should().BeFalse("User's IsEditing should be false when not attached to a list");

            firstUser.IsUserModified.Should()
                .BeFalse("editing the user should not trigger IsUserModified = true because it's not in edit mode");
            company.Users.IsUserModified.Should()
                .BeFalse(
                    "editing an item not in the list should not trigger the company's user colllection IsUserModified = true");
            company.IsUserModified.Should()
                .BeFalse("editing an item not in the list should not trigger the company's IsUserModified = true");

            company.Users.Add(firstUser);

            firstUser.IsEditing.Should()
                .BeTrue("User's IsEditing should be true when attached to a list which is being edited");

            company.Users.Remove(firstUser);

            firstUser.IsEditing.Should()
                .BeFalse("User's IsEditing should be false when detached from a list which is being edited");

            company.Users.Add(firstUser);
            company.Users[company.Users.IndexOf(firstUser)] = secondUser;

            company.Users.Count.Should().Be(1);
            firstUser.IsEditing.Should()
                .BeFalse("First User's IsEditing should be false when it is replaced by another item");

            company.Users.Add(firstUser);
            company.Users.Clear();

            firstUser.IsEditing.Should().BeFalse("User's IsEditing should be false when the list is cleared");

            company.CancelEdit();

            company.Users.Add(firstUser);
            firstUser.Company = company;
            firstUser.BeginEdit();

            firstUser.IsEditing.Should()
                .BeTrue("User's IsEditing should be true because we initiated editing from there");
            company.IsEditing.Should()
                .BeTrue("The company's IsEditing should be true even if we initiated editing from a child");
            company.Users.IsEditing.Should()
                .BeTrue(
                    "The company's user collection IsEditing should be true because we initiated editing from a child");
        }
    }
}