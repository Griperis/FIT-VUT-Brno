/*

using System;
using System.Collections.Generic;
using TeamChat.BL.Model;
using TeamChat.BL.Repositories;
using TeamChat.BL.Services;
using TeamChat.BL.Messages;
using TeamChat.App.Views;
using System.Threading.Tasks;
using TeamChat.BL.Interfaces;
using Moq;
using Xunit;
using TeamChat.App.ViewModels;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Assert = Microsoft.VisualStudio.TestTools.UnitTesting.Assert;

namespace TeamChat.App.Tests
{
    [TestClass]
    public class TeamViewModelTest
    {
        private readonly Mock<IWindowService> windowsServiceMock;
        private readonly Mock<IUserRepository> userRepositoryMock;
        private readonly Mock<ITeamRepository> teamRepositoryMock;
        private readonly Mock<IPostRepository> postRepositoryMock;
        private readonly Mock<ICommentRepository> commentRepositoryMock;

        private readonly Mock<IMediator> mediatorMock;
        private readonly TeamViewModel teamViewModelSUT;

        public TeamViewModelTest()
        {
            this.windowsServiceMock = new Mock<IWindowService>();
            this.userRepositoryMock = new Mock<IUserRepository>();
            this.teamRepositoryMock = new Mock<ITeamRepository>();

            this.mediatorMock = new Mock<IMediator>() { CallBase = true };

            userRepositoryMock.Setup(repository => repository.GetAll())
                .Returns(() => new List<UserListModel>());

            teamRepositoryMock.Setup(repository => repository.GetAll())
                .Returns(() => new List<TeamListModel>());

            postRepositoryMock.Setup(repository => repository.)
            commentRepositoryMock.Setup(repository => repository.GetAll())
               .Returns(() => new List<CommentListModel>());

            teamViewModelSUT = new TeamViewModel(windowsServiceMock.Object, teamRepositoryMock.Object, userRepositoryMock.Object, postRepositoryMock.Object, commentRepositoryMock.Object, mediatorMock.Object);
        }

        [TestMethod]
        public void MemberSelectedCommand_Set_SelectedMember()
        {
            teamViewModelSUT.MemberSelectedCommand.Execute(new UserListModel());

            Assert.IsNotNull(teamViewModelSUT.SelectedMember);

        }

        [TestMethod]
        public void UserSelectedCommand_Set_SelectedMember()
        {
            teamViewModelSUT.UserSelectedCommand.Execute(new UserListModel());

            Assert.IsNotNull(teamViewModelSUT.SelectedUser);

        }


        [TestMethod]
        public void RenameTeamCommand_Sends_TeamUpdatedMessage()
        {
            teamViewModelSUT.RenameTeamCommand.Execute(null);

            mediatorMock.Verify(mediator => mediator.Send(It.IsAny<TeamUpdatedMessage>()), Times.Once);
        }

        [TestMethod]
        public void DeleteTeamCommand_Sends_TeamUpdatedMessage()
        {
            teamViewModelSUT.DeleteTeamCommand.Execute(null);

            mediatorMock.Verify(mediator => mediator.Send(It.IsAny<TeamUpdatedMessage>()), Times.Once);

        }


    }
}

*/