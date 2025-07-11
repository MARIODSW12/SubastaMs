using MediatR;

namespace Subasta.Domain.Events
{
    public class NotificationSendEvent : INotification
    {
        public List<string> IdsUsuarios { get; }
        public string Motivo { get; }
        public string Cuerpo { get; }
        public NotificationSendEvent(List<string> idsUsuarios, string motivo, string cuerpo)
        {
            IdsUsuarios = idsUsuarios;
            Motivo = motivo;
            Cuerpo = cuerpo;
        }
    }
}
