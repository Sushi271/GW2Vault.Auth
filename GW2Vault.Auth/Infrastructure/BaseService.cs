using System;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace GW2Vault.Auth.Infrastructure
{
    public abstract class BaseService
    {
        const string Error500_ExceptionThrownWhileExecuting = "Exception thrown while executing the request. Details:\n";

        protected DbContext Context { get; private set; }

        public BaseService(DbContext context)
            => Context = context;

        protected ServiceResponse TryExecute(Func<ServiceResponse> action, bool inTransaction = false)
        {
            try
            {
                if (inTransaction)
                    Context.Database.BeginTransaction();

                var response = action();

                if (inTransaction)
                    if (response.IsSuccess)
                        Context.Database.CommitTransaction();
                    else Context.Database.RollbackTransaction();

                return response;
            }
            catch (Exception ex)
            {
                if (inTransaction)
                    Context.Database.RollbackTransaction();
                return ServiceResponse.Error(new ErrorDetails(StatusCodes.Status500InternalServerError, $"{Error500_ExceptionThrownWhileExecuting}{ex}"));
            }
        }

        protected ServiceResponse<T> TryExecute<T>(Func<ServiceResponse<T>> action, bool inTransaction = false)
        {
            try
            {
                if (inTransaction)
                    Context.Database.BeginTransaction();

                var response = action();

                if (inTransaction)
                    if (response.IsSuccess)
                        Context.Database.CommitTransaction();
                    else Context.Database.RollbackTransaction();

                return response;
            }
            catch (Exception ex)
            {
                if (inTransaction)
                    Context.Database.RollbackTransaction();
                return ServiceResponse<T>.Error(new ErrorDetails(StatusCodes.Status500InternalServerError, $"{Error500_ExceptionThrownWhileExecuting}{ex}"));
            }
        }
    }
}
