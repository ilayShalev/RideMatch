using RideMatch.Core.Interfaces;
using RideMatch.Data.DbContext;
using RideMatch.Data.Repository;
using RideMatch.Services;
using RideMatch.Services.MapServices;
using RideMatch.Services.RouteServices;
using RideMatch.Services.SchedulingServices;
using RideMatch.Services.UserServices;
using RideMatch.UI.Forms;
using RideMatch.UI.Helpers;
using System;
using System.Windows.Forms;

namespace RideMatch
{
    internal static class Program
    {
        // API key for map services
        private const string GoogleMapsApiKey = "YOUR_GOOGLE_MAPS_API_KEY";

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            try
            {
                // Initialize the database context
                var dbContext = new RideMatchDbContext();

                // Initialize repositories
                var userRepository = new UserRepository(dbContext);
                var vehicleRepository = new VehicleRepository(dbContext);
                var passengerRepository = new PassengerRepository(dbContext);
                var routeRepository = new RouteRepository(dbContext);
                var settingsRepository = new SettingsRepository(dbContext);

                // Initialize services
                IUserService userService = new UserService(userRepository);
                IVehicleService vehicleService = new VehicleService(vehicleRepository);
                IPassengerService passengerService = new PassengerService(passengerRepository);
                IMapService mapService = new MapService(GoogleMapsApiKey);
                IRouteService routeService = new RouteService(routeRepository, null, settingsRepository);
                IRoutingService routingService = new RoutingService(mapService, settingsRepository);
                ISchedulerService schedulerService = new SchedulingService(
                    settingsRepository, routeService, vehicleService, passengerService, routingService);

                // Update route service with routing service (circular reference)
                ((RouteService)routeService).RoutingService = routingService;

                // Initialize the control factory
                ControlFactory.InitializeServices(
                    userService,
                    vehicleService,
                    passengerService,
                    mapService,
                    routeService,
                    routingService,
                    schedulerService,
                    settingsRepository);

                // Create and show the login form
                var loginForm = new LoginForm(userService);
                Application.Run(loginForm);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error initializing application: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}