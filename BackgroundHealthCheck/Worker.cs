using System.ServiceProcess;

namespace BackgroundHealthCheck
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;

        private List<string> _servicesToMonitor = new List<string>
        {
            "Görev Zamanlayýcý", "Otomatik Saat Dilimi Güncelleþtirici"
        };
        public Worker(ILogger<Worker> logger)
        {
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("Checking services...");

                foreach (var serviceName in _servicesToMonitor)
                {
                    CheckAndRestartService(serviceName);

                }

                _logger.LogInformation("Checking complete.Waiting for the next check...");


                await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
            }

        }

        private void CheckAndRestartService(string serviceName)
        {
            ServiceController service = new ServiceController(serviceName);

            if(service.Status != ServiceControllerStatus.Running)
            {
                try
                {
                    TimeSpan timeout = TimeSpan.FromMilliseconds(3000);

                    service.Refresh();
                    service.Stop();
                    service.WaitForStatus(ServiceControllerStatus.Stopped, timeout);

                    service.Start();
                    service.WaitForStatus(ServiceControllerStatus.Running, timeout);

                    _logger.LogInformation($"{serviceName} has been restarted.");

                }
                catch (InvalidOperationException invalidOpEx)
                {
                    _logger.LogError($"Service {serviceName} could not be found: {invalidOpEx.Message}");
                }
                catch (Exception e)
                {
                    _logger.LogError($"Could not restart service {serviceName}:{e.Message}");
                    
                }

            }

        }
    }


}