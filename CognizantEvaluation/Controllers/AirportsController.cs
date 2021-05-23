using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Common.Data;
using Common.Model;
using Microsoft.Extensions.Caching.Memory;

namespace CognizantEvaluation.Controllers
{
    public class AirportsController : Controller
    {
        private readonly AirportContext _context;
        private readonly IMemoryCache cache;

        public AirportsController(AirportContext context, IMemoryCache memoryCache)
        {
            _context = context;
            cache = memoryCache;
        }

        // GET: Airports
        public async Task<IActionResult> Index(string iso)
        {
            ViewData["CurrentFilter"] = iso;

            List<Airport> airports;
            if (!cache.TryGetValue("airports", out airports))
            {
                HttpContext.Response.Headers.Add("from-database", "true");
                airports = await _context.airports.ToListAsync();
                var cacheEntryOptions = new MemoryCacheEntryOptions().SetSlidingExpiration(TimeSpan.FromMinutes(5));
                cache.Set("airports", airports, cacheEntryOptions);
            }
            else
            {
                HttpContext.Response.Headers.Add("from-database", "false");
            }

            return View(iso == null ? airports : airports.Where(t=>t.iso == iso));
        }    

        // GET: Airports/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Airports/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,iata,name,iso,longitude,lattitude,size,type,status,continent")] Airport airport)
        {
            if (ModelState.IsValid)
            {
                _context.Add(airport);
                await _context.SaveChangesAsync();
                cache.Remove("airports");
                return RedirectToAction(nameof(Index));
            }
            return View(airport);
        }
    }
}
