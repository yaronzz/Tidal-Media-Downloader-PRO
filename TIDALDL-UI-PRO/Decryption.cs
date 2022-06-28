using AIGS.Helper;
using AIGS.Common;
using TidalLib;

namespace TIDALDL_UI.Download
{
    public class Decryption
    {
        static byte[] MASTER_KEY = System.Convert.FromBase64String("UIlTTEMmmLfGowo/UC60x2H45W6MdGgTRfo/umg4754=");

        private static byte[] ReadFile(string filepath)
        {
            FileStream fs = new FileStream(filepath, FileMode.Open);
            byte[] array = new byte[fs.Length];
            fs.Read(array, 0, array.Length);
            fs.Close();
            return array;
        }

        private static bool WriteFile(string filepath, byte[] txt)
        {
            try
            {
                FileStream fs = new FileStream(filepath, FileMode.Create);
                fs.Write(txt, 0, txt.Length);
                fs.Close();
                return true;
            }
            catch { return false; }
        }

        public static bool DecryptTrackFile(StreamUrl stream, string filepath)
        {
            try
            {
                if (!System.IO.File.Exists(filepath))
                    return false;
                if (stream.EncryptionKey.IsBlank())
                    return true;

                byte[] security_token = System.Convert.FromBase64String(stream.EncryptionKey);

                byte[] iv = security_token.Skip(0).Take(16).ToArray();
                byte[] str = security_token.Skip(16).ToArray();
                byte[] dec = AESHelper.Decrypt(str, MASTER_KEY, iv);

                byte[] key = dec.Skip(0).Take(16).ToArray();
                byte[] nonce = dec.Skip(16).Take(8).ToArray();
                byte[] nonce2 = new byte[16];
                nonce.CopyTo(nonce2, 0);

                byte[] txt = ReadFile(filepath);
                AES_CTR tool = new AES_CTR(key, nonce2);
                byte[] newt = tool.DecryptBytes(txt);
                bool bfalg = WriteFile(filepath, newt);
                return bfalg;
            }
            catch
            {
                return false;
            }
        }

    }
}