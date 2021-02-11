using Newtonsoft.Json;
using System;
using System.IO;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using Sandbox.Crypto;

using System.ComponentModel;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Generators;

namespace Blockchain
{
    internal class Crypto
    {
        /// <summary>
        /// Ta metoda podpiše array podanih stringov in vrne podpis v obliki string
        /// hkrati vrne out polje verified: podatek, če je transakcija verificirana
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        public static string Podpis(string[] args, out bool verified)
        {
            var random = new SecureRandom();
            int keySize = 2048;
            var keyGenerationParameters = new KeyGenerationParameters(random, keySize);
            RsaKeyPairGenerator generator = new RsaKeyPairGenerator();
            generator.Init(keyGenerationParameters);

            var keyPair = generator.GenerateKeyPair();
            var privateKeyParametersJson = JsonConvert.SerializeObject(keyPair.Private.ToPrivateKeyParameters());
            var publicKeyParametersJson = JsonConvert.SerializeObject(keyPair.Public.ToPublicKeyParameters());


            var rsa = RSA.Create();
            var rsaParameters = JsonConvert.DeserializeObject<RsaPublicKeyParameters>(publicKeyParametersJson).ToRSAParameters();
            rsa.ImportParameters(rsaParameters);

            string message = String.Join<string>(String.Empty, args);

            byte[] zapodpis = Encoding.UTF8.GetBytes(message);

            var encryptedData = rsa.Encrypt(zapodpis, RSAEncryptionPadding.OaepSHA256);




            //#region izvozključevFile

            //Guid g = Guid.NewGuid();
            //string file = string.Format("{0}_privatekey.txt",g);
            //using (TextWriter outputStream = File.CreateText(file))
            //{
            //    ExportPrivateKey(RSA, outputStream);
            //}

            //file = string.Format("{0}_publickey.txt", g);

            //string pubkey = ExportPublicKeyToPEMFormat(RSA);
            //using (TextWriter outputStreamPublicKey = File.CreateText(file))
            //{
            //    outputStreamPublicKey.WriteAsync(pubkey);
            //}

            //#endregion izvozključevFile

            #region izvozključevContainer

            // glej https://docs.microsoft.com/en-us/dotnet/api/system.security.cryptography.rsacryptoserviceprovider.-ctor?view=net-5.0#System_Security_Cryptography_RSACryptoServiceProvider__ctor_System_Security_Cryptography_CspParameters_
            string KeyContainerName = "MyKeyContainer";

            //Create a new key and persist it in
            //the key container.
            // ne shranim več v kontejner, kako se bere, briše, doda?
            //RSAPersistKeyInCSP(KeyContainerName);

            #endregion izvozključevContainer

            //  string message = String.Join<string>(String.Empty, args);

            ///  string signedMessage = SignData(message, RSAPrivateKeyInfo);

            // to je out polje
            //   verified = VerifyData(message, signedMessage, RSAPublicKeyInfo);
            verified = true;
            return Encoding.UTF8.GetString(encryptedData);
        }

        public static string SignData(string message, RSAParameters privateKey)
        {
            ASCIIEncoding byteConverter = new ASCIIEncoding();

            byte[] signedBytes;

            using (var rsa = new RSACryptoServiceProvider())
            {
                // Write the message to a byte array using ASCII as the encoding.
                byte[] originalData = byteConverter.GetBytes(message);

                try
                {
                    // Import the private key used for signing the message
                    rsa.ImportParameters(privateKey);

                    // Sign the data, using SHA256 as the hashing algorithm
                    signedBytes = rsa.SignData(originalData, CryptoConfig.MapNameToOID("SHA256"));
                }
                catch (CryptographicException e)
                {
                    Console.WriteLine(e.Message);
                    return null;
                }
                finally
                {
                    // Set the keycontainer to be cleared when rsa is garbage collected.
                    rsa.PersistKeyInCsp = false;
                }
            }
            // Convert the byte array back to a string message
            return Convert.ToBase64String(signedBytes);
        }

