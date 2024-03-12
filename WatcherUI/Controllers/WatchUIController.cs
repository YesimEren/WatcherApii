using Microsoft.AspNetCore.Mvc;
using WatcherApi.Classes;

namespace WatcherUI.Controllers
{
    public class WatchUIController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly Context _context;
        private readonly ILogger<WatchUIController> _logger;

        public WatchUIController(IConfiguration configuration, [FromServices] Context context, ILogger<WatchUIController> logger)
        {
            _configuration = configuration;
            _context = context;
            _logger = logger;

        }
        public IActionResult Index()
        {
            ViewBag.VirtualMachines = GetVirtualMachinesFromDatabase();
            int virtualMachineCount = ViewBag.VirtualMachines.Count;
            ViewBag.VirtualMachineCount = virtualMachineCount;

            int adminCount = _context.Admins.Count();
            ViewBag.AdminCounts = adminCount;

            return View();
        }

        [HttpGet]
        public IActionResult AddVirtualMachine()
        {
            return View();
        }

        [HttpPost("WatchUI/AddVirtualMachine")]
        public IActionResult AddVirtualMachine(MachineInfo machineinfo)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // SSHInfo'yu doğrudan SSHDbContext'e ekleyin
                    _context.Machines.Add(machineinfo);
                    _context.SaveChanges();

                    // Başarıyla eklendiğine dair mesajı ViewBag'e ekleyin
                    ViewBag.SuccessMessage = "Başarılı bir şekilde eklendi.";


                    return RedirectToAction("Index", "WatchUI");
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Hata: {ex.Message}");
                    ModelState.AddModelError("", "Sanal makine eklenirken bir hata oluştu.");
                    return View(machineinfo);
                }
            }

            return View(machineinfo);
        }

        private List<string> GetVirtualMachinesFromDatabase()
        {
            List<string> virtualMachines = _context.Machines.Select(vm => vm.Host).ToList();
            return virtualMachines;
        }
    }
}
