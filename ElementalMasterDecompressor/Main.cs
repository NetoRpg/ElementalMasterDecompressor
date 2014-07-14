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

                if (ofd.ShowDialog() == DialogResult.OK)
                {

                    using (MemoryStream ms = new MemoryStream(File.ReadAllBytes(ofd.FileName)))
                    {
                        int blockPosition = 0x7C000;
                        ms.Position = blockPosition + 1;

                        int nFiles = ms.ReadByte();
                        int textBlockPointer = ms.extractPiece(0, 4).GetInt32();
                        int textBlockDecSize = ms.extractPiece(0, 4).GetInt32();
                        int textBlockComSize = ms.extractPiece(0, 4).GetInt32();

                        ms.Position = blockPosition + textBlockPointer;

                        string text = String.Empty;

                        for (int i = 0; i < textBlockComSize; i++)
                        {
                            int currentByte = ms.ReadByte();

                            for (int j = 0; j < 8; j++)
                            {
                                if (text.Length >= textBlockDecSize) break;

                                if (((currentByte >> j) & 1) == 1)
                                {
                                    text += Convert.ToChar(ms.ReadByte());
                                }
                                else
                                {
                                    int basePosition = ms.ReadByte();
                                    int size = ms.ReadByte();
                                    int multiplier = (size >> 4) & 0x0F;
                                    size &= 0x0F;

                                    for (int k = 0; k < size + 3; k++)
                                    {
                                        int absolutePosition = 0x12 + basePosition + k + (0x100 * multiplier);

                                        if (text.Length >= 0x1024 && absolutePosition + 0x1000 < text.Length)
                                            absolutePosition += 0x1000;

                                        if (absolutePosition < text.Length)
                                            text += text.Substring(absolutePosition, 1);
                                        else
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

                            if (sfd.ShowDialog() == DialogResult.OK)
                            {
                                using (StreamWriter sw = new StreamWriter(sfd.FileName))
                                {
                                    sw.Write(text);
                                }

                                MessageBox.Show("Successfully Extracted");
                            }
                        }
                    }
                }
            }
        }
    }
}