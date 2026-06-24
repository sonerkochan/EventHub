using EventHub.Areas.Admin.Controllers;
using EventHub.Core.Contracts;
using EventHub.Core.Models.User;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace EventHub.Tests.Unit.Admin;

[Trait("Category", "Unit")]
public class UsersControllerTests
    {
        [Fact]
        public async Task Index_WhenNoRoleFilter_ReturnsViewWithUsers()
        {
            var users = new List<UserListViewModel>
        {
            new()
            {
                Id = "user-id",
                UserName = "admin-user",
                Email = "admin@example.com",
                IsActive = true,
                Roles = ["Admin"]
            }
        };
            var userService = new Mock<IUserService>();
            userService
                .Setup(s => s.GetAllUsersAsync(null))
                .ReturnsAsync(users);
            var controller = new UsersController(userService.Object);

            var result = await controller.Index();

            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<IEnumerable<UserListViewModel>>(viewResult.Model);
            Assert.Same(users, model);
            Assert.Null(controller.ViewBag.RoleFilter);
            userService.Verify(s => s.GetAllUsersAsync(null), Times.Once);
        }
    

    
    [Fact]
        public async Task Index_ReturnsViewWithUsersAndStoresRoleFilter()
        {
            const string role = "Client";
            var users = new List<UserListViewModel>
        {
            new()
            {
                Id = "user-id",
                UserName = "client-user",
                Email = "client@example.com",
                IsActive = true,
                Roles = ["Client"]
            }
        };
            var userService = new Mock<IUserService>();
            userService
                .Setup(s => s.GetAllUsersAsync(role))
                .ReturnsAsync(users);
            var controller = new UsersController(userService.Object);

            var result = await controller.Index(role);

            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<IEnumerable<UserListViewModel>>(viewResult.Model);
            Assert.Same(users, model);
            Assert.Equal(role, controller.ViewBag.RoleFilter);
            userService.Verify(s => s.GetAllUsersAsync(role), Times.Once);
        }

        [Fact]
        public void CreatePartial_ReturnsCreateModalPartialWithCreateUserViewModel()
        {
            var userService = new Mock<IUserService>();
            var controller = new UsersController(userService.Object);

            var result = controller.CreatePartial();

            var partial = Assert.IsType<PartialViewResult>(result);
            Assert.Equal("_CreateModal", partial.ViewName);
            Assert.IsType<CreateUserViewModel>(partial.Model);
            userService.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task CreateUser_WhenModelStateIsInvalid_ReturnsCreateModalPartialAndDoesNotCallService()
        {
            var model = CreateCreateUserViewModel();
            var userService = new Mock<IUserService>();
            var controller = new UsersController(userService.Object);
            controller.ModelState.AddModelError("Email", "Required");

            var result = await controller.Create(model);

            var partial = Assert.IsType<PartialViewResult>(result);
            Assert.Equal("_CreateModal", partial.ViewName);
            Assert.Same(model, partial.Model);
            userService.Verify(s => s.CreateUserAsync(It.IsAny<CreateUserViewModel>()), Times.Never);
        }

        [Fact]
        public async Task CreateUser_WhenServiceSucceeds_ReturnsJsonSuccess()
        {
            var model = CreateCreateUserViewModel();
            var userService = new Mock<IUserService>();
            userService
                .Setup(s => s.CreateUserAsync(model))
                .ReturnsAsync((true, null));
            var controller = new UsersController(userService.Object);

            var result = await controller.Create(model);

            var json = Assert.IsType<JsonResult>(result);
            Assert.True(ReadSuccessProperty(json.Value));
            userService.Verify(s => s.CreateUserAsync(model), Times.Once);
        }

        [Fact]
        public async Task CreateUser_WhenServiceFails_ReturnsCreateModalPartialWithModelError()
        {
            const string error = "A user with that username already exists.";
            var model = CreateCreateUserViewModel();
            var userService = new Mock<IUserService>();
            userService
                .Setup(s => s.CreateUserAsync(model))
                .ReturnsAsync((false, error));
            var controller = new UsersController(userService.Object);

            var result = await controller.Create(model);

            var partial = Assert.IsType<PartialViewResult>(result);
            Assert.Equal("_CreateModal", partial.ViewName);
            Assert.Same(model, partial.Model);
            Assert.False(controller.ModelState.IsValid);
            Assert.Contains(controller.ModelState[string.Empty]!.Errors, e => e.ErrorMessage == error);
            userService.Verify(s => s.CreateUserAsync(model), Times.Once);
        }

        [Fact]
        public async Task EditPartial_WhenUserExists_ReturnsEditModalPartialWithModel()
        {
            const string userId = "user-id";
            var model = CreateEditUserViewModel(userId);
            var userService = new Mock<IUserService>();
            userService
                .Setup(s => s.GetForEditAsync(userId))
                .ReturnsAsync(model);
            var controller = new UsersController(userService.Object);

            var result = await controller.EditPartial(userId);

            var partial = Assert.IsType<PartialViewResult>(result);
            Assert.Equal("_EditModal", partial.ViewName);
            Assert.Same(model, partial.Model);
            userService.Verify(s => s.GetForEditAsync(userId), Times.Once);
        }

        [Fact]
        public async Task EditPartial_WhenUserDoesNotExist_ReturnsNotFound()
        {
            const string userId = "missing-user-id";
            var userService = new Mock<IUserService>();
            userService
                .Setup(s => s.GetForEditAsync(userId))
                .ReturnsAsync((EditUserViewModel?)null);
            var controller = new UsersController(userService.Object);

            var result = await controller.EditPartial(userId);

            Assert.IsType<NotFoundResult>(result);
            userService.Verify(s => s.GetForEditAsync(userId), Times.Once);
        }

        [Fact]
        public async Task EditUser_WhenModelStateIsInvalid_ReturnsEditModalPartialAndDoesNotCallService()
        {
            var model = CreateEditUserViewModel("user-id");
            var userService = new Mock<IUserService>();
            var controller = new UsersController(userService.Object);
            controller.ModelState.AddModelError("Email", "Required");

            var result = await controller.Edit(model);

            var partial = Assert.IsType<PartialViewResult>(result);
            Assert.Equal("_EditModal", partial.ViewName);
            Assert.Same(model, partial.Model);
            userService.Verify(s => s.UpdateUserAsync(It.IsAny<EditUserViewModel>()), Times.Never);
        }

        [Fact]
        public async Task EditUser_WhenServiceSucceeds_ReturnsJsonSuccess()
        {
            var model = CreateEditUserViewModel("user-id");
            var userService = new Mock<IUserService>();
            userService
                .Setup(s => s.UpdateUserAsync(model))
                .ReturnsAsync((true, null));
            var controller = new UsersController(userService.Object);

            var result = await controller.Edit(model);

            var json = Assert.IsType<JsonResult>(result);
            Assert.True(ReadSuccessProperty(json.Value));
            userService.Verify(s => s.UpdateUserAsync(model), Times.Once);
        }

        [Fact]
        public async Task EditUser_WhenServiceFails_ReturnsEditModalPartialWithModelError()
        {
            const string error = "A user with that email already exists.";
            var model = CreateEditUserViewModel("user-id");
            var userService = new Mock<IUserService>();
            userService
                .Setup(s => s.UpdateUserAsync(model))
                .ReturnsAsync((false, error));
            var controller = new UsersController(userService.Object);

            var result = await controller.Edit(model);

            var partial = Assert.IsType<PartialViewResult>(result);
            Assert.Equal("_EditModal", partial.ViewName);
            Assert.Same(model, partial.Model);
            Assert.False(controller.ModelState.IsValid);
            Assert.Contains(controller.ModelState[string.Empty]!.Errors, e => e.ErrorMessage == error);
            userService.Verify(s => s.UpdateUserAsync(model), Times.Once);
        }

        [Fact]
        public async Task DetailsPartial_WhenUserExists_ReturnsDetailsModalPartialWithModel()
        {
            const string userId = "user-id";
            var user = new UserDetailViewModel
            {
                Id = userId,
                UserName = "client-user",
                Email = "client@example.com",
                IsActive = true
            };
            var userService = new Mock<IUserService>();
            userService
                .Setup(s => s.GetUserByIdAsync(userId))
                .ReturnsAsync(user);
            var controller = new UsersController(userService.Object);

            var result = await controller.DetailsPartial(userId);

            var partial = Assert.IsType<PartialViewResult>(result);
            Assert.Equal("_DetailsModal", partial.ViewName);
            Assert.Same(user, partial.Model);
            userService.Verify(s => s.GetUserByIdAsync(userId), Times.Once);
        }

        [Fact]
        public async Task DetailsPartial_WhenUserDoesNotExist_ReturnsNotFound()
        {
            const string userId = "missing-user-id";
            var userService = new Mock<IUserService>();
            userService
                .Setup(s => s.GetUserByIdAsync(userId))
                .ReturnsAsync((UserDetailViewModel?)null);
            var controller = new UsersController(userService.Object);

            var result = await controller.DetailsPartial(userId);

            Assert.IsType<NotFoundResult>(result);
            userService.Verify(s => s.GetUserByIdAsync(userId), Times.Once);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task DeactivateUser_ReturnsJsonWithServiceResult(bool serviceResult)
        {
            const string userId = "user-id";
            var userService = new Mock<IUserService>();
            userService
                .Setup(s => s.DeactivateUserAsync(userId))
                .ReturnsAsync(serviceResult);
            var controller = new UsersController(userService.Object);

            var result = await controller.Deactivate(userId);

            var json = Assert.IsType<JsonResult>(result);
            Assert.Equal(serviceResult, ReadSuccessProperty(json.Value));
            userService.Verify(s => s.DeactivateUserAsync(userId), Times.Once);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task ReactivateUser_ReturnsJsonWithServiceResult(bool serviceResult)
        {
            const string userId = "user-id";
            var userService = new Mock<IUserService>();
            userService
                .Setup(s => s.ReactivateUserAsync(userId))
                .ReturnsAsync(serviceResult);
            var controller = new UsersController(userService.Object);

            var result = await controller.Reactivate(userId);

            var json = Assert.IsType<JsonResult>(result);
            Assert.Equal(serviceResult, ReadSuccessProperty(json.Value));
            userService.Verify(s => s.ReactivateUserAsync(userId), Times.Once);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task DeleteUser_ReturnsJsonWithServiceResult(bool serviceResult)
        {
            const string userId = "user-id";
            var userService = new Mock<IUserService>();
            userService
                .Setup(s => s.DeleteUserAsync(userId))
                .ReturnsAsync(serviceResult);
            var controller = new UsersController(userService.Object);

            var result = await controller.Delete(userId);

            var json = Assert.IsType<JsonResult>(result);
            Assert.Equal(serviceResult, ReadSuccessProperty(json.Value));
            userService.Verify(s => s.DeleteUserAsync(userId), Times.Once);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task AddRole_ReturnsJsonWithServiceResult(bool serviceResult)
        {
            const string userId = "user-id";
            const string role = "Organizer";
            var userService = new Mock<IUserService>();
            userService
                .Setup(s => s.AddRoleToUserAsync(userId, role))
                .ReturnsAsync(serviceResult);
            var controller = new UsersController(userService.Object);

            var result = await controller.AddRole(userId, role);

            var json = Assert.IsType<JsonResult>(result);
            Assert.Equal(serviceResult, ReadSuccessProperty(json.Value));
            userService.Verify(s => s.AddRoleToUserAsync(userId, role), Times.Once);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task RemoveRole_ReturnsJsonWithServiceResult(bool serviceResult)
        {
            const string userId = "user-id";
            const string role = "Organizer";
            var userService = new Mock<IUserService>();
            userService
                .Setup(s => s.RemoveRoleFromUserAsync(userId, role))
                .ReturnsAsync(serviceResult);
            var controller = new UsersController(userService.Object);

            var result = await controller.RemoveRole(userId, role);

            var json = Assert.IsType<JsonResult>(result);
            Assert.Equal(serviceResult, ReadSuccessProperty(json.Value));
            userService.Verify(s => s.RemoveRoleFromUserAsync(userId, role), Times.Once);
        }

        private static CreateUserViewModel CreateCreateUserViewModel()
            => new()
            {
                UserName = "client-user",
                Email = "client@example.com",
                FirstName = "Client",
                LastName = "User",
                PhoneNumber = "123456",
                Password = "Password123",
                ConfirmPassword = "Password123",
                Role = "Client"
            };

        private static EditUserViewModel CreateEditUserViewModel(string id)
            => new()
            {
                Id = id,
                Email = "client@example.com",
                FirstName = "Client",
                LastName = "User",
                PhoneNumber = "123456",
                IsActive = true
            };

        private static bool ReadSuccessProperty(object? value)
        {
            Assert.NotNull(value);
            var property = value.GetType().GetProperty("success");
            Assert.NotNull(property);
            return Assert.IsType<bool>(property.GetValue(value));
        }
    }
