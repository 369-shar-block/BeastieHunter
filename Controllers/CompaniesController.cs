using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using BeastieHunter.Data;
using BeastieHunter.Models;
using BeastieHunter.Models.ViewModels;
using BeastieHunter.Extensions;
using BeastieHunter.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;

namespace BeastieHunter.Controllers
{

    [Authorize(Roles ="Admin")]
    public class CompaniesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IBTCompanyInfoService _companyService;
        private readonly IBTRolesService _rolesService;
        private readonly UserManager<BTUser> _userManager;
        public CompaniesController(ApplicationDbContext context, IBTCompanyInfoService companyService, IBTRolesService rolesService, UserManager<BTUser> userManager)
        {
            _context = context;
            _companyService = companyService;
            _rolesService = rolesService;
            _userManager = userManager;
        }

        

        // GET: Companies/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Companies == null)
            {
                return NotFound();
            }

            var company = await _context.Companies
                .FirstOrDefaultAsync(m => m.Id == id);
            if (company == null)
            {
                return NotFound();
            }

            return View(company);
        }

        [HttpGet]
        public async Task<IActionResult> ManageUserRoles()
        {
            List<ManageUserRolesViewModel> model = new();

            int companyId = (int)User.Identity.GetCompanyId();

            List<BTUser> members = await _companyService.GetAllMembersAsync(companyId);

            string btUserId = _userManager.GetUserId(User);

            foreach(BTUser member in members)
            {

                if (string.Compare(btUserId, member.Id) != 0)
                {
                    ManageUserRolesViewModel viewModel = new();

                    IEnumerable<string> currentRoles = await _rolesService.GetUserRolesAsync(member);

                    viewModel.BTUser = member;
                    viewModel.Roles = new MultiSelectList(await _rolesService.GetRolesAsync(), "Name", "Name", currentRoles);

                    model.Add(viewModel);
                }
                
            }

            return View(model);

        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ManageUserRoles(ManageUserRolesViewModel viewModel)
        {
            int companyId = (int)User.Identity!.GetCompanyId();

            BTUser? btUser = (await _companyService.GetAllMembersAsync(companyId)).FirstOrDefault(m => m.Id == viewModel.BTUser!.Id);

            IEnumerable<string> currentRoles = await _rolesService.GetUserRolesAsync(btUser!);

            string? selectedRole = viewModel.SelectedRoles!.FirstOrDefault();

            if (!string.IsNullOrEmpty(selectedRole))
            {
                if(await _rolesService.RemoveUserFromRolesAsync(btUser!, currentRoles))
                {
                    await _rolesService.AddUserToRoleAsync(btUser!, selectedRole);
                }
            }

            return RedirectToAction(nameof(ManageUserRoles));
        }
    }
}
