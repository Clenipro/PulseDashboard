using System;
using System.IO.Ports;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using System.Text;
using System.Diagnostics;

namespace ApiChamada.Services
{
    public class QRCodeScannerService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<QRCodeScannerService> _logger;
        private SerialPort _scannerPort;
        private StringBuilder _buffer = new StringBuilder();
        private Stopwatch _inputTimer = new Stopwatch();
        private bool _isScanning = false;
        private const int ScannerTimeoutMs = 1000; // Tempo máximo entre caracteres
        private const string DefaultPortName = "COM1"; // Porta padrão, ajuste conforme necessário
        private const int DefaultBaudRate = 9600; // Taxa de transmissão padrão

        public QRCodeScannerService(
            IServiceProvider serviceProvider,
            ILogger<QRCodeScannerService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Serviço de escuta de QR Code iniciado.");

            // Tenta configurar a porta serial
            if (!SetupSerialPort())
            {
                _logger.LogError("Não foi possível configurar a porta serial para o scanner.");
                return;
            }

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await Task.Delay(50, stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Erro no serviço de scanner de QR Code");
                }
            }
        }

        private bool SetupSerialPort()
        {
            try
            {
                // Tenta encontrar a porta do scanner
                string scannerPort = FindScannerPort();

                if (string.IsNullOrEmpty(scannerPort))
                {
                    _logger.LogWarning("Nenhuma porta de scanner encontrada.");
                    return false;
                }

                // Configura a porta serial
                _scannerPort = new SerialPort(scannerPort, DefaultBaudRate)
                {
                    // Configurações típicas para scanners
                    Parity = Parity.None,
                    DataBits = 8,
                    StopBits = StopBits.One,
                    Handshake = Handshake.None,
                    ReadTimeout = 500,
                    WriteTimeout = 500
                };

                // Configura o evento de recebimento de dados
                _scannerPort.DataReceived += OnDataReceived;

                // Abre a porta
                _scannerPort.Open();

                _logger.LogInformation($"Porta serial {scannerPort} configurada com sucesso.");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao configurar porta serial");
                return false;
            }
        }

        private string FindScannerPort()
        {
            // Lógica para encontrar a porta do scanner
            string[] ports = SerialPort.GetPortNames();

            foreach (string port in ports)
            {
                try
                {
                    using (SerialPort testPort = new SerialPort(port, DefaultBaudRate))
                    {
                        testPort.Open();

                        // Testa características específicas do scanner
                        // Você pode adicionar mais verificações específicas do seu scanner
                        if (IsValidScannerPort(testPort))
                        {
                            testPort.Close();
                            return port;
                        }

                        testPort.Close();
                    }
                }
                catch
                {
                    // Porta não disponível ou em uso
                    continue;
                }
            }

            // Tenta porta padrão se nenhuma for encontrada
            return ports.Length > 0 ? ports[0] : DefaultPortName;
        }

        private bool IsValidScannerPort(SerialPort port)
        {
            try
            {
                // Exemplo de verificação (ajuste conforme o scanner específico)
                port.ReadTimeout = 1000;
                string response = port.ReadExisting();
                return !string.IsNullOrWhiteSpace(response);
            }
            catch
            {
                return false;
            }
        }

        private void OnDataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            try
            {
                // Lê os dados da porta serial
                string input = _scannerPort.ReadExisting();

                if (string.IsNullOrEmpty(input))
                    return;

                // Processa cada caractere
                foreach (char c in input)
                {
                    ProcessInput(c.ToString());
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao receber dados do scanner");
            }
        }

        private void ProcessInput(string input)
        {
            // Reinicia o timer ao receber qualquer entrada
            _inputTimer.Restart();
            _isScanning = true;

            // Adiciona ao buffer
            _buffer.Append(input);

            // Verifica e processa o buffer
            CheckAndProcessBuffer();
        }

        private void CheckAndProcessBuffer()
        {
            // Verifica se passou tempo suficiente sem novas entradas
            if (_isScanning && _inputTimer.IsRunning && _inputTimer.ElapsedMilliseconds > ScannerTimeoutMs)
            {
                // Para de contar o tempo
                _inputTimer.Stop();

                // Obtém o buffer completo
                string qrCodeData = _buffer.ToString().Trim();

                // Limpa o buffer
                _buffer.Clear();

                // Reseta o modo de scanning
                _isScanning = false;

                // Processa a entrada
                if (!string.IsNullOrEmpty(qrCodeData))
                {
                    ProcessQRCodeAsync(qrCodeData);
                }
            }
        }

        private void ProcessQRCodeAsync(string qrCodeData)
        {
            // Usa Task.Run para processar de forma assíncrona
            Task.Run(async () =>
            {
                _logger.LogInformation($"Processando entrada de QR Code: {qrCodeData}");

                // Cria um escopo para resolver dependências
                using (var scope = _serviceProvider.CreateScope())
                {
                    try
                    {
                        // Resolve os serviços dentro do escopo
                        var qrCodeService = scope.ServiceProvider.GetRequiredService<QRCodeService>();
                        var ticketDao = scope.ServiceProvider.GetRequiredService<Data.TicketDao>();

                        // Verifica se é um QR code válido
                        bool ehQRCode = qrCodeService.VerificarOrigemQRCode(qrCodeData);
                        if (!ehQRCode)
                        {
                            _logger.LogWarning($"Input não reconhecido como QR Code: {qrCodeData}");
                            return;
                        }

                        var ticket = await ticketDao.ProcessarTicketPorQRCode(qrCodeData);

                        if (ticket == null)
                        {
                            _logger.LogWarning($"Ticket não encontrado para QR Code: {qrCodeData}");
                            return;
                        }

                        _logger.LogInformation($"Ticket processado com sucesso: {ticket.Id}");
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, $"Erro ao processar entrada: {qrCodeData}");
                    }
                }
            });
        }

        public override Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Serviço de scanner de QR Code está sendo interrompido.");

            // Fecha a porta serial
            if (_scannerPort != null && _scannerPort.IsOpen)
            {
                _scannerPort.Close();
                _scannerPort.Dispose();
            }

            return base.StopAsync(cancellationToken);
        }
    }
}