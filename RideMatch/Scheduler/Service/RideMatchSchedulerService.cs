using RideMatch.Core.Interfaces;
using RideMatch.Core.Models;
using RideMatch.Data.DbContext;
using RideMatch.Data.Repository;
using RideMatch.Services;
using RideMatch.Services.MapServices;
using RideMatch.Services.RouteServices;
using RideMatch.Services.SchedulingServices;
using RideMatch.Services.UserServices;
using System;
using System.IO;
using System.Linq;
using System.ServiceProcess;
using System.Threading.Tasks;
using System.Timers;

namespace RideMatch.Scheduler.Service
{
    public class RideMatchSchedulerService : ServiceBase
    {
        private Timer _checkTimer;
        private readonly int _checkIntervalMinutes = 1; // Check every minute
        private readonly string _logFilePath;

        // Services needed for scheduling
        private RideMatchDbContext _dbContext;
        private SettingsRepository _settingsRepository;
        private IUserService _userService;
        private IVehicleService _vehicleService;
        private IPassengerService _passengerService;
        private IMapService _mapService;
        private IRouteService _routeService;
        private IRoutingService _routingService;
        private ISchedulerService _schedulerService;

        // Service constructor
        public RideMatchSchedulerService()
        {
            ServiceName = "RideMatchScheduler";
            CanStop = true;
            CanPauseAndContinue = true;
            AutoLog = true;

            // Set log file path
            string appDataPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "RideMatch");

            // Create directory if it doesn't exist
            if (!Directory.Exists(appDataPath))
            {
                Directory.CreateDirectory(appDataPath);
            }

            _logFilePath = Path.Combine(appDataPath, "scheduler_service.log");

            // Initialize the component
            InitializeComponent();
        }

        // Initializes components
        private void InitializeComponent()
        {
            // Initialize timer
            _checkTimer = new Timer();
            _checkTimer.Interval = _checkIntervalMinutes * 60 * 1000; // Convert to milliseconds
            _checkTimer.Elapsed += CheckScheduleTime;
            _checkTimer.AutoReset = true;
        }

        // Called when service starts
        protected override void OnStart(string[] args)
        {
            try
            {
                Log("RideMatch Scheduler Service starting...");

                // Initialize database context and repositories
                InitializeServices();

                // Start the timer
                _checkTimer.Start();

                Log("Service started successfully");
            }
            catch (Exception ex)
            {
                Log($"Error starting service: {ex.Message}");

                // Stop the service if initialization failed
                this.Stop();
            }
        }

        // Called when service stops
        protected override void OnStop()
        {
            try
            {
                // Stop the timer
                if (_checkTimer != null)
                {
                    _checkTimer.Stop();
                    _checkTimer.Dispose();
                }

                // Clean up resources
                _dbContext?.Dispose();

                Log("Service stopped");
            }
            catch (Exception ex)
            {
                Log($"Error stopping service: {ex.Message}");
            }
        }

        // Called when service is paused
        protected override void OnPause()
        {
            try
            {
                // Pause the timer
                if (_checkTimer != null)
                {
                    _checkTimer.Stop();
                }

                Log("Service paused");
            }
            catch (Exception ex)
            {
                Log($"Error pausing service: {ex.Message}");
            }
        }

        // Called when service is resumed
        protected override void OnContinue()
        {
            try
            {
                // Resume the timer
                if (_checkTimer != null)
                {
                    _checkTimer.Start();
                }

                Log("Service resumed");
            }
            catch (Exception ex)
            {
                Log($"Error resuming service: {ex.Message}");
            }
        }

        // Called when system is shutting down
        protected override void OnShutdown()
        {
            try
            {
                // Clean up resources
                if (_checkTimer != null)
                {
                    _checkTimer.Stop();
                    _checkTimer.Dispose();
                }

                _dbContext?.Dispose();

                Log("Service shut down due to system shutdown");
            }
            catch (Exception ex)
            {
                Log($"Error during shutdown: {ex.Message}");
            }

            base.OnShutdown();
        }

        // Checks if it's time to run the scheduler
        private async void CheckScheduleTime(object sender, ElapsedEventArgs e)
        {
            try
            {
                // Get current time
                DateTime now = DateTime.Now;

                // Get scheduling settings
                var settings = await _settingsRepository.GetSchedulingSettingsAsync();

                if (!settings.IsEnabled)
                {
                    // Scheduling is disabled
                    return;
                }

                // Convert scheduled time to today's date
                DateTime scheduledTime = now.Date.Add(settings.ScheduledTime.TimeOfDay);

                // Check if it's time to run (within the last minute)
                TimeSpan diff = now - scheduledTime;
                if (diff >= TimeSpan.Zero && diff <= TimeSpan.FromMinutes(_checkIntervalMinutes))
                {
                    Log($"Scheduled time reached. Running algorithm at {now}");

                    // Run the algorithm
                    await RunAlgorithmAsync();
                }
            }
            catch (Exception ex)
            {
                Log($"Error in schedule check: {ex.Message}");
            }
        }

