using System.Threading.Channels;

namespace Healthcare.Api.Services;

public class DocumentProcessingQueue
{
    private readonly Channel<Guid> _queue;

    public DocumentProcessingQueue()
    {
        _queue = Channel.CreateUnbounded<Guid>(
            new UnboundedChannelOptions
            {
                SingleReader = true,
                SingleWriter = false
            });
    }

    public ValueTask QueueAsync(
        Guid documentId,
        CancellationToken cancellationToken = default)
    {
        return _queue.Writer.WriteAsync(
            documentId,
            cancellationToken);
    }

    public ValueTask<Guid> DequeueAsync(
        CancellationToken cancellationToken)
    {
        return _queue.Reader.ReadAsync(
            cancellationToken);
    }
}
