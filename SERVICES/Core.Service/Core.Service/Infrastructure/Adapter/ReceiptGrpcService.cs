using Core.Service.Application.Domain.Models;
using Core.Service.Infrastructure.Data.Repositories;
using Core.Service.Application.Services;
using Grpc.Core;
using ZapFinance.ProtoServer.Core;

namespace Core.Service.Infrastructure.Adapter;

public class ReceiptGrpcService : ZapFinance.ProtoServer.Core.ReceiptService.ReceiptServiceBase
{
    private readonly IReceiptRepository _receiptRepository;
    private readonly IUsuarioRepository _usuarioRepository;
    private readonly IGoogleGeminiService _googleGeminiService;
    private readonly ILogger<ReceiptGrpcService> _logger;

    public ReceiptGrpcService(
        IReceiptRepository receiptRepository,
        IUsuarioRepository usuarioRepository,
        IGoogleGeminiService googleGeminiService,
        ILogger<ReceiptGrpcService> logger)
    {
        _receiptRepository = receiptRepository;
        _usuarioRepository = usuarioRepository;
        _googleGeminiService = googleGeminiService;
        _logger = logger;
    }

    public override async Task<ReceiptResponse> UploadReceipt(UploadReceiptRequest request, ServerCallContext context)
    {
        try
        {

            // Validar arquivo
            if (request.ImagemData.IsEmpty)
            {
                throw new RpcException(new Status(StatusCode.InvalidArgument, "Dados da imagem são obrigatórios"));
            }

            // Criar diretório se não existir
            var uploadsPath = Path.Combine(Directory.GetCurrentDirectory(), "uploads", "receipts");
            Directory.CreateDirectory(uploadsPath);

            // Gerar nome único para o arquivo
            var fileExtension = GetFileExtension(request.TipoMime);
            var fileName = $"{Guid.NewGuid()}{fileExtension}";
            var filePath = Path.Combine(uploadsPath, fileName);

            // Converter para base64 e processar com Google Gemini
            var imageBytes = request.ImagemData.ToByteArray();
            var base64String = Convert.ToBase64String(imageBytes);
            
            // Analisar recibo com Google Gemini
            var geminiResult = await _googleGeminiService.AnalyzeReceiptImageAsync(base64String);
            
            // Salvar arquivo
            await File.WriteAllBytesAsync(filePath, imageBytes);

            // Criar entidade Receipt com dados do Google Gemini
            var receipt = new Receipt
            {
                Id = Guid.NewGuid(),
                UsuarioId = Guid.NewGuid(),
                NomeArquivo = request.NomeArquivo,
                CaminhoArquivo = filePath,
                TipoMime = request.TipoMime,
                Descricao = geminiResult.IsSuccessful ? geminiResult.MerchantName ?? "Recibo" : "Recibo",
                Valor = geminiResult.IsSuccessful ? geminiResult.ExtractedAmount ?? 0 : 0,
                Categoria = !string.IsNullOrEmpty(request.Categoria) ? request.Categoria : 
                           (geminiResult.IsSuccessful && !string.IsNullOrEmpty(geminiResult.Category) ? geminiResult.Category : "Geral")
            };

            // Validar se é uma imagem válida
            if (!receipt.IsImagemValida())
            {
                File.Delete(filePath); // Limpar arquivo se inválido
                throw new RpcException(new Status(StatusCode.InvalidArgument, "Tipo de arquivo não suportado"));
            }

            // Salvar no banco
            var savedReceipt = await _receiptRepository.CriarAsync(receipt);

            return MapToReceiptResponse(savedReceipt);
        }
        catch (RpcException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao fazer upload do recibo");
            throw new RpcException(new Status(StatusCode.Internal, "Erro interno do servidor"));
        }
    }

    public override async Task<ReceiptResponse> GetReceipt(GetReceiptRequest request, ServerCallContext context)
    {
        try
        {
            if (!Guid.TryParse(request.ReceiptId, out var receiptId))
            {
                throw new RpcException(new Status(StatusCode.InvalidArgument, "ID do recibo inválido"));
            }

            var receipt = await _receiptRepository.ObterPorIdAsync(receiptId);
            if (receipt == null)
            {
                throw new RpcException(new Status(StatusCode.NotFound, "Recibo não encontrado"));
            }

            return MapToReceiptResponse(receipt);
        }
        catch (RpcException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao obter recibo");
            throw new RpcException(new Status(StatusCode.Internal, "Erro interno do servidor"));
        }
    }

