using System;
using System.Linq;
using System.Runtime.InteropServices;
using PCSC;
using RGiesecke.DllExport;

namespace ReaderRFIDDLL
{
    [ComVisible(true)]
    public class LerRFIDD
    {
        [DllExport]
        public string LerRFID()
        {
            string tagId = "";

            try
            {
                // Cria um contexto PC/SC
                using (var context = ContextFactory.Instance.Establish(SCardScope.System))
                {
                    // Lista todos os leitores disponíveis
                    var readerNames = context.GetReaders();
                    if (readerNames == null || readerNames.Length == 0)
                    {
                        return "Sem leitor";
                    }

                    // Escolhe o primeiro leitor disponível
                    var readerName = readerNames.First();

                    // Cria uma conexão com o leitor selecionado
                    using (var reader = new SCardReader(context))
                    {
                        // Conecta-se ao leitor
                        var result = reader.Connect(readerName, SCardShareMode.Shared, SCardProtocol.Any);
                        if (result != PCSC.SCardError.Success)
                        {
                            return $"Falha ao conectar ao leitor. Código de erro: {result.ToString()}";
                        }

                        // Realiza a transmissão APDU para obter o número de série
                        var command = new byte[] { 0xFF, 0xCA, 0x00, 0x00, 0x00 };
                        var response = new byte[256];
                        PCSC.SCardError transmitResult = reader.Transmit(command, ref response);
                        if (transmitResult != PCSC.SCardError.Success)
                        {
                            return $"Falha ao transmitir. Código de erro: {transmitResult}";
                        }

                        // Extrai o número de série da resposta
                        var responseLength = response.Length;
                        tagId = BitConverter.ToString(response.Take(responseLength - 2).ToArray()).Replace("-", "");

                        // Desconecta-se do leitor
                        reader.Disconnect(SCardReaderDisposition.Leave);
                    }
                }
            }
            catch (Exception ex)
            {
                return $"Erro: {ex.Message}";
            }

            return tagId;
        }
    }
}