        // Runs the algorithm
        private async Task RunAlgorithmAsync()
        {
            try
            {
                // Create a new schedule task with all dependencies
                var task = new ScheduleTask(
                    _vehicleService,
                    _passengerService,
                    _routeService,
                    _routingService,
                    _settingsRepository);

                // Execute the scheduling task
                bool success = await task.ExecuteAsync();

                if (success)
                {
                    Log("Scheduling algorithm completed successfully");
                }
                else
                {
                    Log("Scheduling algorithm failed");
                }
            }
            catch (Exception ex)
            {
                Log($"Error running algorithm: {ex.Message}");

                // Log the error in the scheduling log
                await _settingsRepository.LogSchedulingRunAsync(
                    DateTime.Now,
                    "Failed",
                    0,
                    0,
                    $"Error: {ex.Message}");
            }
        }

        // Calculates pickup times based on target arrival time
        private async Task CalculatePickupTimesBasedOnTargetArrival(Solution solution, string targetTimeString, RoutingService routingService)
        {
            if (solution == null || string.IsNullOrEmpty(targetTimeString))
                return;

            try
            {
                // Parse target time string to minutes from midnight
                int targetTimeMinutes = GetTargetTimeInMinutes(targetTimeString);

                // Convert to DateTime
                DateTime targetTime = DateTime.Today.AddMinutes(targetTimeMinutes);

                // Get destination information
                var destinationInfo = await _settingsRepository.GetDestinationAsync();

                foreach (var vehicle in solution.Vehicles.Where(v => v.Passengers.Count > 0))
                {
                    // Calculate total route time
                    double totalTime = vehicle.TotalTime;

                    // Set departure time based on target arrival
                    DateTime departureTime = targetTime.AddMinutes(-totalTime);

                    // Starting location is vehicle location
                    double currentLat = vehicle.Latitude;
                    double currentLng = vehicle.Longitude;

                    double cumulativeTime = 0;

                    // Calculate pickup times for each passenger
                    for (int i = 0; i < vehicle.Passengers.Count; i++)
                    {
                        var passenger = vehicle.Passengers[i];

                        // Calculate time from current location to passenger
                        double legDistance = CalculateDistance(
                            currentLat, currentLng,
                            passenger.Latitude, passenger.Longitude);

                        double legTime = (legDistance / 30) * 60; // minutes

                        cumulativeTime += legTime;

                        // Set pickup time
                        passenger.PickupTime = departureTime.AddMinutes(cumulativeTime);

                        // Update current location for next passenger
                        currentLat = passenger.Latitude;
                        currentLng = passenger.Longitude;
                    }
                }
            }
            catch (Exception ex)
            {
                Log($"Error calculating pickup times: {ex.Message}");
            }
        }

        // Converts target time to minutes
        private int GetTargetTimeInMinutes(string targetTime)
        {
            if (string.IsNullOrEmpty(targetTime))
                return 0;

            // Format is expected to be "HH:MM"
            string[] parts = targetTime.Split(':');
            if (parts.Length != 2)
                return 0;

            if (!int.TryParse(parts[0], out int hours) || !int.TryParse(parts[1], out int minutes))
                return 0;

            return hours * 60 + minutes;
        }

        // Logs a message to the service log file
        private void Log(string message)
        {
            try
            {
                // Append to log file
                using (StreamWriter writer = File.AppendText(_logFilePath))
                {
                    writer.WriteLine($"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - {message}");
                }
            }
            catch
            {
                // Ignore logging errors
            }
        }

        // Initialize services
        private void InitializeServices()
        {
            try
            {
                // Initialize database context
                _dbContext = new RideMatchDbContext();

                // Initialize repositories
                var userRepository = new UserRepository(_dbContext);
                var vehicleRepository = new VehicleRepository(_dbContext);
                var passengerRepository = new PassengerRepository(_dbContext);
                var routeRepository = new RouteRepository(_dbContext);
                _settingsRepository = new SettingsRepository(_dbContext);

                // Get API key from settings
                string apiKey = _settingsRepository.GetSettingAsync("GoogleMapsApiKey", "").Result;

                // Initialize services
                _userService = new Services.UserServices.UserService(userRepository);
                _vehicleService = new VehicleService(vehicleRepository);
                _passengerService = new PassengerService(passengerRepository);
                _mapService = new MapService(apiKey);
                _routeService = new RouteService(routeRepository, null, _settingsRepository);
                _routingService = new RoutingService(_mapService, _settingsRepository);

                // Resolve circular reference
                ((RouteService)_routeService).RoutingService = _routingService;

                // Initialize scheduler service
                _schedulerService = new SchedulingService(
                    _settingsRepository,
                    _routeService,
                    _vehicleService,
                    _passengerService,
                    _routingService);

                Log("Services initialized successfully");
            }
            catch (Exception ex)
            {
                Log($"Error initializing services: {ex.Message}");
                throw;
            }
        }

        // Helper method to calculate distance between two points
        private double CalculateDistance(double lat1, double lon1, double lat2, double lon2)
        {
            const double EarthRadius = 6371; // Radius in kilometers

            var dLat = ToRadians(lat2 - lat1);
            var dLon = ToRadians(lon2 - lon1);

            var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                   Math.Cos(ToRadians(lat1)) * Math.Cos(ToRadians(lat2)) *
                   Math.Sin(dLon / 2) * Math.Sin(dLon / 2);

            var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

            return EarthRadius * c;
        }

        // Helper method to convert degrees to radians
        private double ToRadians(double degrees)
        {
            return degrees * Math.PI / 180;
        }
    }
}