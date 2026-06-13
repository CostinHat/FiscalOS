using FiscalOS.Domain.LegalReferences;
using FiscalOS.LegalCore;

namespace FiscalOS.Runtime.LegalReferences;

public sealed class InMemoryLegalReferenceResolutionAuditRepository : ILegalReferenceResolutionAuditRepository
{
    private readonly object _sync = new();
    private readonly List<ResolutionAuditEntry> _entries = new();

    public Task StoreAsync(
        ResolutionAuditTrail trail,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(trail);
        cancellationToken.ThrowIfCancellationRequested();

        lock (_sync)
        {
            _entries.AddRange(trail.Entries);
        }

        return Task.CompletedTask;
    }

    public Task<ResolutionAuditTrail?> GetAsync(
        LegalReference query,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query);
        cancellationToken.ThrowIfCancellationRequested();

        lock (_sync)
        {
            var matching = _entries.Where(entry => entry.Query.Equals(query)).ToList();
            return Task.FromResult<ResolutionAuditTrail?>(
                matching.Count == 0 ? null : new ResolutionAuditTrail(matching));
        }
    }

    public Task<IReadOnlyList<ResolutionAuditEntry>> GetByDecisionStatusAsync(
        ResolutionStatus status,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        lock (_sync)
        {
            return Task.FromResult<IReadOnlyList<ResolutionAuditEntry>>(
                _entries.Where(entry => entry.Decision.Status == status).ToList());
        }
    }

    public Task<IReadOnlyList<ResolutionAuditEntry>> GetUnresolvedAsync(
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        lock (_sync)
        {
            return Task.FromResult<IReadOnlyList<ResolutionAuditEntry>>(
                _entries.Where(entry => entry.Decision.Status == ResolutionStatus.Unresolved).ToList());
        }
    }
}
