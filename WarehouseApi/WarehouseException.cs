namespace WarehouseApi
{
    // Custom exception class for WarehouseManager errors
    public class WarehouseException : Exception
    {
        public object ErrorObject { get; }

        public WarehouseException(object errorObject)
        {
            ErrorObject = errorObject;
        }
    }
}