        public static bool VerifyData(string originalMessage, string signedMessage, RSAParameters publicKey)
        {
            bool success = false;
            using (var rsa = new RSACryptoServiceProvider())
            {
                ASCIIEncoding byteConverter = new ASCIIEncoding();

                byte[] bytesToVerify = byteConverter.GetBytes(originalMessage);
                byte[] signedBytes = Convert.FromBase64String(signedMessage);

                try
                {
                    rsa.ImportParameters(publicKey);

                    success = rsa.VerifyData(bytesToVerify, CryptoConfig.MapNameToOID("SHA256"), signedBytes);
                }
                catch (CryptographicException e)
                {
                    Console.WriteLine(e.Message);
                }
                finally
                {
                    rsa.PersistKeyInCsp = false;
                }
            }
            return success;
        }

        // iz https://stackoverflow.com/questions/23734792/c-sharp-export-private-public-rsa-key-from-rsacryptoserviceprovider-to-pem-strin
        /// <summary>
        ///
        /// </summary>
        /// <param name="csp"></param>
        /// <param name="outputStream"></param>
        private static void ExportPrivateKey(RSACryptoServiceProvider csp, TextWriter outputStream)
        {
            if (csp.PublicOnly) throw new ArgumentException("CSP does not contain a private key", "csp");
            var parameters = csp.ExportParameters(true);
            using (var stream = new MemoryStream())
            {
                var writer = new BinaryWriter(stream);
                writer.Write((byte)0x30); // SEQUENCE
                using (var innerStream = new MemoryStream())
                {
                    var innerWriter = new BinaryWriter(innerStream);
                    EncodeIntegerBigEndian(innerWriter, new byte[] { 0x00 }); // Version
                    EncodeIntegerBigEndian(innerWriter, parameters.Modulus);
                    EncodeIntegerBigEndian(innerWriter, parameters.Exponent);
                    EncodeIntegerBigEndian(innerWriter, parameters.D);
                    EncodeIntegerBigEndian(innerWriter, parameters.P);
                    EncodeIntegerBigEndian(innerWriter, parameters.Q);
                    EncodeIntegerBigEndian(innerWriter, parameters.DP);
                    EncodeIntegerBigEndian(innerWriter, parameters.DQ);
                    EncodeIntegerBigEndian(innerWriter, parameters.InverseQ);
                    var length = (int)innerStream.Length;
                    EncodeLength(writer, length);
                    writer.Write(innerStream.GetBuffer(), 0, length);
                }

                var base64 = Convert.ToBase64String(stream.GetBuffer(), 0, (int)stream.Length).ToCharArray();
                outputStream.WriteLine("-----BEGIN RSA PRIVATE KEY-----");
                // Output as Base64 with lines chopped at 64 characters
                for (var i = 0; i < base64.Length; i += 64)
                {
                    outputStream.WriteLine(base64, i, Math.Min(64, base64.Length - i));
                }
                outputStream.WriteLine("-----END RSA PRIVATE KEY-----");
            }
        }

        public static string ExportPublicKeyToPEMFormat(RSACryptoServiceProvider csp)
        {
            TextWriter outputStream = new StringWriter();

            var parameters = csp.ExportParameters(false);
            using (var stream = new MemoryStream())
            {
                var writer = new BinaryWriter(stream);
                writer.Write((byte)0x30); // SEQUENCE
                using (var innerStream = new MemoryStream())
                {
                    var innerWriter = new BinaryWriter(innerStream);
                    EncodeIntegerBigEndian(innerWriter, new byte[] { 0x00 }); // Version
                    EncodeIntegerBigEndian(innerWriter, parameters.Modulus);
                    EncodeIntegerBigEndian(innerWriter, parameters.Exponent);

                    //All Parameter Must Have Value so Set Other Parameter Value Whit Invalid Data  (for keeping Key Structure  use "parameters.Exponent" value for invalid data)
                    EncodeIntegerBigEndian(innerWriter, parameters.Exponent); // instead of parameters.D
                    EncodeIntegerBigEndian(innerWriter, parameters.Exponent); // instead of parameters.P
                    EncodeIntegerBigEndian(innerWriter, parameters.Exponent); // instead of parameters.Q
                    EncodeIntegerBigEndian(innerWriter, parameters.Exponent); // instead of parameters.DP
                    EncodeIntegerBigEndian(innerWriter, parameters.Exponent); // instead of parameters.DQ
                    EncodeIntegerBigEndian(innerWriter, parameters.Exponent); // instead of parameters.InverseQ

                    var length = (int)innerStream.Length;
                    EncodeLength(writer, length);
                    writer.Write(innerStream.GetBuffer(), 0, length);
                }

                var base64 = Convert.ToBase64String(stream.GetBuffer(), 0, (int)stream.Length).ToCharArray();
                outputStream.WriteLine("-----BEGIN PUBLIC KEY-----");
                // Output as Base64 with lines chopped at 64 characters
                for (var i = 0; i < base64.Length; i += 64)
                {
                    outputStream.WriteLine(base64, i, Math.Min(64, base64.Length - i));
                }
                outputStream.WriteLine("-----END PUBLIC KEY-----");

                return outputStream.ToString();
            }
        }

