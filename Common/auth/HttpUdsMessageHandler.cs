// Copyright(c) Microsoft Corporation.
// Licensed under the MIT license.

using System;
using System.Net.Http;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Azure.EventGridEdge.Samples.Common.Auth
{
    /// <summary>
    /// Unix domain message handler.
    /// </summary>
    public class HttpUdsMessageHandler : HttpMessageHandler
    {
        private readonly Uri providerUri;

        public HttpUdsMessageHandler(Uri providerUri)
        {
            this.providerUri = providerUri;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            using (Socket socket = await this.GetConnectedSocketAsync())
            {
                using (var stream = new HttpBufferedStream(new NetworkStream(socket, true)))
                {
                    var serializer = new HttpSerializer();
                    byte[] requestBytes = serializer.SerializeRequest(request);

                    await stream.WriteAsync(requestBytes, 0, requestBytes.Length, cancellationToken);
                    if (request.Content != null)
                    {
                        await request.Content.CopyToAsync(stream);
                    }

                    return await serializer.DeserializeResponseAsync(stream, cancellationToken);
                }
            }
        }

        private async Task<Socket> GetConnectedSocketAsync()
        {
            var endpoint = new UnixDomainSocketEndPoint(this.providerUri.LocalPath);
            var socket = new Socket(AddressFamily.Unix, SocketType.Stream, ProtocolType.Unspecified);
            await socket.ConnectAsync(endpoint);
            return socket;
        }
    }
}
