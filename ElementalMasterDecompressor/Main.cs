using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;

namespace ElementalMasterDecompressor
{
    public partial class Main : Form
    {
        public Main()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                ofd.Filter = "Elemental Master ROM|*.bin";

                // Abre a janela para escolher o arquivo.
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    // Carrega a ROM na memória.
                    using (MemoryStream ms = new MemoryStream(File.ReadAllBytes(ofd.FileName)))
                    {
                        // Offset onde começa o header da compressão.
                        int blockPosition = 0x7C000; 

                        // Movendo o ponteiro para o offset do header + 1, para pular o primeiro byte.
                        ms.Position = blockPosition + 1; 
                        
                        // Lê o número de arquivos.
                        int nFiles = ms.ReadByte();

                        // Extrai 4 bytes e converte para um int.
                        int textBlockPointer = ms.extractPiece(0, 4).GetInt32(); // Offset do bloco de texto
                        int textBlockDecSize = ms.extractPiece(0, 4).GetInt32(); // Tamanho descomprimido
                        int textBlockComSize = ms.extractPiece(0, 4).GetInt32(); // Tamanho comprimido

                        // Como só tenho interesse no bloco de texto, pulo direto para o offset dele
                        // ignorando as informações sobre os outros arquivos.
                        ms.Position = blockPosition + textBlockPointer;

                        // Inicio a string que vai guardar o texto.
                        string text = String.Empty;

                        // Loop no bloco de texto comprimido.
                        for (int i = 0; i < textBlockComSize; i++)
                        {
                            // Lê um byte
                            int currentByte = ms.ReadByte();

                            // Loop nos próximos 8 bytes.
                            for (int j = 0; j < 8; j++)
                            {
                                // Se o tamanho da string de texto for maior ou igual o tamanho do arquivo descomprimido
                                // o loop é interrompido.
                                if (text.Length >= textBlockDecSize) break;
                                
                                // Faz o bitshift no byte que foi lido anteriormente e compara se ele é igual a 1
                                if (((currentByte >> j) & 1) == 1)
                                {
                                    // Se for, lê um byte, converte para char e copia para a string.
                                    text += Convert.ToChar(ms.ReadByte());
                                }
                                else // Se não:
                                {
                                    // Lê o primeiro byte do par.
                                    int basePosition = ms.ReadByte();

                                    // Lê o segundo byte do par.
                                    int size = ms.ReadByte();

                                    // Pega o primeiro nibble do segundo byte.
                                    int multiplier = (size >> 4) & 0x0F;

                                    // Aplica a máscara no segundo byte para pegar apenas o segundo nibble.
                                    size &= 0x0F;

                                    // Faz um loop da quantidade de bytes que serão copiados.
                                    for (int k = 0; k < size + 3; k++)
                                    {
                                        // Calcula a posição
                                        int absolutePosition = 0x12 + basePosition + k + (0x100 * multiplier);

                                        // Se o tamanho da string for maior ou igual a 0x1000 e o offset for menor que o tamanho da string
                                        // soma 0x1000 ao offset.
                                        if (text.Length >= 0x1000 && absolutePosition + 0x1000 < text.Length)
                                            absolutePosition += 0x1000;

                                        // Verifica se o offset é menor que o tamanho da string.
                                        if (absolutePosition < text.Length)
                                        // Se sim, copia um caractere daquela posição da string.
                                            text += text.Substring(absolutePosition, 1);
                                        else
                                        // Se não, adiciona um espaço na string.
                                        // * Nota que isso só é usado no inicio, onde ele tenta descomprimir 0x24 espaços inexistentes.
                                            text += " ";
                                    }
                                    i++;
                                }
                                i++;
                            }
                        }

                        
                        using (SaveFileDialog sfd = new SaveFileDialog())
                        {

                            sfd.Filter = "Text File|*.txt";

                            // Abre a janela de Salvar Arquivo.
                            if (sfd.ShowDialog() == DialogResult.OK)
                            {
                                // Salva o texto no local especificado.
                                using (StreamWriter sw = new StreamWriter(sfd.FileName))
                                {
                                    sw.Write(text);
                                }
                                // Exibe mensagem de sucesso.
                                MessageBox.Show("Successfully Extracted");
                            }
                        }
                    }
                }
            }
        }
    }
}