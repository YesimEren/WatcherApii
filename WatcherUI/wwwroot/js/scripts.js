// scripts.js
function showVirtualMachineInfo() {
    var select = document.getElementById("virtualMachineSelect");
    var selectedVirtualMachine = select.options[select.selectedIndex].value;

    document.getElementById("loadingMessage").innerText = "Veriler alınıyor...";
    document.getElementById("loadingMessage").style.display = "block";

    // AJAX isteği yaparak sanal makina durumunu kontrol et
    fetch(`http://localhost:8081/api/WatcherWebAPI/${selectedVirtualMachine}`, {
        method: 'GET',
        mode: 'cors',
    })
        .then(response => {
            if (!response.ok) {
                throw new Error(`HTTP error! Status: ${response.status}`);
            }
            return response.text(); // Veriyi text olarak al
        })
        .then(data => {
            const jsonData = JSON.parse(data);

            // Sanal makina durumuna göre metni güncelle
            updateStatusText(jsonData.isRunning);

            // Geri kalan kodları buraya taşı
            console.log(jsonData);

            document.getElementById("virtualMachineInfo").style.display = "block";
            var statusRectangleElement = document.getElementById("statusRectangle");
            statusRectangleElement.style.backgroundColor = jsonData.isRunning ? "green" : "red";

            var virtualMachineNameElement = document.getElementById("virtualMachineName");
            virtualMachineNameElement.innerText = "Select Virtual Machine: " + selectedVirtualMachine;
            document.getElementById("loadingMessage").style.display = "none";

            // Docker durumunu kontrol et
            DockerStatus();
        })
        .catch(error => {
            console.error('Hata Detayı:', error.message);
            console.log('catch bloğu içindeyim'); // Eklenen console.log
            document.getElementById("loadingMessage").style.display = "none";
            // Ek hata işlemleri
        });

}

// Sanal makina durumuna göre metni güncelleyen fonksiyon
function updateStatusText(isRunning) {
    var statusTextElement = document.getElementById("statusText");

    if (isRunning) {
        statusTextElement.innerText = 'Sanal MAKİNA AÇIK';
        statusTextElement.style.color = 'green';
    } else {
        statusTextElement.innerText = 'Sanal MAKİNA KAPALI';
        statusTextElement.style.color = 'red';
    }
}

function toggleVirtualMachineStatus() {
    var select = document.getElementById("virtualMachineSelect");
    var selectedVirtualMachine = select.options[select.selectedIndex].value;

    document.getElementById("loadingMessage").innerText = "İşlem yapılıyor...";
    document.getElementById("loadingMessage").style.display = "block";

    // AJAX isteği yaparak sanal makina durumunu kontrol et ve değiştir
    fetch(`https://localhost:8081/api/WatcherWebAPI/${selectedVirtualMachine}/toggle`,
        {
            method: 'POST',
            mode: 'cors',
        })
        .then(response => {
            if (!response.ok) {
                throw new Error(`HTTP error! Status: ${response.status}`);
            }
            return response.text(); // Veriyi text olarak al
        })
        .then(data => {
            // JSON parse işlemini burada yap
            const jsonData = JSON.parse(data);

            // Durum kutusunu güncelle
            var statusRectangleElement = document.getElementById("statusRectangle");

            if (jsonData.isRunning) {
                statusRectangleElement.classList.remove("red");
                statusRectangleElement.classList.add("green");
            } else {
                statusRectangleElement.classList.remove("green");
                statusRectangleElement.classList.add("red");
            }

            document.getElementById("loadingMessage").innerText = `${jsonData.message}`;
            document.getElementById("loadingMessage").style.display = "block";
        })
        .catch(error => {
            console.error('Hata:', error);
            document.getElementById("loadingMessage").style.display = "none";
        });
}

