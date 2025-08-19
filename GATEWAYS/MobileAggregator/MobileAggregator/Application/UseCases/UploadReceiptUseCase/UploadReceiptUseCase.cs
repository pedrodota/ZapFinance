using Google.Protobuf;
using Grpc.Core;
using ZapFinance.ProtoServer.Core;

namespace MobileAggregator.Application.UseCases.UploadReceiptUseCase;

public class UploadReceiptUseCase : IUploadReceiptUseCase
{
    private readonly ReceiptService.ReceiptServiceClient _receiptServiceClient;
    private readonly ILogger<UploadReceiptUseCase> _logger;

    public UploadReceiptUseCase(ReceiptService.ReceiptServiceClient receiptServiceClient, ILogger<UploadReceiptUseCase> logger)
    {
        _receiptServiceClient = receiptServiceClient;
        _logger = logger;
    }

    public async Task<UploadReceiptUseCaseOut> ExecuteAsync(UploadReceiptUseCaseIn request)
    {
        try
        {
            // Validar tipo de arquivo
            var allowedTypes = new[] { "image/jpeg", "image/jpg", "image/png", "image/gif", "image/webp" };
            if (!allowedTypes.Contains(request.Arquivo.ContentType.ToLowerInvariant()))
            {
                return new UploadReceiptUseCaseOut
                {
                    Sucesso = false,
                    Mensagem = "Tipo de arquivo não suportado. Use JPEG, PNG, GIF ou WebP"
                };
            }

            // Converter arquivo para bytes
            byte[] fileBytes;
            using (var memoryStream = new MemoryStream())
            {
                await request.Arquivo.CopyToAsync(memoryStream);
                fileBytes = memoryStream.ToArray();
            }

            // Criar request para gRPC
            var grpcRequest = new UploadReceiptRequest
            {
                UsuarioId = request.UsuarioId,
                NomeArquivo = request.Arquivo.FileName,
                ImagemData = ByteString.CopyFrom(fileBytes),
                TipoMime = request.Arquivo.ContentType,
                Descricao = request.Descricao ?? string.Empty,
                Valor = (double)(request.Valor ?? 0),
                Categoria = request.Categoria ?? string.Empty
            };

            // Chamar serviço gRPC
            var response = await _receiptServiceClient.UploadReceiptAsync(grpcRequest);

            return new UploadReceiptUseCaseOut
            {
                Sucesso = true,
                Mensagem = "Upload realizado com sucesso",
                Receipt = MapToReceiptDto(response)
            };
        }
        catch (RpcException ex)
        {
            _logger.LogError(ex, "Erro gRPC ao fazer upload do recibo");
            return new UploadReceiptUseCaseOut
            {
                Sucesso = false,
                Mensagem = ex.Status.Detail
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao fazer upload do recibo");
            return new UploadReceiptUseCaseOut
            {
                Sucesso = false,
                Mensagem = "Erro interno do servidor"
            };
        }
    }

    private static ReceiptDto MapToReceiptDto(ReceiptResponse response)
    {
        return new ReceiptDto
        {
            Id = response.ReceiptId,
            UsuarioId = response.UsuarioId,
            NomeArquivo = response.NomeArquivo,
            CaminhoArquivo = response.CaminhoArquivo,
            TipoMime = response.TipoMime,
            Descricao = response.Descricao,
            Valor = (decimal)response.Valor,
            Categoria = response.Categoria,
            DataUpload = DateTime.TryParse(response.DataUpload, out var dataUpload) ? dataUpload : DateTime.MinValue,
            DataAtualizacao = DateTime.TryParse(response.DataAtualizacao, out var dataAtualizacao) ? dataAtualizacao : null,
            Ativo = response.Ativo
        };
    }
}