    public override async Task<ListReceiptsResponse> ListReceipts(ListReceiptsRequest request, ServerCallContext context)
    {
        try
        {
            if (!Guid.TryParse(request.UsuarioId, out var usuarioId))
            {
                throw new RpcException(new Status(StatusCode.InvalidArgument, "ID do usuário inválido"));
            }

            var receipts = await _receiptRepository.ListarPorUsuarioAsync(
                usuarioId, 
                request.Pagina, 
                request.TamanhoPagina, 
                request.Filtro, 
                request.Categoria);

            var total = await _receiptRepository.ContarPorUsuarioAsync(
                usuarioId, 
                request.Filtro, 
                request.Categoria);

            var response = new ListReceiptsResponse
            {
                Total = total,
                Pagina = request.Pagina,
                TotalPaginas = (int)Math.Ceiling((double)total / request.TamanhoPagina)
            };

            response.Receipts.AddRange(receipts.Select(MapToReceiptResponse));

            return response;
        }
        catch (RpcException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao listar recibos");
            throw new RpcException(new Status(StatusCode.Internal, "Erro interno do servidor"));
        }
    }

    public override async Task<ReceiptResponse> UpdateReceipt(UpdateReceiptRequest request, ServerCallContext context)
    {
        try
        {
            if (!Guid.TryParse(request.ReceiptId, out var receiptId))
            {
                throw new RpcException(new Status(StatusCode.InvalidArgument, "ID do recibo inválido"));
            }

            var receipt = await _receiptRepository.ObterPorIdAsync(receiptId);
            if (receipt == null)
            {
                throw new RpcException(new Status(StatusCode.NotFound, "Recibo não encontrado"));
            }

            receipt.Atualizar(request.Descricao, (decimal)request.Valor, request.Categoria);
            var updatedReceipt = await _receiptRepository.AtualizarAsync(receipt);

            return MapToReceiptResponse(updatedReceipt);
        }
        catch (RpcException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao atualizar recibo");
            throw new RpcException(new Status(StatusCode.Internal, "Erro interno do servidor"));
        }
    }

    public override async Task<DeleteReceiptResponse> DeleteReceipt(DeleteReceiptRequest request, ServerCallContext context)
    {
        try
        {
            if (!Guid.TryParse(request.ReceiptId, out var receiptId))
            {
                throw new RpcException(new Status(StatusCode.InvalidArgument, "ID do recibo inválido"));
            }

            var success = await _receiptRepository.DeletarAsync(receiptId);

            return new DeleteReceiptResponse
            {
                Sucesso = success,
                Mensagem = success ? "Recibo deletado com sucesso" : "Recibo não encontrado"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao deletar recibo");
            throw new RpcException(new Status(StatusCode.Internal, "Erro interno do servidor"));
        }
    }

    private static ReceiptResponse MapToReceiptResponse(Receipt receipt)
    {
        return new ReceiptResponse
        {
            ReceiptId = receipt.Id.ToString(),
            UsuarioId = receipt.UsuarioId.ToString(),
            NomeArquivo = receipt.NomeArquivo,
            CaminhoArquivo = receipt.CaminhoArquivo,
            TipoMime = receipt.TipoMime,
            Descricao = receipt.Descricao ?? string.Empty,
            Valor = (double)(receipt.Valor ?? 0),
            Categoria = receipt.Categoria ?? string.Empty,
            DataUpload = receipt.DataUpload.ToString("yyyy-MM-ddTHH:mm:ssZ"),
            DataAtualizacao = receipt.DataAtualizacao?.ToString("yyyy-MM-ddTHH:mm:ssZ") ?? string.Empty,
            Ativo = receipt.Ativo
        };
    }

    private static string GetFileExtension(string mimeType)
    {
        return mimeType.ToLowerInvariant() switch
        {
            "image/jpeg" => ".jpg",
            "image/jpg" => ".jpg",
            "image/png" => ".png",
            "image/gif" => ".gif",
            "image/webp" => ".webp",
            _ => ".jpg"
        };
    }
}
