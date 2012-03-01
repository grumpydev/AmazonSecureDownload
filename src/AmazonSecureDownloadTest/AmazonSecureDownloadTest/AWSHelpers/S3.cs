namespace AmazonSecureDownloadTest.AWSHelpers
{
    using System;
    using System.Globalization;
    using System.Security.Cryptography;
    using System.Text;
    using SecureDownloadTest;

    public class S3
    {
        /// <summary>
        /// Gets a secure signed url for downloading from an S3 bucket
        /// </summary>
        /// <param name="bucketName">Name of the S3 bucket</param>
        /// <param name="fileKey">Filename / key</param>
        /// <param name="expires">Expiry date for the link</param>
        /// <param name="accessKeyId">AWS Access Key Id</param>
        /// <param name="secretAccessKey">AWS Secret Access key</param>
        /// <returns></returns>
        public static string GetSecureUrl(string bucketName, string fileKey, DateTime expires, string accessKeyId,
                                          string secretAccessKey)
        {
            if (string.IsNullOrEmpty(bucketName))
            {
                throw new ArgumentNullException("bucketName");
            }

            if (string.IsNullOrEmpty(fileKey))
            {
                throw new ArgumentNullException("fileKey");
            }

            if (string.IsNullOrEmpty(accessKeyId))
            {
                throw new ArgumentNullException("accessKeyId");
            }

            if (string.IsNullOrEmpty(secretAccessKey))
            {
                throw new ArgumentNullException("secretAccessKey");
            }

            if (expires < DateTime.Now)
            {
                throw new ArgumentException("Expiry date cannot be before current time.");
            }

            double unixExpiry = expires.ToUnixTime();
            string stringToSign = BuildStringToSign(bucketName, fileKey, unixExpiry);
            var hmacGenerator = new HMACSHA1(Encoding.UTF8.GetBytes(secretAccessKey));
            byte[] hash = hmacGenerator.ComputeHash(Encoding.UTF8.GetBytes(stringToSign));
            string signature = EncodeSignature(hash);

            return string.Format(
                "http://{0}.s3.amazonaws.com/{1}?AWSAccessKeyId={2}&Expires={3}&Signature={4}",
                bucketName,
                fileKey,
                accessKeyId,
                unixExpiry,
                signature);
        }

        private static string EncodeSignature(byte[] hash)
        {
            return Uri.EscapeDataString(Convert.ToBase64String(hash));
        }

        private static string BuildStringToSign(string bucketName, string fileKey, double unixExpiry)
        {
            return string.Format("GET\n\n\n{0}\n/{1}/{2}", unixExpiry.ToString(CultureInfo.InvariantCulture), bucketName,
                                 fileKey);
        }
    }
}