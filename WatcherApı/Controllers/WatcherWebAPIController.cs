using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Renci.SshNet;
using System.Diagnostics;
using WatcherApi.Classes;

namespace WatchApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class WatcherApiController : ControllerBase
    {
        private readonly Context _context;
        private readonly DataQuery _dataQuery;


        public WatcherApiController(Context context, DataQuery dataQuery)
        {
            _context = context;
            _dataQuery = dataQuery;
        }


        [HttpGet("status/{host}")]
        public IActionResult CheckVirtualMachineStatus(string host)
        {
            try
            {

                var virtualMachine = _context.Machines.FirstOrDefault(vm => vm.Host == host);


                if (virtualMachine != null)
                {
                    bool isMachineRunning = IsVirtualMachineRunning(virtualMachine);

                    if (isMachineRunning)
                    {
                        return Ok(new { Message = $"{virtualMachine.Host} IP'li sanal makine açık", IsRunning = true });

                    }
                    else
                    {
                        return Ok(new { Message = $"{virtualMachine.Host} IP'li sanal makine kapalı", IsRunning = false });
                    }
                }
                else
                {
                    return NotFound("Sanal makine bilgisi bulunamadı");
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { ex.Message, ex.StackTrace });
            }
        }

        private bool IsVirtualMachineRunning(MachineInfo virtualMachine)
        {
            try
            {
                PasswordConnectionInfo connectionInfo = new PasswordConnectionInfo(virtualMachine.Host, virtualMachine.Port, virtualMachine.UserName, virtualMachine.Password);
                using (SshClient client = new SshClient(connectionInfo))
                {
                    client.Connect();

                    if (client.IsConnected)
                    {
                        client.Disconnect();
                        return true; // Sunucuya SSH bağlantısı varsa sanal makine çalışıyor 
                    }
                    else
                    {
                        return false; // Sunucuya SSH bağlantısı yoksa sanal makina kapalı
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Hata oluştu: {ex.Message}");

                return false; // Bir hata oluştuğunda  sanal makine kapalı kabul edilsin
            }
        }


        [HttpGet]
        public IActionResult GetAllVirtualMachines()
        {
            try
            {
                var virtualMachines = _context.Machines.ToList();
                var result = virtualMachines.Select(vm => new
                {
                    vm.Id,
                    vm.Host,
                    vm.UserName,
                    IsRunning = IsVirtualMachineRunning(vm)
                });

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Hata: {ex.Message}");
            }

        }

        [HttpGet("data/{refnr}")]
        public IActionResult GetData(string refnr)
        {
            var result = _dataQuery.GetAddressByRefnr(refnr);

            return Ok(result);


        }

        [HttpGet("sendung")]
        public IActionResult GetSendungByDateRange(string startDate, string endDate)
        {
            return _dataQuery.GetSendungByDateRange(startDate, endDate);
        }


        [HttpPost("{host}/toggle")]
        public IActionResult ToggleVirtualMachineStatus(string host)
        {
            try
            {
                var virtualMachine = _context.Machines.FirstOrDefault(vm => vm.Host == host);
                if (virtualMachine == null)
                    return NotFound("Sanal makine bilgisi bulunamadı");

                bool isMachineRunning = IsVirtualMachineRunning(virtualMachine);
                if (isMachineRunning)
                {
                    CloseVirtualMachine(virtualMachine);
                }
                else
                {
                    OpenVirtualMachine(virtualMachine);
                }

                return Ok(new { Message = $"{virtualMachine.Host} IP'li sanal makine durumu değiştirildi" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Hata: {ex.Message}");
            }
        }

        private void OpenVirtualMachine(MachineInfo virtualMachine)
        {
            ExecuteVBoxManageCommand($"startvm \"{virtualMachine.VirtualName}\"");
        }

        private void CloseVirtualMachine(MachineInfo virtualMachine)
        {
            string arguments = $"controlvm {virtualMachine.VirtualName} poweroff";
            ExecuteVBoxManageCommand(arguments);
        }

        private void ExecuteVBoxManageCommand(string arguments)
        {
            try
            {
                var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "VBoxManage",
                        Arguments = arguments,
                        RedirectStandardOutput = true,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    }
                };
                process.Start();

                string output = process.StandardOutput.ReadToEnd();
                process.WaitForExit();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Hata oluştu: {ex.Message}");
            }
        }

        [HttpGet("docker/{host}")]
        public IActionResult DockerStatus(string host)
        {
            try
            {
                var virtualMachine = _context.Machines.FirstOrDefault(vm => vm.Host == host);

                if (virtualMachine != null)
                {
                    bool isDockerInstalled = IsDockerInstalled(virtualMachine);

                    return Ok(new { Message = $"{virtualMachine.Host} IP'li sanal makine üzerinde Docker {(isDockerInstalled ? "kurulu" : "yok")}" });
                }
                else
                {
                    return NotFound("Sanal makina bilgisi bulunamadı");
                }
            }
            catch (Exception ex)
            {
                var errorResponse = new { Message = ex.Message, StackTrace = ex.StackTrace };
                return StatusCode(500, errorResponse);
            }
        }


        private bool IsDockerInstalled(MachineInfo virtualMachine)

        {
            try
            {

                PasswordConnectionInfo connectionInfo = new PasswordConnectionInfo(virtualMachine.Host, virtualMachine.Port, virtualMachine.UserName, virtualMachine.Password);

                using (SshClient client = new SshClient(connectionInfo))
                {
                    client.Connect();

                    if (client.IsConnected)
                    {
                        string dockercommand = "docker --version";

                        SshCommand command = client.RunCommand(dockercommand);

                        return command.ExitStatus == 0 && command.Result.Contains("Docker");

                    }

                    else
                    {
                        return false;
                    }
                }
            }

            catch (Exception ex)
            {
                Console.WriteLine($"Hata oluştu : {ex.Message}");
                return false;
            }
        }
        [HttpGet("memory/{host}")]
        public IActionResult GetMemoryUsage(string host)
        {
            try
            {
                var virtualMachine = _context.Machines.FirstOrDefault(x => x.Host == host);
                if (virtualMachine == null)
                    return NotFound("Sanal makine bilgisi bulunamadı");

                var commandResult = ExecuteCommand(virtualMachine, "free -m");

                double totalMemory = ParseMemory(commandResult, 1);
                double usedMemory = ParseMemory(commandResult, 2);
                double usagePercentage = (usedMemory / totalMemory) * 100;

                string status = usagePercentage >= 80 ? "Red" : "Green";
                var response = new
                {
                    UsedMemory = usedMemory,
                    UsagePercentage = usagePercentage,
                    Status = status,
                    TotalMemory = totalMemory
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { ex.Message, ex.StackTrace });
            }
        }
        private string ExecuteCommand(MachineInfo virtualMachine, string command)
        {
            try
            {
                var connectionInfo = new PasswordConnectionInfo(virtualMachine.Host, virtualMachine.Port, virtualMachine.UserName, virtualMachine.Password);
                using (var client = new SshClient(connectionInfo))
                {
                    client.Connect();
                    System.Threading.Thread.Sleep(5000);

                    var commandResult = client.RunCommand(command);
                    if (commandResult.ExitStatus != 0)
                        throw new Exception($"Komut çalıştırma hatası: {commandResult.Error}");

                    return commandResult.Result;
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Komut çalıştırma hatası: {ex.Message}");
            }
        }

        private static double ParseMemory(string output, int index)
        {
            var lines = output.Split('\n');
            var values = lines[1].Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            return double.Parse(values[index]);
        }
    }
}

