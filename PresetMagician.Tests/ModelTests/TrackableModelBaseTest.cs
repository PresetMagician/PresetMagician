using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using Catel.Collections;
using FluentAssertions;
using PresetMagician.Tests.TestEntities;
using SharedModels.Collections;
using SharedModels.Extensions;
using Xunit;

namespace PresetMagician.Tests.ModelTests
{
    public class TrackableModelBaseTest
    {
        /// <summary>
        /// Tests system modified properties and ensures that IsUserModified doesn't trigger
        /// </summary>
        [Fact]
        public void TestSystemModifiedProperties()
        {
            var company = new Company();

            company.IsUserModified.Should()
                .BeFalse("The company's IsUserModified should be false because it wasn't modified by the user");

            company.ModifiedProperties = null;

            company.Name = "bla";
            
            company.IsUserModified.Should()
                .BeFalse("The company's IsUserModified still should be false because it wasn't modified by the user");

            company.ModifiedProperties.Should()
                .Contain(nameof(Company.Name), "The name of the company has changed, thus the modified properties should contain 'Name'");

            company.ModifiedProperties = null;
            
            company.IsUserModified.Should()
                .BeFalse("The company's IsUserModified still should be false because it wasn't modified by the user");

            company.ModifiedProperties.Should()
                .BeEmpty("The name of the company has changed, but was reset by ResetModifiedProperties() (as it typically occurs after loading from the DB), thus the modified properties should be empty");
            
            company.Users.Add(new User());
            
            company.IsUserModified.Should()
                .BeFalse("The company's IsUserModified still should be false because it wasn't modified by the user.");

            company.ModifiedProperties.Should()
                .Contain(nameof(Company.Users), "The users property of the company has changed and should contain 'Users'");

            company.ModifiedProperties = null;
            company.Users.First().Name = "bla";

            company.IsUserModified.Should()
                .BeFalse("The company's IsUserModified still should be false because it wasn't modified by the user.");

            company.ModifiedProperties.Should()
                .Contain(nameof(Company.Users), "The users property of the company has changed and should contain 'Users'");

            var oldList = company.Users;
            company.Users = new TrackableCollection<User>();
            company.ModifiedProperties = null;
            
            oldList.RemoveFirst();
            company.ModifiedProperties.Should().BeNullOrEmpty("Modifying a detached list should not modify ModifiedProperties");

            var newUser = new User();
            company.Users.Add(newUser);
            company.Users.Remove(newUser);
            company.ModifiedProperties = null;

            newUser.Name = "foo";
            company.ModifiedProperties.Should().BeNullOrEmpty("Modifying a removed item from a list should not modify ModifiedProperties");

            

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
            var foo = new FastObservableCollection<User>();
            for (int i = 0; i < 10000; i++)
            {
                foo.Add(new User());
            }

        }
        
        [Fact]
        public void TestObservableCollectionSpeed()
        {
            var foo = new ObservableCollection<User>();
            for (int i = 0; i < 10000; i++)
            {
                foo.Add(new User());
            }

        }
        
        [Fact]
        public void TestListSpeed()
        {
            var foo = new List<User>();
            for (int i = 0; i < 10000; i++)
            {
                foo.Add(new User());
            }
        }

        [Fact]
        public void TestTrackedCollectionSpeed()
        {
            var company = new Company();
            for (int i = 0; i < 10000; i++)
            {
                company.Users.Add(new User());
            }
        }