// Docker durumunu kontrol et
function DockerStatus() {
    var select = document.getElementById("virtualMachineSelect");
    var selectedVirtualMachine = select.options[select.selectedIndex].value;

    document.getElementById("loadingMessage").innerText = "Veriler alınıyor...";
    document.getElementById("loadingMessage").style.display = "block";

    // AJAX isteği yaparak Docker durumunu kontrol et
    fetch(`http://localhost:8081/api/WatcherWebAPI/docker/${selectedVirtualMachine}`, {
        method: 'GET',
        mode: 'cors',

    })
        .then(response => {
            if (!response.ok) {
                throw new Error(`HTTP error! Status: ${response.status}`);
            }
            return response.json();
        })
        .then(data => {
            console.log(data);

            // Docker durumuna göre uygun alert mesajını göster
            var dockerSuccessAlert = document.getElementById("dockerSuccessAlert");
            var dockerErrorAlert = document.getElementById("dockerErrorAlert");

            if (data.message && data.message.includes("kurulu")) {
                dockerSuccessAlert.style.display = "block";
                dockerErrorAlert.style.display = "none";
            } else {
                dockerSuccessAlert.style.display = "none";
                dockerErrorAlert.style.display = "block";
            }

            document.getElementById("loadingMessage").innerText = "";
            document.getElementById("loadingMessage").style.display = "none";

            // Docker Durumu bölümünü göster
            document.getElementById("dockerStatusInfo").style.display = "block";
        })
        .catch(error => {
            console.error('Hata:', error);
            document.getElementById("loadingMessage").innerText = "Bir hata oluştu. Docker durumu alınamadı.";
            document.getElementById("loadingMessage").style.display = "block";

            // Docker Durumu bölümünü gizle (hata durumunda)
            document.getElementById("dockerStatusInfo").style.display = "none";
        });
}

function getMemoryUsage() {

    var select = document.getElementById("virtualMachineSelect");
    var selectedVirtualMachine = select.options[select.selectedIndex].value;

    document.getElementById("loadingMessage").innerHTML = '<div class="spinner"></div> Yükleniyor...';
    // document.getElementById("loadingMessage").innerText = "Veriler alınıyor...";
    // document.getElementById("loadingMessage").style.display = "block";

    fetch(`http://localhost:8081/api/WatcherWebAPI/memory/${selectedVirtualMachine}`)
        .then(response => {
            if (!response.ok) {
                throw new Error(`HTTP error! Status: ${response.status}`);
            }
            return response.json();
        })
        .then(data => {
            updateMemoryInfo(data);
            createMemoryPieChart(data.usedMemory, data.totalMemory - data.usedMemory);
        })
        .catch(error => {
            console.error('Hata:', error);
            displayErrorMessage();

        });
}

function updateMemoryInfo(data) {
    document.getElementById("usedMemory").innerText = "Kullanılan Bellek: " + data.usedMemory + " MB";
    document.getElementById("totalMemory").innerText = "Toplam Bellek: " + data.totalMemory + " MB";
    document.getElementById("usagePercentage").innerText = "Kullanım Yüzdesi: " +
        data.usagePercentage.toFixed(2) + "%";


    var memoryStatusCircleElement = document.getElementById("memoryStatusCircle");
    var statusBadgeElement = document.getElementById("statusBadge");

    if (data.status === "Red") {
        statusBadgeElement.innerHTML = '<span class="badge rounded-pill text-danger">' +
            'Usage Percentage: ' + data.usagePercentage.toFixed(2) + '%</span>';
    } else {
        statusBadgeElement.innerHTML = '<span class="badge rounded-pill text-success">' +
            'Usage Percentage: ' + data.usagePercentage.toFixed(2) + '%</span>';
    }

    document.getElementById("loadingMessage").innerText = "";
    document.getElementById("loadingMessage").style.display = "none";
    document.getElementById("memoryInfo").style.display = "block";
}

function displayErrorMessage() {
    document.getElementById("loadingMessage").innerText = "Bir hata oluştu. Bellek kullanımı alınamadı.";
    document.getElementById("loadingMessage").style.display = "block";
    document.getElementById("memoryInfo").style.display = "none";
}

function createMemoryPieChart(usedMemory, freeMemory) {
    var ctx = document.getElementById("memoryPieChart").getContext("2d");
    var memoryData = {
        labels: ["Used Memory", "Free Memory"],
        datasets: [{
            data: [usedMemory, freeMemory],
            backgroundColor: ["#8B0000", "#FFC0CB"],
            borderColor: ["#FFFFFF", "#000000"], // Çizgi renkleri
            borderWidth: 1, // Çizgi kalınlığı
        }],
    };

    new Chart(ctx, {
        type: 'pie',
        data: memoryData,
        options: {
            maintainAspectRatio: false, // Boyutları korumamayı ayarlar
            responsive: true, // Duyarlılık (sayfa boyutu değiştikçe grafik boyutunu ayarlar)
            legend: {
                display: true, // Göstergeyi göster/gizle
                position: 'bottom', // Etiketleri grafik altında göster
                labels: {
                    fontColor: 'black', // Etiket renkleri
                },
            },
            elements: {
                arc: {
                    borderColor: '#FFFFFF', // Dilim çizgi rengi
                    borderWidth: 2, // Dilim çizgi kalınlığı
                },
            },
        },
    });
}



