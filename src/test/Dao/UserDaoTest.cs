using Xunit;
using FluentAssertions;
using iLib.src.main.DAO;
using iLib.src.main.Model;

[Collection("DAO Tests")]
public class UserDaoTest : NHibernateTest
{
    private UserDao _userDao = null!;
    private User? _user;

    protected override void Initialize()
    {
        _user = new User
        {
            Name = "Mihail",
            Surname = "Gurzu",
            Email = "myEmail",
            Password = "password",
            Address = "123 Main St",
            TelephoneNumber = "555-1234",
            Role = UserRole.CITIZEN,
            Uuid = Guid.NewGuid().ToString()
        };
        Session.Save(_user);
        _userDao = new UserDao(Session);
    }

    [Fact]
    public void TestFindByIdExistingUser()
    {
        var retrievedUser = _userDao.FindById(_user!.Id);
        retrievedUser.Should().BeEquivalentTo(_user);
    }

    [Fact]
    public void TestFindByIdNonExistingUser()
    {
        var retrievedUser = _userDao.FindById(Guid.NewGuid());
        retrievedUser.Should().BeNull();
    }

    [Fact]
    public void TestSave()
    {
        var userToPersist = new User
        {
            Name = "John",
            Surname = "Doe",
            Email = "john@example.com",
            Password = "password",
            Address = "456 Elm St",
            TelephoneNumber = "555-5678",
            Role = UserRole.CITIZEN,
            Uuid = Guid.NewGuid().ToString()
        };

        _userDao.Save(userToPersist);
        var manuallyRetrievedUser = Session.QueryOver<User>()
                                           .Where(u => u.Uuid == userToPersist.Uuid)
                                           .SingleOrDefault();

        manuallyRetrievedUser.Should().BeEquivalentTo(userToPersist);
    }

    [Fact]
    public void TestUpdate()
    {
        _user!.Name = "Teodor";
        _userDao.Save(_user);

        var manuallyRetrievedUser = Session.QueryOver<User>()
                                           .Where(u => u.Uuid == _user.Uuid)
                                           .SingleOrDefault();

        manuallyRetrievedUser.Should().BeEquivalentTo(_user);
    }

    [Fact]
    public void TestDeleteExistingUser()
    {
        var userToDelete = new User
        {
            Name = "John",
            Surname = "Doe",
            Email = "john@example.com",
            Password = "password",
            Address = "456 Elm St",
            TelephoneNumber = "555-5678",
            Role = UserRole.CITIZEN,
            Uuid = Guid.NewGuid().ToString()
        };
        Session.Save(userToDelete);

        var manuallyRetrievedUsers = Session.QueryOver<User>().List();
        manuallyRetrievedUsers.Should().Contain(userToDelete);

        _userDao.Delete(userToDelete);
        manuallyRetrievedUsers = Session.QueryOver<User>().List();

        manuallyRetrievedUsers.Should().NotContain(userToDelete);
    }

    [Fact]
    public void TestDelete_WhenUserNotExist_ThrowsIllegalArgumentException()
    {
        var tmpUser = new User
        {
            Name = "NonExisting",
            Surname = "User",
            Email = "nonexisting@example.com",
            Password = "password",
            Address = "789 Maple St",
            TelephoneNumber = "555-0000",
            Role = UserRole.CITIZEN,
            Uuid = Guid.NewGuid().ToString()
        };

        Action act = () => _userDao.Delete(tmpUser);
        act.Should().Throw<ArgumentException>().WithMessage("Entity is not persisted!");
    }

    [Fact]
    public void TestFindUsersByEmail()
    {
        var retrievedUser = _userDao.FindUserByEmail("myEmail");
        retrievedUser.Should().BeEquivalentTo(_user);
    }

    [Fact]
    public void TestFindUsersByEmail_WhenNoResult_ThrowsRuntimeException()
    {
        Action act = () => _userDao.FindUserByEmail("nonexisting@example.com");
        act.Should().Throw<InvalidOperationException>().WithMessage("User not found");
    }

    [Fact]
    public void TestFindUsers()
    {
        var userToAdd = new User
        {
            Name = "Mihail",
            Surname = "Another",
            Email = "another@example.com",
            Password = "password",
            Address = "789 Maple St",
            TelephoneNumber = "555-0000",
            Role = UserRole.CITIZEN,
            Uuid = Guid.NewGuid().ToString()
        };
        Session.Save(userToAdd);

        var retrievedUsers = _userDao.FindUsers("Mihail", "inexistingSurname", null, 0, 10);
        retrievedUsers.Should().BeEmpty();

        retrievedUsers = _userDao.FindUsers("Mihail", "Gurzu", null, 0, 10);
        retrievedUsers.Should().ContainSingle()
                      .Which.Should().BeEquivalentTo(_user);

        retrievedUsers = _userDao.FindUsers("Mihail", null, null, 0, 10);
        retrievedUsers.Should().Contain([_user!, userToAdd]);
    }

    [Fact]
    public void TestFindUsersPaginationAndOrder()
    {
        var user2 = new User
        {
            Name = "a",
            Surname = "Doe",
            Email = "a@example.com",
            Password = "password",
            Address = "123 Main St",
            TelephoneNumber = "555-0000",
            Role = UserRole.CITIZEN,
            Uuid = Guid.NewGuid().ToString()
        };
        Session.Save(user2);

        var user3 = new User
        {
            Name = "z",
            Surname = "Smith",
            Email = "z@example.com",
            Password = "password",
            Address = "456 Elm St",
            TelephoneNumber = "555-1111",
            Role = UserRole.CITIZEN,
            Uuid = Guid.NewGuid().ToString()
        };
        Session.Save(user3);

        var retrievedUsersFirstPage = _userDao.FindUsers(null, null, null, 0, 2);
        var retrievedUsersSecondPage = _userDao.FindUsers(null, null, null, 2, 1);

        retrievedUsersFirstPage.Should().HaveCount(2);
        retrievedUsersFirstPage.Should().Contain([_user!, user2]);

        retrievedUsersSecondPage.Should().ContainSingle()
                                .Which.Should().BeEquivalentTo(user3);
    }

    [Fact]
    public void TestCountUsers()
    {
        var user2 = new User
        {
            Name = "a",
            Surname = "Doe",
            Email = "a@example.com",
            Password = "password",
            Address = "123 Main St",
            TelephoneNumber = "555-0000",
            Role = UserRole.CITIZEN,
            Uuid = Guid.NewGuid().ToString()
        };
        Session.Save(user2);

        var user3 = new User
        {
            Name = "z",
            Surname = "Smith",
            Email = "z@example.com",
            Password = "password",
            Address = "456 Elm St",
            TelephoneNumber = "555-1111",
            Role = UserRole.CITIZEN,
            Uuid = Guid.NewGuid().ToString()
        };
        Session.Save(user3);

        var resultsNumber = _userDao.CountUsers(null, null, null);

        resultsNumber.Should().Be(3);
    }
}
