namespace AmazonSecureDownloadTest
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using AWSHelpers;

    internal class Program
    {
        private const string LogFile = @"urls.txt";

        private const string AccessKeyId = "";

        private const string SecretAccessKey = "";

        private const string KeyPairId = "";

        private const string PrivateKey = "";

        private static void Main()
        {
            const string FileName = "My.File.Zip";
            const string S3Bucket = "mybucket";
            const string CloudFrontBaseUrl = @"http://blahblah.cloudfront.net/";
            const int MinutesValid = 15;

            var expiry = DateTime.Now.AddMinutes(MinutesValid);

            var s3SecureUrl = S3.GetSecureUrl(S3Bucket, FileName, expiry, AccessKeyId, SecretAccessKey);
            var cloudFrontSecureUrl = CloudFront.GetSecureUrl(CloudFrontBaseUrl + FileName, expiry, PrivateKey, KeyPairId);

            using (StreamWriter output = File.CreateText(LogFile))
            {
                output.WriteLine("S3 URL: {0}", s3SecureUrl);
                output.WriteLine();
                output.WriteLine("CloudFront URL: {0}", cloudFrontSecureUrl);
                output.WriteLine();
                output.WriteLine("Expires: {0}", expiry);
                output.Close();
            }

            Process.Start(LogFile);
        }
    }
}