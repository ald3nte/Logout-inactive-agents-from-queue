using LogoutUnactiveAgentsCustomWorker.Repositories;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace LogoutUnactiveAgentsCustomWorker
{
    public class App
    {
        private readonly ITCXRepository _tcxRepository;
        private readonly ILogger<App> _logger;

        public App(ITCXRepository tcxRepository,ILogger<App> logger)
        {
            _tcxRepository = tcxRepository;
            _logger = logger;
        }

        public void Run()
        {
            _logger.LogInformation("Starting program..");
            _tcxRepository.StartLiteningChanges();
            Console.ReadKey();

           
        }
    }
}
