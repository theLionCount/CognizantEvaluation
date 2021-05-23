using CognizantEvaluation.Controllers;
using Common.Data;
using Common.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace UnitTest
{
    public class AirportsControllerTests
    {
        [Fact]
        public async Task IndexNoFilter()
        {
            var options = new DbContextOptionsBuilder<AirportContext>()
            .UseInMemoryDatabase(databaseName: "AirportDB")
            .Options;

            using var context = new AirportContext(options);
            context.Database.EnsureDeleted();

            context.airports.Add(new Airport() { continent = Continent.EU, iso = "IT", name = "airport1", status = 1, type = AirportType.seaplanes });
            context.airports.Add(new Airport() { continent = Continent.EU, iso = "IT", name = "airport2", status = 1, type = AirportType.seaplanes });
            context.airports.Add(new Airport() { continent = Continent.EU, iso = "HU", name = "airport3", status = 1, type = AirportType.seaplanes });
            context.airports.Add(new Airport() { continent = Continent.EU, iso = "GB", name = "airport4", status = 1, type = AirportType.seaplanes });
            context.SaveChanges();

            object o = new object();
            var cacheMock = new Mock<IMemoryCache>();
            var mockCacheEntry = new Mock<ICacheEntry>();
            cacheMock.Setup(t => t.TryGetValue(It.IsAny<object>(), out o)).Returns(false);
            cacheMock.Setup(mc => mc.CreateEntry(It.IsAny<object>())).Returns(mockCacheEntry.Object);

            var controller = new AirportsController(context, cacheMock.Object);

            controller.ControllerContext = new ControllerContext() { HttpContext = new DefaultHttpContext() };

            var result = await controller.Index(null);
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<IEnumerable<Airport>>(viewResult.ViewData.Model);
            Assert.Equal(4, model.Count());
        }

        [Fact]
        public async Task IndexFiltered()
        {
            var options = new DbContextOptionsBuilder<AirportContext>()
            .UseInMemoryDatabase(databaseName: "AirportDB")
            .Options;

            using var context = new AirportContext(options);
            context.Database.EnsureDeleted();

            context.airports.Add(new Airport() { continent = Continent.EU, iso = "IT", name = "airport1", status = 1, type = AirportType.seaplanes });
            context.airports.Add(new Airport() { continent = Continent.EU, iso = "IT", name = "airport2", status = 1, type = AirportType.seaplanes });
            context.airports.Add(new Airport() { continent = Continent.EU, iso = "HU", name = "airport3", status = 1, type = AirportType.seaplanes });
            context.airports.Add(new Airport() { continent = Continent.EU, iso = "GB", name = "airport4", status = 1, type = AirportType.seaplanes });
            context.SaveChanges();

            object o = new object();
            var cacheMock = new Mock<IMemoryCache>();
            var mockCacheEntry = new Mock<ICacheEntry>();
            cacheMock.Setup(t => t.TryGetValue(It.IsAny<object>(), out o)).Returns(false);
            cacheMock.Setup(mc => mc.CreateEntry(It.IsAny<object>())).Returns(mockCacheEntry.Object);

            var controller = new AirportsController(context, cacheMock.Object);

            controller.ControllerContext = new ControllerContext() { HttpContext = new DefaultHttpContext() };

            var result = await controller.Index("IT");
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<IEnumerable<Airport>>(viewResult.ViewData.Model);
            Assert.Equal(2, model.Count());
        }
    }
}
