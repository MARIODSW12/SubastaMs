using MongoDB.Bson;

using Subasta.Domain.Events;

using Subasta.Infrastructure.Interfaces;
using RestSharp;
using MediatR;

namespace Subasta.Infrastructure.Consumer
{
    public class SendNotificationConsumer(IServiceProvider serviceProvider, IRestClient restClient) : INotificationHandler<NotificationSendEvent>
    {
        private readonly IServiceProvider _serviceProvider = serviceProvider;
        private readonly IRestClient RestClient = restClient;

        public async Task Handle(NotificationSendEvent message, CancellationToken cancellationToken)
        {
            try
            {
                Console.WriteLine("noti enviando");
                var APIRequest = new RestRequest(Environment.GetEnvironmentVariable("NOTIFICACION_MS_URL") + "/enviar", Method.Post);
                APIRequest.AddJsonBody(new
                {
                    IdsUsuarios = message.IdsUsuarios,
                    Motivo = message.Motivo,
                    Cuerpo = message.Cuerpo
                });
                var APIResponse = await RestClient.ExecuteAsync(APIRequest);
                Console.WriteLine($"Enviando notificación a los usuarios: {string.Join(", ", message.IdsUsuarios)}");
                if (!APIResponse.IsSuccessful)
                {
                    throw new Exception("Error al obtener la información del usuario.");
                }
            }
            catch (Exception ex)
            {
                throw;
            }
        }
    }
}