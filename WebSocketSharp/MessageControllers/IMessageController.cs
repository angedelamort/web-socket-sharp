using System.Threading.Tasks;

namespace WebSocketSharp.MessageControllers
{
    /// <summary>
    /// Interface for the factory. For internal use only. Use MessageController
    /// </summary>
    public interface IMessageController
    {
        Task<IMessageResponse> ProcessRequestAsync(IMessageRequest messageRequest);
        Task ProcessResponseAsync(IMessageResponse messageRequest);
    }
}
