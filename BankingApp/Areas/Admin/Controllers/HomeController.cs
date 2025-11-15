using AutoMapper;
using BankingApp.Core.Application.Interfaces;
using BankingApp.Core.Application.Services;
using BankingApp.Core.Application.ViewModels.Dashboards;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BankingApp.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "ADMIN")]
    public class HomeController : Controller
    {
        private readonly IDashboardsStatsService _dashboardsStatsService;
        private IMapper _mapper;

        public HomeController(IDashboardsStatsService dashboardsStatsService, IMapper mapper)
        {
            _dashboardsStatsService = dashboardsStatsService;
            _mapper = mapper;
        }
        public async Task<ActionResult >Index()
        {

            var dto = await _dashboardsStatsService.GetAdminStats();
            var vm = _mapper.Map<AdminDashboardViewModel>(dto);
            return View(vm);
        }

        public ActionResult Details(int id)
        {
            return View();
        }

        // GET: HomeController/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: HomeController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // GET: HomeController/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: HomeController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // GET: HomeController/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: HomeController/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id, IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }
    }
}
