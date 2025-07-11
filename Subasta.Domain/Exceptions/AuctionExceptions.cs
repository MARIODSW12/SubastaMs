

namespace Subasta.Domain.Exceptions
{
    public class InvalidStatusException : Exception
    {
        public InvalidStatusException() : base("Estado de subasta invalido") { }
    }

    public class InvalidBasePriceException : Exception
    {
        public InvalidBasePriceException() : base("El precio base debe ser mayor a 0") { }
    }

    public class InvalidDescriptionException : Exception
    {
        public InvalidDescriptionException() : base("La descripcion no puede estar vacia") { }
    }

    public class InvalidDurationException : Exception
    {
        public InvalidDurationException() : base("La duracion debe ser mayor a 0") { }
    }

    public class InvalidIdException : Exception
    {
        public InvalidIdException() : base("El id de la subasta es invalido") { }
    }

    public class InvalidPrizeIdException : Exception
    {
        public InvalidPrizeIdException() : base("El id del premio es invalido") { }
    }

    public class InvalidMinimumIncreaseException : Exception
    {
        public InvalidMinimumIncreaseException() : base("El aumento minimo debe ser mayor a 0") { }
    }

    public class InvalidNameException : Exception
    {
        public InvalidNameException() : base("El nombre de la subasta no puede estar vacio") { }
    }

    public class InvalidReservePriveException : Exception
    {
        public InvalidReservePriveException() : base("El precio de reserva no puede ser menor a 0") { }
    }

    public class InvalidUserIdException : Exception
    {
        public InvalidUserIdException() : base("El id del usuario es invalido") { }
    }

    public class InvalidPrizeUserIdException : Exception
    {
        public InvalidPrizeUserIdException() : base("El id del usuario del premio es invalido") { }
    }

    public class InvalidDeliverDirectionException : Exception
    {
        public InvalidDeliverDirectionException() : base("La direccion de entrega no puede estar vacia") { }
    }

    public class InvalidDeliverMethodException : Exception
    {
        public InvalidDeliverMethodException() : base("El metodo de entrega es inválido") { }
    }

    public class InvalidAuctionPricesException : Exception
    {
        public InvalidAuctionPricesException() : base("El precio base no puede ser mayor al de reserva") { }
    }

    public class InvalidAuctionProductQuantityException : Exception
    {
        public InvalidAuctionProductQuantityException() : base("La cantidad del producto no puede ser menor a 0") { }
    }

    public class InvalidAuctionProductIdException : Exception
    {
        public InvalidAuctionProductIdException() : base("La id del producto no puede estar vacio") { }
    }

    public class InvalidStartDateException : Exception
    {
        public InvalidStartDateException() : base("La fecha de inicio no puede ser menor o igual a hoy") { }
    }

    public class MongoDBConnectionException : Exception 
    {
        public MongoDBConnectionException() : base("Error al conectar con la base de datos de mongo") { }
    }

    public class MongoDBUnnexpectedException : Exception
    {
        public MongoDBUnnexpectedException() : base("Error inesperado con la base de datos de mongo") { }
    }
}
