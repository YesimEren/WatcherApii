using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Renci.SshNet;
using System.Diagnostics;
using WatcherApi.Classes;

namespace WatcherApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    
    public class WatcherWebAPIController : ControllerBase
    {
        private readonly Context _context;
        private readonly DataQuery _dataQuery;
        

        public WatcherWebAPIController(Context context, DataQuery dataQuery)
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
                var errorResponse = new { Message = ex.Message, StackTrace = ex.StackTrace };
                return StatusCode(500, errorResponse);
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

        [Authorize(Roles = "Admin, IT")]
        [HttpGet]
        public IActionResult GetAllVirtualMachines()
        {
            try
            {
                var virtualMachines = _context.Machines.ToList();
                // IsRunning değerini doğru bir şekilde set etmek için bu kısmı güncelleyin
                var result = virtualMachines.Select(vm => new { vm.Id, vm.Host, vm.UserName, IsRunning = IsVirtualMachineRunning(vm) });

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

                if (virtualMachine != null)
                {
                    bool isMachineRunning = IsVirtualMachineRunning(virtualMachine);

                    if (isMachineRunning)
                    {
                        // Sanal makine açıksa, kapat
                        CloseVirtualMachine(virtualMachine);
                    }
                    else
                    {
                        // Sanal makine kapalıysa, aç
                        OpenVirtualMachine(virtualMachine);
                    }

                    return Ok(new { Message = $"{virtualMachine.Host} IP'li sanal makine durumu değiştirildi" });
                }
                else
                {
                    return NotFound("Sanal makine bilgisi bulunamadı");
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Hata: {ex.Message}");
            }
        }

        private void OpenVirtualMachine(MachineInfo virtualMachine)
        {
            string arguments = $"startvm \"{virtualMachine.VirtualName}\"";
            ExecuteVBoxManageCommand(arguments);
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
                // VBoxManage komutunu başlat
                Process process = new Process();
                process.StartInfo.FileName = "VBoxManage";
                process.StartInfo.Arguments = arguments;
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.CreateNoWindow = true;
                process.Start();

                // Çıktıyı oku (isteğe bağlı)
                string output = process.StandardOutput.ReadToEnd();

                process.WaitForExit();
                process.Close();

                // output değişkenini kullanarak gerekli işlemleri yapabilirsiniz
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Hata oluştu: {ex.Message}");
                // Hata durumunda gerekli işlemleri yapabilirsiniz
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
                        // SSH komutu başarılı bir şekilde çalıştıysa ve çıktı içinde "Docker" geçiyorsa Docker yüklüdür
                        return command.ExitStatus == 0 && command.Result.Contains("Docker");

                    }

                    else
                    {
                        return false; // Sunucuya SSH bağlantısı yoksa Docker kurulu değil.
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
                //Verilen hosta göre verileri veritabanınsan çek
                var virtualMachine = _context.Machines.FirstOrDefault(x => x.Host == host);
                if (virtualMachine != null)
                {
                    //SHH aşağıdaki komutu çalıştır.
                    string command = "free -m";

                    //free -m seçilen IP adresinde çalıştır.ve çıktısını alır.
                    var commandResult = ExecuteCommand(virtualMachine, command);

                    //Çıktıyı parçala
                    double totalMemory = ParseTotalMemory(commandResult);
                    double usedMemory = ParseUsedMemory(commandResult);
                    double usagePercentage = (usedMemory / totalMemory) * 100;

                    //Eşik değer kontrolü yap.
                    string status = "Green";
                    if (usagePercentage >= 80)
                    {
                        status = "Red";
                    }

                    //Sonuçları Json Formatına dönüştür.
                    return Ok(new
                    {
                        UsedMemory = usedMemory,
                        UsagePercentage = usagePercentage,
                        Status = status,
                        TotalMemory = totalMemory

                    });

                }
                else
                {
                    return NotFound("Sanal Makina Bilgisi Bulunamadı");
                }
            }
            catch (Exception ex)
            {
                //Hata durumuna uuygun HTTP durm kodu ve hata mesajı ver.
                var error = new { Message = ex.Message, StackTrace = ex.StackTrace };
                return StatusCode(500, error);
            }
        }
        private string ExecuteCommand(MachineInfo virtualMachine, string command)
        {
            try
            {
                //SSH bağlantısı için gerekli olan bilgileri içeren nesne
                PasswordConnectionInfo connectionInfo = new PasswordConnectionInfo(virtualMachine.Host, virtualMachine.Port, virtualMachine.UserName, virtualMachine.Password);
                //SSH Bağlantısı kur.
                using (SshClient client = new SshClient(connectionInfo))
                {
                    client.Connect();
                    System.Threading.Thread.Sleep(5000);

                    //SSH Bağlnatısı başarılı ise komutu çalıştır çıktısını al.
                    if (client.IsConnected)
                    {
                        var commandResult = client.RunCommand(command);
                        // Çalıştırılan komutun çıktısını geri döner
                        if (commandResult.ExitStatus == 0)
                        {
                            var output = commandResult.Result;
                            return output;
                        }
                        else
                        {
                            var errorMesage = commandResult.Error;
                            throw new Exception($"Komut çalıştırma hatası:{errorMesage}");
                        }
                    }
                    else
                    {
                        //SSH Bağlantısı kurulamazsa hata fırlat.
                        throw new Exception("SSH Bağlantısı kurulamadı");
                    }
                }
            }
            catch (Exception ex)
            {
                // Hata durumunda uygun bir hata mesajı ile istisna fırlat
                throw new Exception($"Komut çalıştırma hatası: {ex.Message}");
            }
        }

        private static double ParseUsedMemory(string output)
        {
            string[] lines = output.Split('\n');
            string usedMemoryLine = lines[1];

            string[] values = usedMemoryLine.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            double usedMemory = double.Parse(values[2]);

            return usedMemory;
        }

        private static double ParseTotalMemory(string output)
        {
            string[] lines = output.Split('\n');
            string totalMemoryLine = lines[1];

            string[] values = totalMemoryLine.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            double totalMemory = double.Parse(values[1]);

            return totalMemory;
        }
    }
}
