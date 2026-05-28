using HospitalManagement.Web.Models.Modules;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HospitalManagement.Web.Controllers;

[Authorize]
public class ModulesController : Controller
{
    private static readonly Dictionary<string, ModulePageViewModel> Modules = new (StringComparer.OrdinalIgnoreCase)
    {
        ["statistics"] = new ()
        {
            Key = "statistics",
            Title = "Statistics",
            Description = "This statistics section exists in the web app to mirror the original desktop admin shell, but it is not implemented yet."
        },
        ["pharmacy"] = new ()
        {
            Key = "pharmacy",
            Title = "Pharmacy",
            Description = "This area exists in the web toolbar to mirror the original desktop shell, but it is not implemented yet."
        },
        ["patient-registration"] = new ()
        {
            Key = "patient-registration",
            Title = "Patient Registration",
            Description = "This ER management section is present in the sidebar as a placeholder and is not implemented yet."
        },
        ["queue"] = new ()
        {
            Key = "queue",
            Title = "Queue",
            Description = "This ER management section is present in the sidebar as a placeholder and is not implemented yet."
        },
        ["triage"] = new ()
        {
            Key = "triage",
            Title = "Triage",
            Description = "This ER management section is present in the sidebar as a placeholder and is not implemented yet."
        },
        ["room-assignment"] = new ()
        {
            Key = "room-assignment",
            Title = "Room Assignment",
            Description = "This ER management section is present in the sidebar as a placeholder and is not implemented yet."
        },
        ["examination"] = new ()
        {
            Key = "examination",
            Title = "Examination",
            Description = "This ER management section is present in the sidebar as a placeholder and is not implemented yet."
        },
        ["transfer-log"] = new ()
        {
            Key = "transfer-log",
            Title = "Transfer Log",
            Description = "This ER management section is present in the sidebar as a placeholder and is not implemented yet."
        },
        ["room-management"] = new ()
        {
            Key = "room-management",
            Title = "Room Management",
            Description = "This ER management section is present in the sidebar as a placeholder and is not implemented yet."
        }
    };

    [HttpGet]
    public IActionResult Index(string id)
    {
        if (string.Equals(id, "medical-staff", StringComparison.OrdinalIgnoreCase))
        {
            return RedirectToAction("Dashboard", "MedicalStaff");
        }

        if (!Modules.TryGetValue(id, out ModulePageViewModel? module))
        {
            return NotFound();
        }

        return View(module);
    }
}
