namespace GW2Vault.Auth.Infrastructure
{
    public class ServiceResponse
    {
        public ErrorDetails ErrorDetails { get; }

        public bool IsSuccess => ErrorDetails == null;

        protected ServiceResponse(ErrorDetails errorDetails)
            => ErrorDetails = errorDetails;

        public ServiceResponse<T> AsGenericResponse<T>(T responseDto = default)
        {
            if (IsSuccess)
                return ServiceResponse<T>.Success(responseDto);
            return ServiceResponse<T>.Error(ErrorDetails);
        }

        public static ServiceResponse Success()
            => new ServiceResponse(null);

        public static ServiceResponse<T> Success<T>(T responseDto)
            => ServiceResponse<T>.Success(responseDto);

        public static ServiceResponse Error(int code, string message)
            => Error(new ErrorDetails(code, message));

        public static ServiceResponse<T> Error<T>(int code, string message)
            => Error<T>(new ErrorDetails(code, message));

        public static ServiceResponse Error(ErrorDetails errorDetails)
            => new ServiceResponse(errorDetails);

        public static ServiceResponse<T> Error<T>(ErrorDetails errorDetails)
            => ServiceResponse<T>.Error(errorDetails);

        public static ServiceResponse BadRequest(string customMessage = null)
            => Error(400, customMessage ?? "Bad Request.");
        
        public static ServiceResponse<T> BadRequest<T>(string customMessage = null)
            => Error<T>(400, customMessage ?? "Bad Request.");

        public static ServiceResponse Unauthorized(string customMessage = null)
            => Error(401, customMessage ?? "Unauthorized.");
        
        public static ServiceResponse<T> Unauthorized<T>(string customMessage = null)
            => Error<T>(401, customMessage ?? "Unauthorized.");

        public static ServiceResponse Forbidden(string customMessage = null)
            => Error(403, customMessage ?? "Forbidden.");
        
        public static ServiceResponse<T> Forbidden<T>(string customMessage = null)
            => Error<T>(403, customMessage ?? "Forbidden.");

        public static ServiceResponse NotFound(string customMessage = null)
            => Error(404, customMessage ?? "Not Found.");
        
        public static ServiceResponse<T> NotFound<T>(string customMessage = null)
            => Error<T>(404, customMessage ?? "Not Found.");

        public static ServiceResponse InternalServerError(string customMessage = null)
            => Error(500, customMessage ?? "Internal Server Error.");
        
        public static ServiceResponse<T> InternalServerError<T>(string customMessage = null)
            => Error<T>(500, customMessage ?? "Internal Server Error.");
    }

    public class ServiceResponse<T> : ServiceResponse
    {
        public T ResponseDTO { get; }

        private ServiceResponse(T responseDTO)
            : base(null)
            => ResponseDTO = responseDTO;

        private ServiceResponse(ErrorDetails errorDetails)
            : base(errorDetails)
        { 
        }

        public static new ServiceResponse<T> Error(int code, string message)
            => Error(new ErrorDetails(code, message));

        public static new ServiceResponse<T> Error(ErrorDetails errorDetails)
            => new ServiceResponse<T>(errorDetails);

        public static ServiceResponse<T> Success(T responseDto)
            => new ServiceResponse<T>(responseDto);
    }
}
