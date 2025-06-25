using MythApi.Gods.DBRepositories;
using MythApi.Common.Database;
using MythApi.Common.Database.Models;
using Microsoft.EntityFrameworkCore;
using Moq;
using MythApi.Gods.Models;
using Mythology = MythApi.Common.Database.Models.Mythology;
using God = MythApi.Common.Database.Models.God;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.AspNetCore.Components.Web;
using System.Data.Entity;

namespace UnitTests
{

    public class GodRepositoryTests
    {
        private GodRepository _repository;
        private Mock<AppDbContext> _mockContext;
        private List<God> _gods;

        //[SetUp]
        public void Setup()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .Options;
            _mockContext = new Mock<AppDbContext>(options);
            _gods = new List<God>(); 

            var gods = new List<God>().AsQueryable();
            var mockGodSet = new Mock<Microsoft.EntityFrameworkCore.DbSet<God>>();
            mockGodSet.As<IQueryable<God>>().Setup(m => m.Provider).Returns(gods.Provider);
            mockGodSet.As<IQueryable<God>>().Setup(m => m.Expression).Returns(gods.Expression);
            mockGodSet.As<IQueryable<God>>().Setup(m => m.ElementType).Returns(gods.ElementType);
            mockGodSet.As<IQueryable<God>>().Setup(m => m.GetEnumerator()).Returns(gods.GetEnumerator()); 
            mockGodSet.Setup(m => m.AsQueryable()).Returns(gods.AsQueryable());
            mockGodSet.As<IAsyncEnumerable<God>>().Setup(m => m.GetAsyncEnumerator(default)).Returns(new List<God>().ToAsyncEnumerable().GetAsyncEnumerator());
            mockGodSet.Setup(m => m.Add(It.IsAny<God>())).Callback<God>(g => _gods.Add(g));


            var mockAliases = new Mock<Microsoft.EntityFrameworkCore.DbSet<Alias>>();
            var mockMythologies = new Mock<Microsoft.EntityFrameworkCore.DbSet<Mythology>>();

            var context = _mockContext.Object;
            context.Gods = mockGodSet.Object;
            context.Aliases = mockAliases.Object;
            context.Mythologies = mockMythologies.Object;

            _repository = new GodRepository(context);
        }

        //[Test]
        public async Task AddOrUpdateGods_ShouldAddNewGod()
        {
            var gods = new List<GodInput>
            {
                new GodInput { Name = "Zeus", MythologyId = 1, Description = "God of the sky" }
            };

            var result = await _repository.AddOrUpdateGods(gods);

            Assert.That(result.Count, Is.EqualTo(1));
            Assert.That(result.First().Name, Is.EqualTo("Zeus"));
        }

        //[Test]
        public async Task GetAllGodsAsync_ShouldReturnAllGods()
        {
            var gods = new List<God>
            {
                new God { Name = "Zeus", MythologyId = 1, Description = "God of the sky" },
                new God { Name = "Hera", MythologyId = 1, Description = "Goddess of marriage" }
            };
            _mockContext.Object.Gods.AddRange(gods);
            await _mockContext.Object.SaveChangesAsync();

            var result = await _repository.GetAllGodsAsync();

            Assert.That(result.Count, Is.EqualTo(2));
        }

        //[Test]
        public async Task GetGodAsync_ShouldReturnGodById()
        {
            var god = new God { Id = 1, Name = "Zeus", MythologyId = 1, Description = "God of the sky" };
            _mockContext.Object.Gods.Add(god);
            await _mockContext.Object.SaveChangesAsync();

            var parameter = new GodParameter(1);
            var result = await _repository.GetGodAsync(parameter);

            Assert.That(result.Name, Is.EqualTo("Zeus"));
        }

        //[Test]
        public async Task GetGodByNameAsync_ShouldReturnGodsByName()
        {
            var gods = new List<God>
            {
                new God { Name = "Zeus", MythologyId = 1, Description = "God of the sky" },
                new God { Name = "Hera", MythologyId = 1, Description = "Goddess of marriage" }
            };
            _mockContext.Object.Gods.AddRange(gods);
            await _mockContext.Object.SaveChangesAsync();

            var parameter = new GodByNameParameter("Zeus", true);
            var result = await _repository.GetGodByNameAsync(parameter);

            Assert.That(result.Count, Is.EqualTo(1));
            Assert.That(result.First().Name, Is.EqualTo("Zeus"));
        }
    }
}