        [Fact]
        public void TestIncrementalUserModified()
        {
            var company = new Company();
            company.ModifiedProperties = null;
            ((IEditableObject) company).BeginEdit();
            company.Name = "schorsch";
            company.EndEdit();

            company.IsUserModified.Should().BeFalse("User modified should be false after edit");
            
            company.BeginEdit();
            company.CancelEdit();
            
            company.IsUserModified.Should().BeFalse("User modified should still be false after edit");
            company.ModifiedProperties.Should().Contain(nameof(company.Name));
            company.ModifiedProperties.Should().NotContain(nameof(company.Users));
            company.UserModifiedProperties.Should().BeNullOrEmpty();
            
            company.BeginEdit();
            company.Users.Add(new User());
            company.ModifiedProperties.Should().Contain(nameof(company.Name));
            company.ModifiedProperties.Should().Contain(nameof(company.Users));
            company.UserModifiedProperties.Should().NotContain(nameof(company.Name));
            company.UserModifiedProperties.Should().Contain(nameof(company.Users));
            company.CancelEdit();

            company.UserModifiedProperties.Should().BeEmpty();
            company.ModifiedProperties.Should().Contain(nameof(company.Name));
            
        }

        [Fact]
        public void TestUserModifiedCollectionProperties()
        {
            
            var firstUser = new User();
            var secondUser = new User();
            var company = new Company();
            company.Users.Add(firstUser);
            company.Users.Add(secondUser);
            
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
                .BeTrue("changing the first user's name should result in the company's users collection to be IsUserModified = true");
            company.IsUserModified.Should()
                .BeTrue("changing the first user's name should result in the company's IsUserModified = true");

            company.ModifiedProperties.Should().Contain(nameof(Company.Users));
            
            company.IsEditing.Should().BeTrue("Company's IsEditing should be true inside of the edit mode ");
            company.Users.IsEditing.Should().BeTrue("Company's user collection IsEditing should be true inside of the edit mode ");
            firstUser.IsEditing.Should().BeTrue("User's IsEditing should be true inside of the edit mode ");

            ((IEditableObject) company).CancelEdit();

            firstUser.Name.Should().NotBe("yups", "reverting the change should also revert the name yups");
            firstUser.IsUserModified.Should()
                .BeFalse("cancelling the edit on the company should result in the user's name to be reverted as well, resulting in IsUserModified = false");
            company.Users.IsUserModified.Should()
                .BeFalse("cancelling the edit on the company should result in the user collection to be IsUserModified = false");
            company.IsUserModified.Should()
                .BeFalse("cancelling the edit on the company should result in the company's IsUserModified = false");

            company.IsEditing.Should().BeFalse("Company's IsEditing should be false outside of the edit mode ");
            company.Users.IsEditing.Should().BeFalse("Company's user collection IsEditing should be false outside of the edit mode ");
            firstUser.IsEditing.Should().BeFalse("User's IsEditing should be false outside of the edit mode ");
            
            company.Users = new TrackableCollection<User>();
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
                .BeFalse("editing an item not in the list should not trigger the company's user colllection IsUserModified = true");
            company.IsUserModified.Should()
                .BeFalse("editing an item not in the list should not trigger the company's IsUserModified = true");
            
            company.Users.Add(firstUser);
            
            firstUser.IsEditing.Should().BeTrue("User's IsEditing should be true when attached to a list which is being edited");
            
            company.Users.Remove(firstUser);
            
            firstUser.IsEditing.Should().BeFalse("User's IsEditing should be false when detached from a list which is being edited");
            
            company.Users.Add(firstUser);
            company.Users[company.Users.IndexOf(firstUser)] = secondUser;
            
            company.Users.Count.Should().Be(1);
            firstUser.IsEditing.Should().BeFalse("First User's IsEditing should be false when it is replaced by another item");
            
            company.Users.Add(firstUser);
            company.Users.Clear();
            
            firstUser.IsEditing.Should().BeFalse("User's IsEditing should be false when the list is cleared");
            
            company.CancelEdit();
            
            company.Users.Add(firstUser);
            firstUser.Company = company;
            firstUser.BeginEdit();
            
            firstUser.IsEditing.Should().BeTrue("User's IsEditing should be true because we initiated editing from there");
            company.IsEditing.Should().BeTrue("The company's IsEditing should be true even if we initiated editing from a child");
            company.Users.IsEditing.Should().BeTrue("The company's user collection IsEditing should be true because we initiated editing from a child");
            
            
            

        }
    }
}