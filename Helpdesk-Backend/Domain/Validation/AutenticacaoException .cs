namespace Domain.Validation
{
    public class AutenticacaoException : Exception
    {
        public AutenticacaoException(string message)
            : base(message)
        {
        }
    }
}