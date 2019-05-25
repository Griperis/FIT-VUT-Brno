using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using TeamChat.DAL.Entities;

namespace TeamChat.DAL.Seed
{
    class Program
    {
        static void Main(string[] args)
        {
            using (var dbContext = CreateDbContext())
            {
                ClearDatabase(dbContext);
                SeedData(dbContext);
            }
        }
        private static void SeedData(TeamChatDbContext dbContext)
        {
            //Teams creating
            var teamVyvoj = new Team()
            {
                Name = "Vývoj",
            };

            var teamTestovani = new Team
            {
                Name = "Testování"
            };

            //Users Creating
            var userAdmin = new User
            {
                Name = "Administrator",
                Email = "admin",
                Password = "0ZKObPUJ+W4pzYeKxhi6JIonoECVp5mF4uMW/lsm6IfxeQuE",
                LastLoginTime = DateTime.Now
            };
            var userAdam = new User
            {
                Name = "Adam Dvořák",
                Email = "dvorak@email.cz",
                Password = "0ZKObPUJ+W4pzYeKxhi6JIonoECVp5mF4uMW/lsm6IfxeQuE",
                LastLoginTime = new DateTime(2000, 1, 1)
            };
            var userMartina = new User
            {
                Name = "Martina Nová",
                Email = "nova@post.cz",
                Password = "0ZKObPUJ+W4pzYeKxhi6JIonoECVp5mF4uMW/lsm6IfxeQuE",
                LastLoginTime = new DateTime(2001, 2, 21)
            };
            var userJan = new User
            {
                Name = "Jan Bouda",
                Email = "janbouda@seznam.cz",
                Password = "0ZKObPUJ+W4pzYeKxhi6JIonoECVp5mF4uMW/lsm6IfxeQuE",
                LastLoginTime = new DateTime(2005, 3, 25)

            };
            var userAneta = new User
            {
                Name = "Aneta Kolárová ",
                Email = "kolar@hotmail.com",
                Password = "0ZKObPUJ+W4pzYeKxhi6JIonoECVp5mF4uMW/lsm6IfxeQuE",
                LastLoginTime = new DateTime(2007, 2, 11)
            };


            // Junction table fill
            var userAdminteamVyvoj = new TeamUser()
            {
                User = userAdmin,
                Team = teamVyvoj
            };
            var userAdminTesovani = new TeamUser
            {
                User = userAdmin,
                Team = teamTestovani
            };

            var userAdamteamVyvoj = new TeamUser
            {
                User = userAdam,
                Team = teamVyvoj
            };

            var userAdamteamTestovani = new TeamUser
            {
                User = userAdam,
                Team = teamTestovani
            };

            var userMartinateamTestovani = new TeamUser
            {
                User = userMartina,
                Team = teamTestovani
            };
            var userJanteamVyvoj = new TeamUser
            {
                User = userJan,
                Team = teamVyvoj
            };
            var userAnetateamVyvoj = new TeamUser
            {
                User = userAneta,
                Team = teamVyvoj,
            };

            //Adding user to teams
            userAdmin.Teams.Add(userAdminteamVyvoj);
            userAdmin.Teams.Add(userAdminTesovani);
            userAdam.Teams.Add(userAdamteamVyvoj);
            userAdam.Teams.Add(userAdamteamTestovani);
            userAneta.Teams.Add(userAnetateamVyvoj);     
            userJan.Teams.Add(userJanteamVyvoj);
            userMartina.Teams.Add(userMartinateamTestovani);

            teamVyvoj.Members.Add(userAnetateamVyvoj);
            teamVyvoj.Members.Add(userAdamteamVyvoj);
            teamVyvoj.Members.Add(userAdminteamVyvoj);
            teamVyvoj.Members.Add(userJanteamVyvoj);
            dbContext.Teams.Add(teamVyvoj);

            teamTestovani.Members.Add(userAdminTesovani);
            teamTestovani.Members.Add(userMartinateamTestovani);
            teamTestovani.Members.Add(userAdamteamTestovani);

            //Creating vyvojPost1
            var vyvojPost1 = new Post
            {
                Author = userAneta,
                Content = "**Zdravím, vítejte v teamu vývoj !**.",                    
                CreationTime = new DateTime(2019,4,29,10,21,55),
                Title = "Vítejte!"
            };
            userAneta.Activities.Add(vyvojPost1);

            //Comments to vyvojPost1
            var vyvojPost1Comment1 = new Comment
            {
                Author = userAdmin,
                BelongsTo = vyvojPost1,
                Content = "Dobrý den. Jsem administrátor a jsem Vám kdykoliv k službám.",
                CreationTime = new DateTime(2019, 4, 29, 10, 33, 20)
            };
            userAdmin.Activities.Add(vyvojPost1Comment1);

            var vyvojPost1Comment2 = new Comment
            {
                Author = userAdam,
                BelongsTo = vyvojPost1,
                Content = "Dobré odpoledne.",
                CreationTime = new DateTime(2019, 4, 29, 13, 55, 04)
            };
            userAdam.Activities.Add(vyvojPost1Comment2);

            var vyvojPost1Comment3 = new Comment
            {
                Author = userJan,
                BelongsTo = vyvojPost1,
                Content = "Hezký den.",
                CreationTime = new DateTime(2019, 4, 29, 14, 5, 28)
            };
            userJan.Activities.Add(vyvojPost1Comment3);

            teamVyvoj.Posts.Add(vyvojPost1);
            vyvojPost1.Comments.Add(vyvojPost1Comment1);
            vyvojPost1.Comments.Add(vyvojPost1Comment2);
            vyvojPost1.Comments.Add(vyvojPost1Comment3);

            //Creating vyvojPost2
            var vyvojPost2 = new Post
            {
                Author = userAdam,
                Content = "Dobrý den, dne **27.5.2019** proběhne zahájení našeho nového projektu. Prosím o svědomitou přípravu a prostudování zadání. Předem děkuji.",
                CreationTime = DateTime.Now,
                Title = "Zahájení projektu"
            };
            userAdam.Activities.Add(vyvojPost2);

            //Comments to vyvojPost2
            var vyvojPost2Comment1 = new Comment
            {
                Author = userAneta,
                BelongsTo = vyvojPost2,
                Content = "Kde můžeme již zmiňované zadání najít?",
                CreationTime = DateTime.Now,
            };
            userAneta.Activities.Add(vyvojPost2Comment1);

            var vyvojPost2Comment2 = new Comment
            {
                Author = userJan,
                BelongsTo = vyvojPost2,
                Content = "Zadání je na našich webových stránkách v sekci projekty/budoucí projekty.",
                CreationTime = DateTime.Now,
            };
            userJan.Activities.Add(vyvojPost2Comment2);
            var vyvojPost2Comment3 = new Comment
            {
                Author = userAneta,
                BelongsTo = vyvojPost2,
                Content = "Děkuji.",
                CreationTime = DateTime.Now,
            };
            userAneta.Activities.Add(vyvojPost2Comment3);

            teamVyvoj.Posts.Add(vyvojPost2);
            vyvojPost2.Comments.Add(vyvojPost2Comment1);
            vyvojPost2.Comments.Add(vyvojPost2Comment2);
            vyvojPost2.Comments.Add(vyvojPost2Comment3);

            //Creating vyvojPost3
            var vyvojPost3 = new Post
            {
                Author = userAdam,
                Content = "Vyhovuje všem termín schůze?",
                Title = "Schůze 27.5 v 10:00",
                CreationTime = DateTime.Now,
            };
            userAdam.Activities.Add(vyvojPost3);

            //Comments to vyvojPost3
            var vyvojPost3Comment1 = new Comment
            {
                Author = userAneta,
                BelongsTo = vyvojPost3,
                Content = "Termín mi vyhovuje. Na jaké téma bude zasedání?",
                CreationTime = DateTime.Now,
            };
            userAneta.Activities.Add(vyvojPost3Comment1);

            var vyvojPost3Comment2 = new Comment
            {
                Author = userJan,
                BelongsTo = vyvojPost3,
                Content = "Termín je v pořádku.",
                CreationTime = DateTime.Now,
            };
            userJan.Activities.Add(vyvojPost3Comment2);

            var vyvojPost3Comment3 = new Comment
            {
                Author = userAdam,
                BelongsTo = vyvojPost3,
                Content = "Téma: zahájení projektu a rozdělení práce na projektu.",
                CreationTime = DateTime.Now,
            };
            userAdam.Activities.Add(vyvojPost3Comment2);

            teamVyvoj.Posts.Add(vyvojPost3);
            vyvojPost3.Comments.Add(vyvojPost3Comment1);
            vyvojPost3.Comments.Add(vyvojPost3Comment2);
            vyvojPost3.Comments.Add(vyvojPost3Comment3);

            //Creating testovaniPost1
            var testovaniPost1 = new Post
            {
                Author = userMartina,
                Content = "Našla jsem chybu ve *výpočtu ceny uskladněného materiálu*. Prosím autora o opravu.",
                CreationTime = new DateTime(2019, 6, 1, 10, 5, 3),
                Title = "Bug"
            };
            userMartina.Activities.Add(testovaniPost1);

            //Comments to testovaniPost1
            var testovaniPost1Comment1 = new Comment
            {
                Author = userAdam,
                BelongsTo = testovaniPost1,
                Content = "Omlouvám se, hned ji opravím.",
                CreationTime = new DateTime(2019, 6, 1, 10, 35, 3),
            };
            userAdam.Activities.Add(testovaniPost1Comment1);
            

            var testovaniPost1Comment2 = new Comment
            {
                Author = userAdam,
                BelongsTo = testovaniPost1,
                Content = "Chyba je opravena.",
                CreationTime = new DateTime(2019, 6, 1, 13, 12, 55),
            };
            userAdam.Activities.Add(testovaniPost1Comment2);

            teamTestovani.Posts.Add(testovaniPost1);
            testovaniPost1.Comments.Add(testovaniPost1Comment1);
            testovaniPost1.Comments.Add(testovaniPost1Comment2);
            
            //adding teams
            dbContext.Teams.Add(teamTestovani);
            dbContext.Teams.Add(teamVyvoj);

            //adding users
            dbContext.Users.Add(userMartina);
            dbContext.Users.Add(userJan);
            dbContext.Users.Add(userAdam);
            dbContext.Users.Add(userAdmin);
            dbContext.Users.Add(userAneta);
            dbContext.Users.Add(userAdam);

            //adding posts
            dbContext.Posts.Add(vyvojPost1);
            dbContext.Posts.Add(vyvojPost2);
            dbContext.Posts.Add(vyvojPost3);

            //adding comments to post1
            dbContext.Comments.Add(vyvojPost1Comment1);
            dbContext.Comments.Add(vyvojPost1Comment2);
            dbContext.Comments.Add(vyvojPost1Comment3);

            //adding comments to post2
            dbContext.Comments.Add(vyvojPost2Comment1);
            dbContext.Comments.Add(vyvojPost2Comment2);
            dbContext.Comments.Add(vyvojPost2Comment3);

            //adding comments to post3
            dbContext.Comments.Add(vyvojPost3Comment1);
            dbContext.Comments.Add(vyvojPost3Comment2);
            dbContext.Comments.Add(vyvojPost3Comment3);

            //adding posts
            dbContext.Posts.Add(testovaniPost1);

            //adding comments to post1
            dbContext.Comments.Add(testovaniPost1Comment1);
            dbContext.Comments.Add(testovaniPost1Comment2);

            dbContext.SaveChanges();

        }

        private static void ClearDatabase(TeamChatDbContext dbContext)
        {
            dbContext.RemoveRange(dbContext.Posts);
            dbContext.RemoveRange(dbContext.Comments);
            dbContext.RemoveRange(dbContext.TeamUsers);
            dbContext.RemoveRange(dbContext.Users);
            dbContext.RemoveRange(dbContext.Teams);
            dbContext.SaveChanges();
        }

        private static TeamChatDbContext CreateDbContext()
        {
            var optionsBuilder = new DbContextOptionsBuilder<TeamChatDbContext>();
            optionsBuilder.UseSqlServer(
                @"Data Source=(LocalDB)\MSSQLLocalDB;Initial Catalog = TasksDB;MultipleActiveResultSets = True;Integrated Security = True; ");
            return new TeamChatDbContext(optionsBuilder.Options);
        }
    }
}
