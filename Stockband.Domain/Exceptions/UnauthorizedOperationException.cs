namespace Stockband.Domain.Exceptions;

public class UnauthorizedOperationException:Exception
{
    public UnauthorizedOperationException()
        :base("Unable to access this operation")
    {
        
    }

    public UnauthorizedOperationException(string msg)
        : base(msg)
    {
        
    }
}