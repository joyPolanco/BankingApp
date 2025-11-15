using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BankingApp.Core.Application.Dtos.Operations
{
    public class OperationResultDto
    {
        public bool IsSuccessful { get; set; }
        public string StatusMessage { get; set; }
        public bool IsInternalError { get; set; }
    }
}
