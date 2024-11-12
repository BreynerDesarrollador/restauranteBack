namespace RestauranteBack.WebApiRestaurante.ClasesGenerales
{
    public class ExcepcionPeticionApi : Exception
    {
        private int _codigoError;

        public int CodigoError => _codigoError;

        public ExcepcionPeticionApi()
        {
        }

        public ExcepcionPeticionApi(string message, int codigoError)
            : base(message)
        {
            _codigoError = codigoError;
        }

        public ExcepcionPeticionApi(string message, int codigoError, Exception? innerException)
            : base(message, innerException)
        {
            _codigoError = codigoError;
        }
    }
}