        private static void EncodeIntegerBigEndian(BinaryWriter stream, byte[] value, bool forceUnsigned = true)
        {
            stream.Write((byte)0x02); // INTEGER
            var prefixZeros = 0;
            for (var i = 0; i < value.Length; i++)
            {
                if (value[i] != 0) break;
                prefixZeros++;
            }
            if (value.Length - prefixZeros == 0)
            {
                EncodeLength(stream, 1);
                stream.Write((byte)0);
            }
            else
            {
                if (forceUnsigned && value[prefixZeros] > 0x7f)
                {
                    // Add a prefix zero to force unsigned if the MSB is 1
                    EncodeLength(stream, value.Length - prefixZeros + 1);
                    stream.Write((byte)0);
                }
                else
                {
                    EncodeLength(stream, value.Length - prefixZeros);
                }
                for (var i = prefixZeros; i < value.Length; i++)
                {
                    stream.Write(value[i]);
                }
            }
        }

        private static void EncodeLength(BinaryWriter stream, int length)
        {
            if (length < 0) throw new ArgumentOutOfRangeException("length", "Length must be non-negative");
            if (length < 0x80)
            {
                // Short form
                stream.Write((byte)length);
            }
            else
            {
                // Long form
                var temp = length;
                var bytesRequired = 0;
                while (temp > 0)
                {
                    temp >>= 8;
                    bytesRequired++;
                }
                stream.Write((byte)(bytesRequired | 0x80));
                for (var i = bytesRequired - 1; i >= 0; i--)
                {
                    stream.Write((byte)(length >> (8 * i) & 0xff));
                }
            }
        }

        public static void RSAPersistKeyInCSP(string ContainerName)
        {
            try
            {
                // Create a new instance of CspParameters.  Pass
                // 13 to specify a DSA container or 1 to specify
                // an RSA container.  The default is 1.
                CspParameters cspParams = new CspParameters
                {
                    // Specify the container name using the passed variable.
                    KeyContainerName = ContainerName
                };

                //Create a new instance of RSACryptoServiceProvider to generate
                //a new key pair.  Pass the CspParameters class to persist the
                //key in the container.
                RSACryptoServiceProvider RSAalg = new RSACryptoServiceProvider(cspParams);

                //Indicate that the key was persisted.
                Console.WriteLine("The RSA key was persisted in the container, \"{0}\".", ContainerName);
            }
            catch (CryptographicException e)
            {
                Console.WriteLine(e.Message);
            }
        }

        public static void RSADeleteKeyInCSP(string ContainerName)
        {
            try
            {
                // Create a new instance of CspParameters.  Pass
                // 13 to specify a DSA container or 1 to specify
                // an RSA container.  The default is 1.
                CspParameters cspParams = new CspParameters
                {
                    // Specify the container name using the passed variable.
                    KeyContainerName = ContainerName
                };

                //Create a new instance of RSACryptoServiceProvider.
                //Pass the CspParameters class to use the
                //key in the container.
                RSACryptoServiceProvider RSAalg = new RSACryptoServiceProvider(cspParams)
                {
                    //Delete the key entry in the container.
                    PersistKeyInCsp = false
                };

                //Call Clear to release resources and delete the key from the container.
                RSAalg.Clear();

                //Indicate that the key was persisted.
                Console.WriteLine("The RSA key was deleted from the container, \"{0}\".", ContainerName);
            }
            catch (CryptographicException e)
            {
                Console.WriteLine(e.Message);
            }
        }

    }
}