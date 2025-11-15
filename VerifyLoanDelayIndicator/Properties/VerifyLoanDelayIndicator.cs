using System;
using System.Data;
using BankingApp.Core.Application.Interfaces;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace VerifyLoanDelayIndicator;

public class VerifyLoanDelayIndicator
{
    private readonly ILogger _logger;
    private ILoanServiceForWebApp _loanService;
    public VerifyLoanDelayIndicator(ILoggerFactory loggerFactory, ILoanServiceForWebApp loanService)
    {
        _logger = loggerFactory.CreateLogger<VerifyLoanDelayIndicator>();
        _loanService = loanService;
    }

    [Function("VerifyLoanDelayIndicator")]
    public async Task Run([TimerTrigger("%TimeTrigger%")] TimerInfo myTimer)
    {
        _logger.LogInformation("C# Timer trigger function executed at: {executionTime}", DateTime.Now);
        
        if (myTimer.ScheduleStatus is not null)
        {
            _logger.LogInformation("Next timer schedule at: {nextSchedule}", myTimer.ScheduleStatus.Next);
        }

        if (myTimer.IsPastDue)
        {

        }
        else
        {
            await _loanService.VerifyAndMarkDelayedLoansAsync();


        }
    }
}