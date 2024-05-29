using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Renci.SshNet;
using System;

namespace WatcherApi.Classes
{
    public class DataQuery
    {
        public IActionResult GetAddressByRefnr(string refnr)
        {
            using var client = new SshClient("217.160.27.147", "root", "8DCMu9_r8v");
            //using var client = new SshClient("192.168.2.133", "tas-root", "tas-root");
            client.Connect();

            if (client.IsConnected)
            {
                Console.WriteLine("SSH bağlantısı kuruldu.");
                var command = client.CreateCommand($"mysql -u root -p\"8DCMu9_r8v\" taskinAmendWeb -e \"SELECT address.CITY,address.COUNTRY,address.KDNR,address.NAME1,address.STREET,address.ZIPCODE FROM sendung INNER JOIN address ON sendung.user_id = address.id WHERE sendung.refnr = '{refnr}';\"");

                //var command = client.CreateCommand($"mysql -u root -p\"12345\" testdata -e \"SELECT address.* FROM sendung INNER JOIN address ON sendung.user_id = address.id WHERE sendung.refnr = '{refnr}';\"");

                var result = command.Execute();

                Console.WriteLine(result);

                client.Disconnect();
                Console.WriteLine("SSH bağlantısı sonlandırıldı.");

                // Tablo verisini dize olarak dön
                return new OkObjectResult(result);
            }
            else
            {
                Console.WriteLine("SSH bağlantısı kurulamadı.");
                return new StatusCodeResult(500);
            }
        }

        public IActionResult GetSendungByDateRange(string startDate, string endDate)
        {
            using var client = new SshClient("217.160.27.147", "root", "8DCMu9_r8v");
            client.Connect();

            if (client.IsConnected)
            {
                Console.WriteLine("SSH bağlantısı kuruldu.");

                var command = client.CreateCommand($"mysql -u root -p\"8DCMu9_r8v\" taskinAmendWeb -e \"SELECT sendung.created AS Erstellungsdatum, sendung.refnr AS Referenz, user.longname as Ersteller FROM sendung, user WHERE created >= '{startDate} 00:00:00' AND created <= '{endDate} 23:59:59' AND sendung.user_id = user.id;\"");

                var result = command.Execute();

                // Sonucu uygun bir JSON formatına dönüştür
                var jsonObject = ConvertToJsonObject(result);

                client.Disconnect();
                Console.WriteLine("SSH bağlantısı sonlandırıldı.");

                return new OkObjectResult(jsonObject);
            }
            else
            {
                Console.WriteLine("SSH bağlantısı kurulamadı.");
                return new StatusCodeResult(500);
            }
        }

        private string ConvertToJsonObject(string result)
        {
            // Boşlukları kaldırıp sonucu düzgün bir JSON formatına dönüştür
            var lines = result.Split('\n', StringSplitOptions.RemoveEmptyEntries);
            var jsonList = new List<Dictionary<string, string>>();

            foreach (var line in lines)
            {
                var items = line.Split('\t', StringSplitOptions.RemoveEmptyEntries);
                var dictionary = new Dictionary<string, string>();

                for (int i = 0; i < items.Length; i++)
                {
                    dictionary[$"Column{i + 1}"] = items[i];
                }

                jsonList.Add(dictionary);
            }

            return JsonConvert.SerializeObject(jsonList);
        }

    }
}
