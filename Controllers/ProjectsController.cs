using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using BeastieHunter.Data;
using BeastieHunter.Models;
using BeastieHunter.Extensions;
using BeastieHunter.Services.Interfaces;
using BeastieHunter.Models.ViewModels;
using BeastieHunter.Models.Enums;
using Microsoft.AspNetCore.Identity;
using BeastieHunter.Services;
using Microsoft.AspNetCore.Authorization;

namespace BeastieHunter.Controllers
{
    [Authorize]
    public class ProjectsController : Controller
    {
        
        private readonly IBTRolesService _rolesService;
        private readonly IBTLookupService _lookupService;
        private readonly IBTFileService _fileService;
        private readonly IBTProjectService _projectService;
        private readonly UserManager<BTUser> _userManager;
        private readonly IBTCompanyInfoService _companyInfoService;

        public ProjectsController( IBTRolesService rolesService, IBTLookupService lookupService, IBTFileService fileService, IBTProjectService projectService, UserManager<BTUser> userManager, IBTCompanyInfoService companyInfoService)
        {
           
            _rolesService = rolesService;
            _lookupService = lookupService;
            _fileService = fileService;
            _projectService = projectService;
            _userManager = userManager;
            _companyInfoService = companyInfoService;
        }

      

        // GET: Projects
        public async Task<IActionResult> MyProjects()
        {
            string userId = _userManager.GetUserId(User);

            List<Project> projects = await _projectService.GetUserProjectsAsync(userId);

            return View(projects);
        }

        // GET: Projects
        public async Task<IActionResult> AllProjects()
        {
            List<Project> projects = new();

            int companyId = User.Identity.GetCompanyId().Value;

            if(User.IsInRole(nameof(Roles.Admin)) || User.IsInRole(nameof(Roles.ProjectManager)))
            {
                projects = await _companyInfoService.GetAllProjectsAsync(companyId);
            }
            else
            {
                projects = await _projectService.GetAllProjectsByCompany(companyId);
            }

            return View(projects);
        }

