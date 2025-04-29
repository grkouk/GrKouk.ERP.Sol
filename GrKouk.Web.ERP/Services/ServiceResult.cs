namespace GrKouk.Web.ERP.Services;

public class ServiceResult
{
    public bool Success { get; private set; }
    public string ErrorMessage { get; private set; }
    public string ErrorCode { get; private set; }
    public object Data { get; private set; }

    private ServiceResult() { }

    public static ServiceResult Ok(object data = null)
    {
        return new ServiceResult
        {
            Success = true,
            Data = data
        };
    }

    public static ServiceResult Error(string errorMessage, string errorCode = null)
    {
        return new ServiceResult
        {
            Success = false,
            ErrorMessage = errorMessage,
            ErrorCode = errorCode
        };
    }
}