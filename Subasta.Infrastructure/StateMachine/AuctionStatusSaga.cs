using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MassTransit;
using Subasta.Application.StateMachine;

namespace Subasta.Infrastructure.StateMachine
{
    public class AuctionStatusSaga : MassTransitStateMachine<AuctionStatusSagaData>
    {
        public State Pending { get; set; }
        public State Active { get; set; }
        public State Ended { get; set; }
        public State Deserted { get; set; }
        public State Completed { get; set; }
        public State Canceled { get; set; }
        public State Delivered { get; set; }

        public Event<AuctionCreated> AuctionCreated { get; set; }
        public Event<AuctionStarted> AuctionStarted { get; set; }
        public Event<AuctionFinished> AuctionFinished { get; set; }
        public Event<AuctionDeserted> AuctionDeserted { get; set; }
        public Event<AuctionCanceled> AuctionCanceled { get; set; }
        public Event<AuctionActived> AuctionActived { get; set; }
        public Event<PaymentReceived> PaymentReceived { get; set; }
        public Event<PrizeDelivered> PrizeDelivered { get; set; }


        public AuctionStatusSaga()
        {
            InstanceState(x => x.CurrentState);

            Event(() => AuctionCreated, x => x.CorrelateById(context => context.Message.AuctionId));
            Event(() => AuctionStarted, x => x.CorrelateById(context => context.Message.AuctionId));
            Event(() => AuctionFinished, x => x.CorrelateById(context => context.Message.AuctionId));
            Event(() => AuctionDeserted, x => x.CorrelateById(context => context.Message.AuctionId));
            Event(() => AuctionCanceled, x => x.CorrelateById(context => context.Message.AuctionId));
            Event(() => AuctionActived, x => x.CorrelateById(context => context.Message.AuctionId));
            Event(() => PaymentReceived, x => x.CorrelateById(context => context.Message.AuctionId));
            Event(() => PrizeDelivered, x => x.CorrelateById(context => context.Message.AuctionId));

            Initially(
                When(AuctionCreated)
                    .Then(context =>
                    {
                        context.Saga.CorrelationId = context.Message.AuctionId;
                        context.Saga.CreatedAt = DateTime.UtcNow;
                    })
                .TransitionTo(Pending)
            );

            During(Pending,
                When(AuctionStarted)
                .Then(
                    context => context.Saga.UpdatedAt = DateTime.UtcNow
                    )
                .TransitionTo(Active)
            );

            During(Active,
                When(AuctionFinished)
                .Then(
                    context => context.Saga.UpdatedAt = DateTime.UtcNow
                    )
                .TransitionTo(Ended)
            );

            During(Active,
                When(AuctionDeserted)
                .Then(
                    context => context.Saga.UpdatedAt = DateTime.UtcNow
                    )
                .TransitionTo(Deserted)
            );

            During(Pending,
                When(AuctionCanceled)
                .Then(
                    context => context.Saga.UpdatedAt = DateTime.UtcNow
                    )
                .TransitionTo(Canceled)
            );

            During(Canceled,
                When(AuctionActived)
                .Then(
                    context => context.Saga.UpdatedAt = DateTime.UtcNow
                    )
                .TransitionTo(Pending)
            );

            During(Ended,
                When(PaymentReceived)
                .Then(
                    context => context.Saga.UpdatedAt = DateTime.UtcNow
                    )
                .TransitionTo(Completed)
            );

            During(Ended,
                When(AuctionDeserted)
                .Then(
                    context => context.Saga.UpdatedAt = DateTime.UtcNow
                    )
                .TransitionTo(Deserted)
            );

            During(Completed,
                When(PrizeDelivered)
                .Then(
                    context => context.Saga.UpdatedAt = DateTime.UtcNow
                    )
                .TransitionTo(Delivered)
            );

        }
    }
}
