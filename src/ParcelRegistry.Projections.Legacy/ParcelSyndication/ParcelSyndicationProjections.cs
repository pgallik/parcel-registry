namespace ParcelRegistry.Projections.Legacy.ParcelSyndication
{
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore;
    using NodaTime;
    using Parcel.Events;

    public class ParcelSyndicationProjections : ConnectedProjection<LegacyContext>
    {
        public ParcelSyndicationProjections()
        {
            When<Envelope<ParcelWasRegistered>>(async (context, message, ct) =>
            {
                var parcelSyndicationItem = new ParcelSyndicationItem
                {
                    Position = message.Position,
                    ParcelId = message.Message.ParcelId,
                    CaPaKey = message.Message.VbrCaPaKey,
                    RecordCreatedAt = Instant.FromDateTimeUtc(message.CreatedUtc.ToUniversalTime()),
                    LastChangedOn = Instant.FromDateTimeUtc(message.CreatedUtc.ToUniversalTime()),
                    ChangeType = message.EventName,
                    IsComplete = true,
                };

                parcelSyndicationItem.ApplyProvenance(message.Message.Provenance);

                await context
                    .ParcelSyndication
                    .AddAsync(parcelSyndicationItem, ct);
            });

            When<Envelope<ParcelWasRealized>>(async (context, message, ct) =>
            {
                await context.CreateNewParcelSyndicationItem(
                    message.Message.ParcelId,
                    message,
                    x => x.Status = ParcelStatus.Realized,
                    ct);
            });

            When<Envelope<ParcelWasCorrectedToRealized>>(async (context, message, ct) =>
            {
                await context.CreateNewParcelSyndicationItem(
                    message.Message.ParcelId,
                    message,
                    x => x.Status = ParcelStatus.Realized,
                    ct);
            });

            When<Envelope<ParcelWasRetired>>(async (context, message, ct) =>
            {
                await context.CreateNewParcelSyndicationItem(
                    message.Message.ParcelId,
                    message,
                    x => x.Status = ParcelStatus.Retired,
                    ct);
            });

            When<Envelope<ParcelWasCorrectedToRetired>>(async (context, message, ct) =>
            {
                await context.CreateNewParcelSyndicationItem(
                    message.Message.ParcelId,
                    message,
                    x => x.Status = ParcelStatus.Retired,
                    ct);
            });

            When<Envelope<ParcelWasRemoved>>(async (context, message, ct) =>
            {
                await context.CreateNewParcelSyndicationItem(
                    message.Message.ParcelId,
                    message,
                    x => { },
                    ct);
            });

            When<Envelope<ParcelAddressWasAttached>>(async (context, message, ct) =>
            {
                await context.CreateNewParcelSyndicationItem(
                    message.Message.ParcelId,
                    message,
                    x => x.AddAddressId(message.Message.AddressId),
                    ct);
            });

            When<Envelope<ParcelAddressWasDetached>>(async (context, message, ct) =>
            {
                await context.CreateNewParcelSyndicationItem(
                    message.Message.ParcelId,
                    message,
                    x => x.RemoveAddressId(message.Message.AddressId),
                    ct);
            });
        }
    }
}