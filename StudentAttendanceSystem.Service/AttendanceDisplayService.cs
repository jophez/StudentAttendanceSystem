using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using StudentAttendanceSystem.Data;
using StudentAttendanceSystem.Service.Forms;

namespace StudentAttendanceSystem.Service
{
    public class AttendanceDisplayService : BackgroundService
    {
        private readonly ILogger<AttendanceDisplayService> _logger;
        private readonly DatabaseConnection _dbConnection;
        private AttendanceDisplayForm? _displayForm;
        private Thread? _uiThread;

        public AttendanceDisplayService(ILogger<AttendanceDisplayService> logger, DatabaseConnection dbConnection)
        {
            _logger = logger;
            _dbConnection = dbConnection;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Attendance Display Service starting...");

            try
            {
                // Start the UI thread
                _uiThread = new Thread(() =>
                {
                    Application.EnableVisualStyles();
                    Application.SetCompatibleTextRenderingDefault(false);
                    
                    _displayForm = new AttendanceDisplayForm(_dbConnection);
                    Application.Run(_displayForm);
                });

                _uiThread.SetApartmentState(ApartmentState.STA);
                _uiThread.Start();

                // Keep the service running
                while (!stoppingToken.IsCancellationRequested)
                {
                    await Task.Delay(1000, stoppingToken);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in Attendance Display Service");
            }
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Attendance Display Service stopping...");

            if (_displayForm != null)
            {
                _displayForm.Invoke(() => _displayForm.Close());
            }

            if (_uiThread != null && _uiThread.IsAlive)
            {
                _uiThread.Join(5000); // Wait up to 5 seconds for clean shutdown
            }

            await base.StopAsync(cancellationToken);
        }
    }
}