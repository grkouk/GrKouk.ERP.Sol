using System;

namespace GrKouk.Erp.Domain.Shared;

/// <summary>
/// A shared catalog row the shops pull by the time the hub received it.
/// <see cref="ModifiedAt"/> is the editing shop's clock and still decides which of two edits wins.
/// <see cref="ReceivedAt"/> is the hub's clock when it stored the change: the pull asks by it, so an
/// edit that reaches the hub late is still pulled by a shop that synced in between.
/// </summary>
public interface IHubReceivedStamp
{
    DateTime ModifiedAt { get; set; }

    DateTime ReceivedAt { get; set; }
}
