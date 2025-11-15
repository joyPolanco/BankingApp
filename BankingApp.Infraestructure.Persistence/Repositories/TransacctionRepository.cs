using BankingApp.Core.Domain.Common.Enums;
using BankingApp.Core.Domain.Entities;
using BankingApp.Infraestructure.Persistence.Contexts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace BankingApp.Infraestructure.Persistence.Repositories
{
    public class TransacctionRepository : GenericRepository<Transaction>, ITransacctionRepository
    {

        private readonly BankingContext context;

        public TransacctionRepository(BankingContext context) : base(context)
        {

            this.context = context;
        }




        public async Task<bool> ApproveTransaction(int id, Transaction transaction)
        {
            var entry = await context.Set<Transaction>().FindAsync(id);

            if (entry != null)
            {

                entry.Status = OperationStatus.APPROVED;
                await context.SaveChangesAsync();
                return true;
            }

            return false;
        }



        public async Task<bool> DeclieneTransaction(int id, Transaction transaction)
        {
            var entry = await context.Set<Transaction>().FindAsync(id);

            if (entry != null)
            {

                entry.Status = OperationStatus.DECLINED;
                await context.SaveChangesAsync();
                return true;
            }

            return false;
        }


        public async Task<bool> MarkAsCredit(int id, Transaction transaction)
        {
            var entry = await context.Set<Transaction>().FindAsync(id);

            if (entry != null)
            {

                entry.Type = TransactionType.CREDIT;
                await context.SaveChangesAsync();
                return true;
            }

            return false;
        }



        public async Task<bool> MarkAsDebit(int id, Transaction transaction)
        {

            var entry = await context.Set<Transaction>().FindAsync(id);

            if (entry != null)
            {

                entry.Type = TransactionType.DEBIT;
                await context.SaveChangesAsync();
                return true;
            }

            return false;
        }




    }
}
