using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Teste_RFID
{
    public partial class Form1 : Form
    {
        int retCode;
        int hCard;
        int hContext;
        int Protocol;
        public bool connActive = false;
        string readername = "ACS ACR122 0";      // change depending on reader
        public byte[] SendBuff = new byte[263];
        public byte[] RecvBuff = new byte[263];
        public int SendLen, RecvLen, nBytesRet, reqType, Aprotocol, dwProtocol, cbPciLength;
        public Card.SCARD_READERSTATE RdrState;
        public Card.SCARD_IO_REQUEST pioSendRequest;       public Form1()
        {
            InitializeComponent();
            SelectDevice();
            establishContext();

        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (connectCard())
            {
                string cardUID = getcardUID();
                textBlock1.Text = cardUID; //displaying on text block
            }
        }

        public bool connectCard()
        {
            connActive = true;

            retCode = Card.SCardConnect(hContext
                                      , readername
                                      , Card.SCARD_SHARE_SHARED
                                      , Card.SCARD_PROTOCOL_T0 | Card.SCARD_PROTOCOL_T1
                                      , ref hCard
                                      , ref Protocol);

            if (retCode != Card.SCARD_S_SUCCESS)
            {
                MessageBox.Show(Card.GetScardErrMsg(retCode), "Cartão não disponível", MessageBoxButtons.OK, MessageBoxIcon.Error);
                connActive = false;
                return false;
            }
            return true;
        }

        private string getcardUID() //only for mifare 1k cards
        {
            string cardUID = "";
            byte[] receivedUID = new byte[256];
            Card.SCARD_IO_REQUEST request = new Card.SCARD_IO_REQUEST();
            request.dwProtocol = Card.SCARD_PROTOCOL_T1;
            request.cbPciLength = System.Runtime.InteropServices.Marshal.SizeOf(typeof(Card.SCARD_IO_REQUEST));
            byte[] sendBytes = new byte[] { 0xFF, 0xCA, 0x00, 0x00, 0x00 }; //get UID command      for Mifare cards
            int outBytes = receivedUID.Length;

            int status = Card.SCardTransmit(hCard
                                          , ref request
                                          , ref sendBytes[0]
                                          , sendBytes.Length
                                          , ref request
                                          , ref receivedUID[0]
                                          , ref outBytes);

            if (status != Card.SCARD_S_SUCCESS)
            {
                cardUID = "Error";
            }
            else
            {
                cardUID = BitConverter.ToString(receivedUID.Take(4).ToArray()).Replace("-", string.Empty).ToLower();
            }

            return cardUID;
        }


        public void SelectDevice()
        {
            List<string> availableReaders = this.ListReaders();
            this.RdrState = new Card.SCARD_READERSTATE();
            readername = availableReaders[0].ToString();//selecting first device
            this.RdrState.RdrName = readername;
        }

        public List<string> ListReaders()
        {
            int ReaderCount = 0;
            List<string> AvailableReaderList = new List<string>();

            //Make sure a context has been established before 
            //retrieving the list of smartcard readers.
            retCode = Card.SCardListReaders(hContext, null, null, ref ReaderCount);
            if (retCode != Card.SCARD_S_SUCCESS)
            {
                MessageBox.Show(Card.GetScardErrMsg(retCode));
                //connActive = false;
            }

            byte[] ReadersList = new byte[ReaderCount];

            //Get the list of reader present again but this time add sReaderGroup, retData as 2rd & 3rd parameter respectively.
            retCode = Card.SCardListReaders(hContext, null, ReadersList, ref ReaderCount);
            if (retCode != Card.SCARD_S_SUCCESS)
            {
                MessageBox.Show(Card.GetScardErrMsg(retCode));
            }

            string rName = "";
            int indx = 0;
            if (ReaderCount > 0)
            {
                // Convert reader buffer to string
                while (ReadersList[indx] != 0)
                {

                    while (ReadersList[indx] != 0)
                    {
                        rName = rName + (char)ReadersList[indx];
                        indx = indx + 1;
                    }

                    //Add reader name to list
                    AvailableReaderList.Add(rName);
                    rName = "";
                    indx = indx + 1;

                }
            }
            return AvailableReaderList;

        }

        internal void establishContext()
        {
            retCode = Card.SCardEstablishContext(Card.SCARD_SCOPE_SYSTEM, 0, 0, ref hContext);
            if (retCode != Card.SCARD_S_SUCCESS)
            {
                MessageBox.Show("Verifique o dispositivo e tente novamente", "Leitor não conectado", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                connActive = false;
                return;
            }
        }
    }
}