        // GET: Projects
        public async Task<IActionResult> ArchivedProjects()
        {
            int companyId = User.Identity.GetCompanyId().Value;

            List<Project> projects = await _projectService.GetArchivedProjectsByCompany(companyId);

            return View(projects);
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UnassignedProjects()
        {
            int companyId = User.Identity.GetCompanyId().Value;

            List<Project> projects = new();

            projects = await _projectService.GetUnassignedProjectsAsync(companyId);

            return View(projects);
        }

        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> AssignPM(int projectId)
        {
            int companyId = User.Identity.GetCompanyId().Value;

            AssignPMViewModel model = new();

            model.Project = await _projectService.GetProjectByIdAsync(projectId, companyId);
            model.PMList = new SelectList(await _rolesService.GetUsersInRoleAsync(nameof(Roles.ProjectManager), companyId), "Id", "FullName");

            return View(model);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AssignPM(AssignPMViewModel model)
        {
            if (!string.IsNullOrEmpty(model.PMID))
            {
                await _projectService.AddProjectManagerAsync(model.PMID, model.Project.Id);

                return RedirectToAction(nameof(Details), new {id=model.Project.Id});
            }

            return RedirectToAction(nameof(AssignPM), new {projectId = model.Project.Id});
        }


        [Authorize(Roles = "Admin, ProjectManager")]
        [HttpGet]
        public async Task<IActionResult> AssignMembers(int id)
        {
            ProjectMembersViewModel model = new();

            int companyId = User.Identity.GetCompanyId().Value;

            model.Project = await _projectService.GetProjectByIdAsync(id, companyId);

            List<BTUser> developers = await _rolesService.GetUsersInRoleAsync(nameof(Roles.Developer), companyId);
            List<BTUser> submitters = await _rolesService.GetUsersInRoleAsync(nameof(Roles.Submitter), companyId);

            List<BTUser> companyMembers = developers.Concat(submitters).ToList();

            List<string> projectMembers = model.Project.Members.Select(m => m.Id).ToList();
            model.Users = new MultiSelectList(companyMembers, "Id", "FullName", projectMembers);

            return View(model);
        }

        [Authorize(Roles = "Admin, ProjectManager")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AssignMembers(ProjectMembersViewModel model)
        {
            if(model.SelectedUsers != null)
            {
                List<string> memberIds = (await _projectService.GetAllProjectMembersExceptPMAsync(model.Project.Id)).Select(model => model.Id).ToList();

                foreach(string member in memberIds)
                {
                    await _projectService.RemoveUserFromProjectAsync(member, model.Project.Id);
                }

                foreach(string member in model.SelectedUsers)
                {
                    await _projectService.AddUserToProjectAsync(member, model.Project.Id);
                }

                return RedirectToAction("Details", "Projects", new { id = model.Project.Id });

            }

            return RedirectToAction(nameof(AssignMembers), new { id = model.Project.Id });
        }



        // GET: Projects/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null )
            {
                return NotFound();
            }

            int companyId = User.Identity.GetCompanyId().Value;

            Project project = await _projectService.GetProjectByIdAsync(id.Value, companyId);



            if (project == null)
            {
                return NotFound();
            }

            return View(project);
        }




        // GET: Projects/Create
        [Authorize(Roles = "Admin, ProjectManager")]
        public async Task<IActionResult> Create()
        {
            int companyId = User.Identity.GetCompanyId().Value;

            AddProjectWithPMViewModel model = new();

            model.PMList = new SelectList(await _rolesService.GetUsersInRoleAsync(Roles.ProjectManager.ToString(), companyId), "Id", "FullName");
            model.PriorityList = new SelectList(await _lookupService.GetProjectPrioritiesAsync(), "Id", "Name");



            return View(model);
        }




        // POST: Projects/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin, ProjectManager")]
        public async Task<IActionResult> Create(AddProjectWithPMViewModel model)
        {

            if(model != null)
            {
                int companyId = User.Identity.GetCompanyId().Value;

                try
                {
                    if(model.Project.FormFile != null)
                    {
                        model.Project.FileData = await _fileService.ConvertFileToByteArrayAsync(model.Project.FormFile);
                        model.Project.FileName = model.Project.FormFile.FileName;
                        model.Project.FileContentType = model.Project.FormFile.ContentType;
                    }

                    model.Project.CompanyId = companyId;

                    await _projectService.AddNewProjectAsync(model.Project);

                    if (!string.IsNullOrEmpty(model.PmId))
                    {
                        await _projectService.AddUserToProjectAsync(model.PmId, model.Project.Id);
                    }


                }
                catch (Exception)
                {

                    throw;
                }
            }

            //_context.Add(project);
            //await _context.SaveChangesAsync();
            //return RedirectToAction(nameof(Index));


            return RedirectToAction("Index");
        }




        // GET: Projects/Edit/5
        [Authorize(Roles = "Admin, ProjectManager")]
        public async Task<IActionResult> Edit(int? id)
        {
            int companyId = User.Identity.GetCompanyId().Value;

            AddProjectWithPMViewModel model = new();

            model.Project = await _projectService.GetProjectByIdAsync(id.Value, companyId);

            model.PMList = new SelectList(await _rolesService.GetUsersInRoleAsync(Roles.ProjectManager.ToString(), companyId), "Id", "FullName");
            model.PriorityList = new SelectList(await _lookupService.GetProjectPrioritiesAsync(), "Id", "Name");



            return View(model);
        }

        // POST: Projects/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin, ProjectManager")]
        public async Task<IActionResult> Edit(AddProjectWithPMViewModel model)
        {
            if (model != null)
            {
              

                try
                {
                    if (model.Project.FormFile != null)
                    {
                        model.Project.FileData = await _fileService.ConvertFileToByteArrayAsync(model.Project.FormFile);
                        model.Project.FileName = model.Project.FormFile.FileName;
                        model.Project.FileContentType = model.Project.FormFile.ContentType;
                    }


                    await _projectService.UpdateProjectAsync(model.Project);

                    if (!string.IsNullOrEmpty(model.PmId))
                    {
                        await _projectService.AddUserToProjectAsync(model.PmId, model.Project.Id);
                    }

                    return RedirectToAction("Index");
                }
                catch (DbUpdateConcurrencyException)
                {

                    if (!await ProjectExists(model.Project.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
            }

            return RedirectToAction("Edit");
        }

        // GET: Projects/Delete/5
        [Authorize(Roles = "Admin, ProjectManager")]
        public async Task<IActionResult> Archive(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            int companyId = User.Identity.GetCompanyId().Value;

            var project = await _projectService.GetProjectByIdAsync(id.Value, companyId);
            if (project == null)
            {
                return NotFound();
            }

            return View(project);
        }

        // POST: Projects/Delete/5
        [Authorize(Roles = "Admin, ProjectManager")]
        [HttpPost, ActionName("Archive")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ArchiveConfirmed(int id)
        {
            int companyId = User.Identity.GetCompanyId().Value;


            var project = await _projectService.GetProjectByIdAsync(id, companyId);

            await _projectService.ArchiveProjectAsync(project);

            return RedirectToAction(nameof(Index));
        }


        // GET: Projects/Delete/5
        [Authorize(Roles = "Admin, ProjectManager")]
        public async Task<IActionResult> Restore(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            int companyId = User.Identity.GetCompanyId().Value;

            var project = await _projectService.GetProjectByIdAsync(id.Value, companyId);
            if (project == null)
            {
                return NotFound();
            }

            return View(project);
        }

        // POST: Projects/Delete/5
        [Authorize(Roles = "Admin, ProjectManager")]
        [HttpPost, ActionName("Restore")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RestoreConfirmed(int id)
        {
            int companyId = User.Identity.GetCompanyId().Value;


            var project = await _projectService.GetProjectByIdAsync(id, companyId);

            await _projectService.RestoreProjectAsync(project);

            return RedirectToAction(nameof(Index));
        }






        private async Task<bool> ProjectExists(int id)
        {
            int companyId = User.Identity.GetCompanyId().Value;

            return (await _projectService.GetAllProjectsByCompany(companyId)).Any(p => p.Id == id);
        }
    }
}
