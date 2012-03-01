namespace AmazonSecureDownloadTest.AWSHelpers
{
    using System;
    using System.Security.Cryptography;
    using System.Text;
    using SecureDownloadTest;

    public static class CloudFront
    {
        /// <summary>
        /// Gets a secure signed url for downloading through Amazon CloudFront
        /// </summary>
        /// <param name="resourceUrl">URL to the resource</param>
        /// <param name="expires">Expiry date for the download link</param>
        /// <param name="keyXml">Private key XML (Use OpenSSLKey to convert from pem format)</param>
        /// <param name="keypairId">Key Pair Id of the key from AWS Security Credentials page</param>
        /// <returns></returns>
        public static string GetSecureUrl(string resourceUrl, DateTime expires, string keyXml, string keypairId)
        {
            if (string.IsNullOrEmpty(resourceUrl))
            {
                throw new ArgumentNullException("resourceUrl");
            }

            if (string.IsNullOrEmpty(keyXml))
            {
                throw new ArgumentNullException("keyXml");
            }

            if (string.IsNullOrEmpty(keypairId))
            {
                throw new ArgumentNullException("keypairId");
            }

            if (expires < DateTime.Now)
            {
                throw new ArgumentException("Expiry date cannot be before current time.");
            }

            var unixExpiry = expires.ToUnixTime();
            var policy = BuildCannedPolicy(resourceUrl, unixExpiry);
            var rsa = GetRsaProvider(keyXml);
            var rsaSignature = rsa.SignData(Encoding.UTF8.GetBytes(policy), new SHA1CryptoServiceProvider());
            var signature = EncodeSignature(rsaSignature);

            return string.Format(
                            "{0}?Expires={1}&Signature={2}&Key-Pair-Id={3}", 
                            resourceUrl, 
                            unixExpiry, 
                            signature, 
                            keypairId);
        }

        private static RSACryptoServiceProvider GetRsaProvider(string keyXml)
        {
            var cspParams = new CspParameters();
            cspParams.Flags |= CspProviderFlags.UseMachineKeyStore;

            var rsa = new RSACryptoServiceProvider(cspParams) { PersistKeyInCsp = false };
            rsa.FromXmlString(keyXml);

            return rsa;
        }

        private static string EncodeSignature(byte[] data)
        {
            // From the Amazon docs - apparently making up your own encoding
            // is better than using one of the web standards...
            return Convert.ToBase64String(data).Replace('+', '-').Replace('=', '_').Replace('/', '~');
        }

        private static string BuildCannedPolicy(string resourceUrl, double unixExpiry)
        {
            return String.Format(
                        @"{{""Statement"":[{{""Resource"":""{0}"",""Condition"":{{""DateLessThan"":{{""AWS:EpochTime"":{1}}}}}}}]}}", 
                        resourceUrl, 
                        unixExpiry);
        }
    }
}