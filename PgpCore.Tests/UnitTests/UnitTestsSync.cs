using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using Org.BouncyCastle.Bcpg;
using Org.BouncyCastle.Bcpg.OpenPgp;
using Xunit;

namespace PgpCore.Tests
{
    public class UnitTestsSync
    {
        [Fact]
        public void GenerateKey_CreatePublicPrivateKeyFiles()
        {
            // Arrange
            Directory.CreateDirectory(keyDirectory);
            PGP pgp = new PGP();

            // Act
            pgp.GenerateKey(publicKeyFilePath1, privateKeyFilePath1, password1);

            // Assert
            Assert.True(File.Exists(publicKeyFilePath1));
            Assert.True(File.Exists(privateKeyFilePath1));
        }

        #region File
        [Theory]
        [MemberData(nameof(KeyTypeValues))]
        public void EncryptFile_CreateEncryptedFile(KeyType keyType)
        {
            // Arrange
            Arrange(keyType);
            PGP pgp = new PGP();

            // Act
            pgp.EncryptFile(contentFilePath, encryptedContentFilePath, publicKeyFilePath1);

            // Assert
            Assert.True(File.Exists(encryptedContentFilePath));

            // Teardown
            Teardown();
        }

        [Theory]
        [MemberData(nameof(HashAlgorithmTagValues))]
        public void EncryptFile_CreateEncryptedFileWithDifferentHashAlgorithms(HashAlgorithmTag hashAlgorithmTag)
        {
            // Arrange
            Arrange(KeyType.Known);
            PGP pgp = new PGP();
            pgp.HashAlgorithmTag = hashAlgorithmTag;

            // Act
            pgp.EncryptFile(contentFilePath, encryptedContentFilePath, publicKeyFilePath1);

            // Assert
            Assert.True(File.Exists(encryptedContentFilePath));

            // Teardown
            Teardown();
        }

        [Theory]
        [InlineData(KeyType.Generated)]
        [InlineData(KeyType.Known)]
        [InlineData(KeyType.KnownGpg)]
        public void SignFile_CreateSignedFile(KeyType keyType)
        {
            // Arrange
            Arrange(keyType);
            PGP pgp = new PGP();

            // Act
            pgp.SignFile(contentFilePath, signedContentFilePath, privateKeyFilePath1, password1);

            // Assert
            Assert.True(File.Exists(signedContentFilePath));

            // Teardown
            Teardown();
        }

        [Theory]
        [InlineData(KeyType.Generated)]
        [InlineData(KeyType.Known)]
        [InlineData(KeyType.KnownGpg)]
        public void EncryptFile_CreateEncryptedFileWithMultipleKeys(KeyType keyType)
        {
            // Arrange
            Arrange(keyType);
            PGP pgp = new PGP();
            List<string> keys = new List<string>()
            {
                publicKeyFilePath1,
                publicKeyFilePath2
            };

            // Act
            pgp.EncryptFile(contentFilePath, encryptedContentFilePath, keys);

            // Assert
            Assert.True(File.Exists(encryptedContentFilePath));

            // Teardown
            Teardown();
        }

        [Theory]
        [InlineData(KeyType.Generated)]
        [InlineData(KeyType.Known)]
        [InlineData(KeyType.KnownGpg)]
        public void EncryptFileAndSign_CreateEncryptedAndSignedFile(KeyType keyType)
        {
            // Arrange
            Arrange(keyType);
            PGP pgp = new PGP();

            // Act
            pgp.EncryptFileAndSign(contentFilePath, encryptedContentFilePath, publicKeyFilePath1, privateKeyFilePath1, password1);

            // Assert
            Assert.True(File.Exists(encryptedContentFilePath));

            // Teardown
            Teardown();
        }

        [Theory]
        [InlineData(KeyType.Generated)]
        [InlineData(KeyType.Known)]
        [InlineData(KeyType.KnownGpg)]
        public void EncryptFileAndSign_CreateEncryptedAndSignedFileWithMultipleKeys(KeyType keyType)
        {
            // Arrange
            Arrange(keyType);
            PGP pgp = new PGP();
            List<string> keys = new List<string>()
            {
                publicKeyFilePath1,
                publicKeyFilePath2
            };

            // Act
            pgp.EncryptFileAndSign(contentFilePath, encryptedContentFilePath, keys, privateKeyFilePath1, password1);

            // Assert
            Assert.True(File.Exists(encryptedContentFilePath));

            // Teardown
            Teardown();
        }

        [Theory]
        [InlineData(KeyType.Generated)]
        [InlineData(KeyType.Known)]
        [InlineData(KeyType.KnownGpg)]
        public void DecryptFile_DecryptEncryptedFile(KeyType keyType)
        {
            // Arrange
            Arrange(keyType);
            PGP pgp = new PGP();

            // Act
            pgp.EncryptFile(contentFilePath, encryptedContentFilePath, publicKeyFilePath1);
            pgp.DecryptFile(encryptedContentFilePath, decryptedContentFilePath1, privateKeyFilePath1, password1);
            string decryptedContent = File.ReadAllText(decryptedContentFilePath1);

            // Assert
            Assert.True(File.Exists(encryptedContentFilePath));
            Assert.True(File.Exists(decryptedContentFilePath1));
            Assert.Equal(content, decryptedContent.Trim());

            // Teardown
            Teardown();
        }

        [Theory]
        [MemberData(nameof(HashAlgorithmTagValues))]
        public void DecryptFile_DecryptEncryptedFileWithDifferentHashAlgorithms(HashAlgorithmTag hashAlgorithmTag)
        {
            // Arrange
            Arrange(KeyType.Known);
            PGP pgp = new PGP();
            pgp.HashAlgorithmTag = hashAlgorithmTag;

            // Act
            pgp.EncryptFile(contentFilePath, encryptedContentFilePath, publicKeyFilePath1);
            pgp.DecryptFile(encryptedContentFilePath, decryptedContentFilePath1, privateKeyFilePath1, password1);
            string decryptedContent = File.ReadAllText(decryptedContentFilePath1);

            // Assert
            Assert.True(File.Exists(encryptedContentFilePath));
            Assert.True(File.Exists(decryptedContentFilePath1));
            Assert.Equal(content, decryptedContent.Trim());

            // Teardown
            Teardown();
        }

        //[Theory]
        //[InlineData(KeyType.Generated, FileType.GeneratedLarge)]
        //public void DecryptLargeFile_DecryptEncryptedFile(KeyType keyType, FileType fileType)
        //{
        //    // Arrange
        //    Arrange(keyType, fileType);
        //    PGP pgp = new PGP();

        //    // Act
        //    pgp.EncryptFile(contentFilePath, encryptedContentFilePath, publicKeyFilePath1);
        //    pgp.DecryptFile(encryptedContentFilePath, decryptedContentFilePath1, privateKeyFilePath1, password1);

        //    // Assert
        //    Assert.True(File.Exists(encryptedContentFilePath));
        //    Assert.True(File.Exists(decryptedContentFilePath1));

        //    // Teardown
        //    Teardown();
        //}

        [Theory]
        [InlineData(KeyType.Generated)]
        [InlineData(KeyType.Known)]
        [InlineData(KeyType.KnownGpg)]
        public void DecryptFile_DecryptEncryptedFileWithMultipleKeys(KeyType keyType)
        {
            // Arrange
            Arrange(keyType);
            PGP pgp = new PGP();
            List<string> keys = new List<string>()
            {
                publicKeyFilePath1,
                publicKeyFilePath2
            };

            // Act
            pgp.EncryptFile(contentFilePath, encryptedContentFilePath, keys);
            pgp.DecryptFile(encryptedContentFilePath, decryptedContentFilePath1, privateKeyFilePath1, password1);
            pgp.DecryptFile(encryptedContentFilePath, decryptedContentFilePath2, privateKeyFilePath2, password2);
            string decryptedContent1 = File.ReadAllText(decryptedContentFilePath1);
            string decryptedContent2 = File.ReadAllText(decryptedContentFilePath2);

            // Assert
            Assert.True(File.Exists(encryptedContentFilePath));
            Assert.True(File.Exists(decryptedContentFilePath1));
            Assert.True(File.Exists(decryptedContentFilePath2));
            Assert.Equal(content, decryptedContent1.Trim());
            Assert.Equal(content, decryptedContent2.Trim());

            // Teardown
            Teardown();
        }

        [Theory]
        [InlineData(KeyType.Generated)]
        [InlineData(KeyType.Known)]
        [InlineData(KeyType.KnownGpg)]
        public void DecryptFile_DecryptSignedAndEncryptedFile(KeyType keyType)
        {
            // Arrange
            Arrange(keyType);
            PGP pgp = new PGP();

            // Act
            pgp.EncryptFileAndSign(contentFilePath, encryptedContentFilePath, publicKeyFilePath1, privateKeyFilePath1, password1);
            pgp.DecryptFile(encryptedContentFilePath, decryptedContentFilePath1, privateKeyFilePath1, password1);
            string decryptedContent = File.ReadAllText(decryptedContentFilePath1);

            // Assert
            Assert.True(File.Exists(encryptedContentFilePath));
            Assert.True(File.Exists(decryptedContentFilePath1));
            Assert.Equal(content, decryptedContent.Trim());

            // Teardown
            Teardown();
        }

        [Theory]
        [InlineData(KeyType.Generated)]
        [InlineData(KeyType.Known)]
        [InlineData(KeyType.KnownGpg)]
        public void DecryptFile_DecryptSignedAndEncryptedFileWithMultipleKeys(KeyType keyType)
        {
            // Arrange
            Arrange(keyType);
            PGP pgp = new PGP();
            List<string> keys = new List<string>()
            {
                publicKeyFilePath1,
                publicKeyFilePath2
            };

            // Act
            pgp.EncryptFileAndSign(contentFilePath, encryptedContentFilePath, keys, privateKeyFilePath1, password1);
            pgp.DecryptFile(encryptedContentFilePath, decryptedContentFilePath1, privateKeyFilePath1, password1);
            pgp.DecryptFile(encryptedContentFilePath, decryptedContentFilePath2, privateKeyFilePath2, password2);
            string decryptedContent1 = File.ReadAllText(decryptedContentFilePath1);
            string decryptedContent2 = File.ReadAllText(decryptedContentFilePath2);

            // Assert
            Assert.True(File.Exists(encryptedContentFilePath));
            Assert.True(File.Exists(decryptedContentFilePath1));
            Assert.True(File.Exists(decryptedContentFilePath2));
            Assert.Equal(content, decryptedContent1.Trim());
            Assert.Equal(content, decryptedContent2.Trim());

            // Teardown
            Teardown();
        }
        
        [Theory]
        [InlineData(KeyType.Generated)]
        [InlineData(KeyType.Known)]
        [InlineData(KeyType.KnownGpg)]
        public void DecryptFileAndVerify_DecryptUnsignedFile(KeyType keyType)
        {
            // Arrange
            Arrange(keyType);
            PGP pgp = new PGP();

            // Act
            pgp.EncryptFile(contentFilePath, encryptedContentFilePath, publicKeyFilePath2);
            var ex = Assert.Throws<PgpException>(() => pgp.DecryptFileAndVerify(encryptedContentFilePath,
                decryptedContentFilePath1, publicKeyFilePath1, privateKeyFilePath2, password2));
           
            string decryptedContent = File.ReadAllText(decryptedContentFilePath1);

            // Assert
            Assert.Equal("File was not signed.", ex.Message);
            Assert.True(File.Exists(encryptedContentFilePath));
            Assert.True(File.Exists(decryptedContentFilePath1));
            Assert.Equal(string.Empty, decryptedContent.Trim());

            // Teardown
            Teardown();
        }

        
        [Theory]
        [InlineData(KeyType.Generated)]
        [InlineData(KeyType.Known)]
        [InlineData(KeyType.KnownGpg)]
        public void DecryptFileAndVerify_DecryptWithWrongKey(KeyType keyType)
        {
            // Arrange
            Arrange(keyType);
            PGP pgp = new PGP();

            // Act
            pgp.EncryptFileAndSign(contentFilePath, encryptedContentFilePath, publicKeyFilePath1, privateKeyFilePath1, password1);
            var ex = Assert.Throws<PgpException>(() => pgp.DecryptFileAndVerify(encryptedContentFilePath,
                decryptedContentFilePath1, publicKeyFilePath2, privateKeyFilePath1, password1));
           
            string decryptedContent = File.ReadAllText(decryptedContentFilePath1);

            // Assert
            Assert.Equal("Failed to verify file.", ex.Message);
            Assert.True(File.Exists(encryptedContentFilePath));
            Assert.True(File.Exists(decryptedContentFilePath1));
            Assert.Equal(string.Empty, decryptedContent.Trim());

            // Teardown
            Teardown();
        }
        
        [Theory]
        [InlineData(KeyType.Generated)]
        [InlineData(KeyType.Known)]
        [InlineData(KeyType.KnownGpg)]
        public void DecryptFileAndVerify_DecryptSignedAndEncryptedFile(KeyType keyType)
        {
            // Arrange
            Arrange(keyType);
            PGP pgp = new PGP();

            // Act
            pgp.EncryptFileAndSign(contentFilePath, encryptedContentFilePath, publicKeyFilePath1, privateKeyFilePath1, password1);
            pgp.DecryptFileAndVerify(encryptedContentFilePath, decryptedContentFilePath1, publicKeyFilePath1, privateKeyFilePath1, password1);
            string decryptedContent = File.ReadAllText(decryptedContentFilePath1);

            // Assert
            Assert.True(File.Exists(encryptedContentFilePath));
            Assert.True(File.Exists(decryptedContentFilePath1));
            Assert.Equal(content, decryptedContent.Trim());

            // Teardown
            Teardown();
        }
        
        [Theory]
        [InlineData(KeyType.Generated)]
        [InlineData(KeyType.Known)]
        [InlineData(KeyType.KnownGpg)]
        public void DecryptFileAndVerify_DecryptSignedAndEncryptedFileDifferentKeys(KeyType keyType)
        {
            // Arrange
            Arrange(keyType);
            PGP pgp = new PGP();

            // Act
            pgp.EncryptFileAndSign(contentFilePath, encryptedContentFilePath, publicKeyFilePath2, privateKeyFilePath1, password1);
            pgp.DecryptFileAndVerify(encryptedContentFilePath, decryptedContentFilePath1, publicKeyFilePath1, privateKeyFilePath2, password2);
            string decryptedContent = File.ReadAllText(decryptedContentFilePath1);

            // Assert
            Assert.True(File.Exists(encryptedContentFilePath));
            Assert.True(File.Exists(decryptedContentFilePath1));
            Assert.Equal(content, decryptedContent.Trim());

            // Teardown
            Teardown();
        }

        [Theory]
        [InlineData(KeyType.Generated)]
        [InlineData(KeyType.Known)]
        [InlineData(KeyType.KnownGpg)]
        public void Verify_VerifyEncryptedAndSignedFile(KeyType keyType)
        {
            // Arrange
            Arrange(keyType);
            PGP pgp = new PGP();

            // Act
            pgp.EncryptFileAndSign(contentFilePath, encryptedContentFilePath, publicKeyFilePath1, privateKeyFilePath1, password1);
            bool verified = pgp.VerifyFile(encryptedContentFilePath, publicKeyFilePath1);

            // Assert
            Assert.True(File.Exists(encryptedContentFilePath));
            Assert.True(verified);

            // Teardown
            Teardown();
        }

        [Theory]
        [InlineData(KeyType.Generated)]
        [InlineData(KeyType.Known)]
        [InlineData(KeyType.KnownGpg)]
        public void Verify_DoNotVerifyEncryptedAndSignedFile(KeyType keyType)
        {
            // Arrange
            Arrange(keyType);
            PGP pgp = new PGP();

            // Act
            pgp.EncryptFileAndSign(contentFilePath, encryptedContentFilePath, publicKeyFilePath1, privateKeyFilePath1, password1);
            bool verified = pgp.VerifyFile(encryptedContentFilePath, publicKeyFilePath2);

            // Assert
            Assert.True(File.Exists(encryptedContentFilePath));
            Assert.False(verified);

            // Teardown
            Teardown();
        }

        [Theory]
        [InlineData(KeyType.Generated)]
        [InlineData(KeyType.Known)]
        [InlineData(KeyType.KnownGpg)]
        public void Verify_VerifySignedFile(KeyType keyType)
        {
            // Arrange
            Arrange(keyType);
            PGP pgp = new PGP();

            // Act
            pgp.SignFile(contentFilePath, signedContentFilePath, privateKeyFilePath1, password1);
            bool verified = pgp.VerifyFile(signedContentFilePath, publicKeyFilePath1);

            // Assert
            Assert.True(File.Exists(signedContentFilePath));
            Assert.True(verified);

            // Teardown
            Teardown();
        }

        [Theory]
        [InlineData(KeyType.Generated)]
        [InlineData(KeyType.Known)]
        [InlineData(KeyType.KnownGpg)]
        public void Verify_DoNotVerifySignedFile(KeyType keyType)
        {
            // Arrange
            Arrange(keyType);
            PGP pgp = new PGP();

            // Act
            pgp.SignFile(contentFilePath, signedContentFilePath, privateKeyFilePath1, password1);
            bool verified = pgp.VerifyFile(signedContentFilePath, publicKeyFilePath2);

            // Assert
            Assert.True(File.Exists(signedContentFilePath));
            Assert.False(verified);

            // Teardown
            Teardown();
        }
        #endregion File

        #region Stream
        [Theory]
        [InlineData(KeyType.Generated)]
        [InlineData(KeyType.Known)]
        [InlineData(KeyType.KnownGpg)]
        public void EncryptStream_CreateEncryptedFile(KeyType keyType)
        {
            // Arrange
            Arrange(keyType);
            PGP pgp = new PGP();

            // Act
            using (FileStream inputFileStream = new FileStream(contentFilePath, FileMode.Open))
            using (Stream outputFileStream = File.Create(encryptedContentFilePath))
            using (Stream publicKeyStream = new FileStream(publicKeyFilePath1, FileMode.Open))
                pgp.EncryptStream(inputFileStream, outputFileStream, publicKeyStream);

            // Assert
            Assert.True(File.Exists(encryptedContentFilePath));

            // Teardown
            Teardown();
        }

        [Theory]
        [InlineData(KeyType.Generated)]
        [InlineData(KeyType.Known)]
        [InlineData(KeyType.KnownGpg)]
        public void SignStream_CreateSignedFile(KeyType keyType)
        {
            // Arrange
            Arrange(keyType);
            PGP pgp = new PGP();

            // Act
            using (FileStream inputFileStream = new FileStream(contentFilePath, FileMode.Open))
            using (Stream outputFileStream = File.Create(encryptedContentFilePath))
            using (Stream privateKeyStream = new FileStream(privateKeyFilePath1, FileMode.Open))
                pgp.SignStream(inputFileStream, outputFileStream, privateKeyStream, password1);

            // Assert
            Assert.True(File.Exists(encryptedContentFilePath));

            // Teardown
            Teardown();
        }

        [Theory]
        [InlineData(KeyType.Generated)]
        [InlineData(KeyType.Known)]
        [InlineData(KeyType.KnownGpg)]
        public void EncryptStream_CreateEncryptedStreamWithMultipleKeys(KeyType keyType)
        {
            // Arrange
            Arrange(keyType);
            PGP pgp = new PGP();

            // Act
            using (FileStream inputFileStream = new FileStream(contentFilePath, FileMode.Open))
            using (Stream outputFileStream = File.Create(encryptedContentFilePath))
            using (Stream publicKeyStream1 = new FileStream(publicKeyFilePath1, FileMode.Open))
            using (Stream publicKeyStream2 = new FileStream(publicKeyFilePath2, FileMode.Open))
                pgp.EncryptStream(inputFileStream, outputFileStream, new List<Stream>() { publicKeyStream1, publicKeyStream2 });

            // Assert
            Assert.True(File.Exists(encryptedContentFilePath));

            // Teardown
            Teardown();
        }

        [Theory]
        [InlineData(KeyType.Generated)]
        [InlineData(KeyType.Known)]
        [InlineData(KeyType.KnownGpg)]
        public void EncryptStreamAndSign_CreateEncryptedAndSignedStream(KeyType keyType)
        {
            // Arrange
            Arrange(keyType);
            PGP pgp = new PGP();

            // Act
            using (FileStream inputFileStream = new FileStream(contentFilePath, FileMode.Open))
            using (Stream outputFileStream = File.Create(encryptedContentFilePath))
            using (Stream publicKeyStream = new FileStream(publicKeyFilePath1, FileMode.Open))
            using (Stream privateKeyStream = new FileStream(privateKeyFilePath1, FileMode.Open))
                pgp.EncryptStreamAndSign(inputFileStream, outputFileStream, publicKeyStream, privateKeyStream, password1);

            // Assert
            Assert.True(File.Exists(encryptedContentFilePath));

            // Teardown
            Teardown();
        }

        [Theory]
        [InlineData(KeyType.Generated)]
        [InlineData(KeyType.Known)]
        [InlineData(KeyType.KnownGpg)]
        public void EncryptStreamAndSign_CreateEncryptedAndSignedStreamWithMultipleKeys(KeyType keyType)
        {
            // Arrange
            Arrange(keyType);
            PGP pgp = new PGP();

            // Act
            using (FileStream inputFileStream = new FileStream(contentFilePath, FileMode.Open))
            using (Stream outputFileStream = File.Create(encryptedContentFilePath))
            using (Stream publicKeyStream1 = new FileStream(publicKeyFilePath1, FileMode.Open))
            using (Stream publicKeyStream2 = new FileStream(publicKeyFilePath2, FileMode.Open))
            using (Stream privateKeyStream = new FileStream(privateKeyFilePath1, FileMode.Open))
                pgp.EncryptStreamAndSign(inputFileStream, outputFileStream, new List<Stream>() { publicKeyStream1, publicKeyStream2 }, privateKeyStream, password1);

            // Assert
            Assert.True(File.Exists(encryptedContentFilePath));

            // Teardown
            Teardown();
        }

        [Theory]
        [InlineData(KeyType.Generated)]
        [InlineData(KeyType.Known)]
        [InlineData(KeyType.KnownGpg)]
        public void DecryptStream_DecryptEncryptedStream(KeyType keyType)
        {
            // Arrange
            Arrange(keyType);
            PGP pgp = new PGP();

            // Act
            using (FileStream inputFileStream = new FileStream(contentFilePath, FileMode.Open))
            using (Stream outputFileStream = File.Create(encryptedContentFilePath))
            using (Stream publicKeyStream = new FileStream(publicKeyFilePath1, FileMode.Open))
                pgp.EncryptStream(inputFileStream, outputFileStream, publicKeyStream);

            using (FileStream inputFileStream = new FileStream(encryptedContentFilePath, FileMode.Open))
            using (Stream outputFileStream = File.Create(decryptedContentFilePath1))
            using (Stream privateKeyStream = new FileStream(privateKeyFilePath1, FileMode.Open))
                pgp.DecryptStream(inputFileStream, outputFileStream, privateKeyStream, password1);

            string decryptedContent = File.ReadAllText(decryptedContentFilePath1);

            // Assert
            Assert.True(File.Exists(encryptedContentFilePath));
            Assert.True(File.Exists(decryptedContentFilePath1));
            Assert.Equal(content, decryptedContent.Trim());

            // Teardown
            Teardown();
        }

        [Theory]
        [InlineData(KeyType.Generated)]
        [InlineData(KeyType.Known)]
        [InlineData(KeyType.KnownGpg)]
        public void DecryptStream_DecryptEncryptedStreamWithMultipleKeys(KeyType keyType)
        {
            // Arrange
            Arrange(keyType);
            PGP pgp = new PGP();

            // Act
            using (FileStream inputFileStream = new FileStream(contentFilePath, FileMode.Open))
            using (Stream outputFileStream = File.Create(encryptedContentFilePath))
            using (Stream publicKeyStream1 = new FileStream(publicKeyFilePath1, FileMode.Open))
            using (Stream publicKeyStream2 = new FileStream(publicKeyFilePath2, FileMode.Open))
                pgp.EncryptStream(inputFileStream, outputFileStream, new List<Stream>() { publicKeyStream1, publicKeyStream2 });

            using (FileStream inputFileStream = new FileStream(encryptedContentFilePath, FileMode.Open))
            using (Stream outputFileStream = File.Create(decryptedContentFilePath1))
            using (Stream privateKeyStream = new FileStream(privateKeyFilePath1, FileMode.Open))
                pgp.DecryptStream(inputFileStream, outputFileStream, privateKeyStream, password1);

            using (FileStream inputFileStream = new FileStream(encryptedContentFilePath, FileMode.Open))
            using (Stream outputFileStream = File.Create(decryptedContentFilePath2))
            using (Stream privateKeyStream = new FileStream(privateKeyFilePath2, FileMode.Open))
                pgp.DecryptStream(inputFileStream, outputFileStream, privateKeyStream, password2);

            string decryptedContent1 = File.ReadAllText(decryptedContentFilePath1);
            string decryptedContent2 = File.ReadAllText(decryptedContentFilePath2);

            // Assert
            Assert.True(File.Exists(encryptedContentFilePath));
            Assert.True(File.Exists(decryptedContentFilePath1));
            Assert.True(File.Exists(decryptedContentFilePath2));
            Assert.Equal(content, decryptedContent1.Trim());
            Assert.Equal(content, decryptedContent2.Trim());

            // Teardown
            Teardown();
        }

        [Theory]
        [InlineData(KeyType.Generated)]
        [InlineData(KeyType.Known)]
        [InlineData(KeyType.KnownGpg)]
        public void DecryptStream_DecryptSignedAndEncryptedStream(KeyType keyType)
        {
            // Arrange
            Arrange(keyType);
            PGP pgp = new PGP();

            // Act
            using (FileStream inputFileStream = new FileStream(contentFilePath, FileMode.Open))
            using (Stream outputFileStream = File.Create(encryptedContentFilePath))
            using (Stream publicKeyStream = new FileStream(publicKeyFilePath1, FileMode.Open))
            using (Stream privateKeyStream = new FileStream(privateKeyFilePath1, FileMode.Open))
                pgp.EncryptStreamAndSign(inputFileStream, outputFileStream, publicKeyStream, privateKeyStream, password1);

            using (FileStream inputFileStream = new FileStream(encryptedContentFilePath, FileMode.Open))
            using (Stream outputFileStream = File.Create(decryptedContentFilePath1))
            using (Stream privateKeyStream = new FileStream(privateKeyFilePath1, FileMode.Open))
                pgp.DecryptStream(inputFileStream, outputFileStream, privateKeyStream, password1);

            string decryptedContent = File.ReadAllText(decryptedContentFilePath1);

            bool verified = pgp.VerifyFile(encryptedContentFilePath, publicKeyFilePath1);

            // Assert
            Assert.True(File.Exists(encryptedContentFilePath));
            Assert.True(File.Exists(decryptedContentFilePath1));
            Assert.Equal(content, decryptedContent.Trim());
            Assert.True(verified);

            // Teardown
            Teardown();
        }

        [Theory]
        [InlineData(KeyType.Generated)]
        [InlineData(KeyType.Known)]
        [InlineData(KeyType.KnownGpg)]
        public void DecryptStream_DecryptSignedAndEncryptedStreamWithMultipleKeys(KeyType keyType)
        {
            // Arrange
            Arrange(keyType);
            PGP pgp = new PGP();

            // Act
            using (FileStream inputFileStream = new FileStream(contentFilePath, FileMode.Open))
            using (Stream outputFileStream = File.Create(encryptedContentFilePath))
            using (Stream publicKeyStream1 = new FileStream(publicKeyFilePath1, FileMode.Open))
            using (Stream publicKeyStream2 = new FileStream(publicKeyFilePath2, FileMode.Open))
                pgp.EncryptStream(inputFileStream, outputFileStream, new List<Stream>() { publicKeyStream1, publicKeyStream2 });

            using (FileStream inputFileStream = new FileStream(encryptedContentFilePath, FileMode.Open))
            using (Stream outputFileStream = File.Create(decryptedContentFilePath1))
            using (Stream privateKeyStream = new FileStream(privateKeyFilePath1, FileMode.Open))
                pgp.DecryptStream(inputFileStream, outputFileStream, privateKeyStream, password1);

            using (FileStream inputFileStream = new FileStream(encryptedContentFilePath, FileMode.Open))
            using (Stream outputFileStream = File.Create(decryptedContentFilePath2))
            using (Stream privateKeyStream = new FileStream(privateKeyFilePath2, FileMode.Open))
                pgp.DecryptStream(inputFileStream, outputFileStream, privateKeyStream, password2);

            string decryptedContent1 = File.ReadAllText(decryptedContentFilePath1);
            string decryptedContent2 = File.ReadAllText(decryptedContentFilePath2);

            bool verified = pgp.VerifyFile(encryptedContentFilePath, publicKeyFilePath1);

            // Assert
            Assert.True(File.Exists(encryptedContentFilePath));
            Assert.True(File.Exists(decryptedContentFilePath1));
            Assert.True(File.Exists(decryptedContentFilePath2));
            Assert.Equal(content, decryptedContent1.Trim());
            Assert.Equal(content, decryptedContent2.Trim());
            Assert.True(verified);

            // Teardown
            Teardown();
        }

        [Theory]
        [InlineData(KeyType.Generated)]
        [InlineData(KeyType.Known)]
        [InlineData(KeyType.KnownGpg)]
        public void Verify_VerifyEncryptedAndSignedStream(KeyType keyType)
        {
            // Arrange
            Arrange(keyType);
            PGP pgp = new PGP();

            // Act
            using (FileStream inputFileStream = new FileStream(contentFilePath, FileMode.Open))
            using (Stream outputFileStream = File.Create(encryptedContentFilePath))
            using (Stream publicKeyStream = new FileStream(publicKeyFilePath1, FileMode.Open))
            using (Stream privateKeyStream = new FileStream(privateKeyFilePath1, FileMode.Open))
                pgp.EncryptStreamAndSign(inputFileStream, outputFileStream, publicKeyStream, privateKeyStream, password1);

            bool verified = pgp.VerifyFile(encryptedContentFilePath, publicKeyFilePath1);

            // Assert
            Assert.True(File.Exists(encryptedContentFilePath));
            Assert.True(verified);

            // Teardown
            Teardown();
        }

        [Theory]
        [InlineData(KeyType.Generated)]
        [InlineData(KeyType.Known)]
        [InlineData(KeyType.KnownGpg)]
        public void Verify_DoNotVerifyEncryptedAndSignedStream(KeyType keyType)
        {
            // Arrange
            Arrange(keyType);
            PGP pgp = new PGP();

            // Act
            using (FileStream inputFileStream = new FileStream(contentFilePath, FileMode.Open))
            using (Stream outputFileStream = File.Create(encryptedContentFilePath))
            using (Stream publicKeyStream = new FileStream(publicKeyFilePath1, FileMode.Open))
            using (Stream privateKeyStream = new FileStream(privateKeyFilePath1, FileMode.Open))
                pgp.EncryptStreamAndSign(inputFileStream, outputFileStream, publicKeyStream, privateKeyStream, password1);

            bool verified = pgp.VerifyFile(encryptedContentFilePath, publicKeyFilePath2);

            // Assert
            Assert.True(File.Exists(encryptedContentFilePath));
            Assert.False(verified);

            // Teardown
            Teardown();
        }

        [Theory]
        [InlineData(KeyType.Generated)]
        [InlineData(KeyType.Known)]
        [InlineData(KeyType.KnownGpg)]
        public void Verify_VerifySignedStream(KeyType keyType)
        {
            // Arrange
            Arrange(keyType);
            PGP pgp = new PGP();
            bool verified = false;

            // Act
            using (FileStream inputFileStream = new FileStream(contentFilePath, FileMode.Open))
            using (Stream outputFileStream = File.Create(signedContentFilePath))
            using (Stream privateKeyStream = new FileStream(privateKeyFilePath1, FileMode.Open))
                pgp.SignStream(inputFileStream, outputFileStream, privateKeyStream, password1);

            using (FileStream inputFileStream = new FileStream(signedContentFilePath, FileMode.Open))
            using (Stream publicKeyStream = new FileStream(publicKeyFilePath1, FileMode.Open))
                verified = pgp.VerifyStream(inputFileStream, publicKeyStream);

            // Assert
            Assert.True(File.Exists(signedContentFilePath));
            Assert.True(verified);

            // Teardown
            Teardown();
        }

        [Theory]
        [InlineData(KeyType.Generated)]
        [InlineData(KeyType.Known)]
        [InlineData(KeyType.KnownGpg)]
        public void Verify_DoNotVerifySignedStream(KeyType keyType)
        {
            // Arrange
            Arrange(keyType);
            PGP pgp = new PGP();
            bool verified = false;

            // Act
            using (FileStream inputFileStream = new FileStream(contentFilePath, FileMode.Open))
            using (Stream outputFileStream = File.Create(signedContentFilePath))
            using (Stream privateKeyStream = new FileStream(privateKeyFilePath1, FileMode.Open))
                pgp.SignStream(inputFileStream, outputFileStream, privateKeyStream, password1);

            using (FileStream inputFileStream = new FileStream(signedContentFilePath, FileMode.Open))
            using (Stream publicKeyStream = new FileStream(publicKeyFilePath2, FileMode.Open))
                verified = pgp.VerifyStream(inputFileStream, publicKeyStream);

            // Assert
            Assert.True(File.Exists(signedContentFilePath));
            Assert.False(verified);

            // Teardown
            Teardown();
        }
        #endregion Stream

        private void Arrange(KeyType keyType, FileType fileType = FileType.Known)
        {
            Directory.CreateDirectory(keyDirectory);
            Directory.CreateDirectory(contentDirectory);
            PGP pgp = new PGP();

            // Create keys
            if (keyType == KeyType.Generated)
            {
                pgp.GenerateKey(publicKeyFilePath1, privateKeyFilePath1, userName1, password1);
                pgp.GenerateKey(publicKeyFilePath2, privateKeyFilePath2, userName2, password2);
            }
            else if (keyType == KeyType.Known)
            {
                using (StreamWriter streamWriter = File.CreateText(publicKeyFilePath1))
                {
                    streamWriter.WriteLine(publicKey1);
                }

                using (StreamWriter streamWriter = File.CreateText(publicKeyFilePath2))
                {
                    streamWriter.WriteLine(publicKey2);
                }

                using (StreamWriter streamWriter = File.CreateText(privateKeyFilePath1))
                {
                    streamWriter.WriteLine(privateKey1);
                }

                using (StreamWriter streamWriter = File.CreateText(privateKeyFilePath2))
                {
                    streamWriter.WriteLine(privateKey2);
                }
            }
            else if (keyType == KeyType.KnownGpg)
            {
                using (StreamWriter streamWriter = File.CreateText(publicKeyFilePath1))
                {
                    streamWriter.WriteLine(publicGpgKey1);
                }

                using (StreamWriter streamWriter = File.CreateText(publicKeyFilePath2))
                {
                    streamWriter.WriteLine(publicGpgKey2);
                }

                using (StreamWriter streamWriter = File.CreateText(privateKeyFilePath1))
                {
                    streamWriter.WriteLine(privateGpgKey1);
                }

                using (StreamWriter streamWriter = File.CreateText(privateKeyFilePath2))
                {
                    streamWriter.WriteLine(privateGpgKey2);
                }
            }

            // Create content file
            if (fileType == FileType.Known)
            {
                using (StreamWriter streamWriter = File.CreateText(contentFilePath))
                {
                    streamWriter.WriteLine(content);
                }
            }
            else if (fileType == FileType.GeneratedLarge)
            {
                CreateRandomFile(contentFilePath, 7000);
            }
        }

        private void CreateRandomFile(string filePath, int sizeInMb)
        {
            // Note: block size must be a factor of 1MB to avoid rounding errors
            const int blockSize = 1024 * 8;
            const int blocksPerMb = (1024 * 1024) / blockSize;

            byte[] data = new byte[blockSize];

            using (RNGCryptoServiceProvider crypto = new RNGCryptoServiceProvider())
            {
                using (FileStream stream = File.OpenWrite(filePath))
                {
                    for (int i = 0; i < sizeInMb * blocksPerMb; i++)
                    {
                        crypto.GetBytes(data);
                        stream.Write(data, 0, data.Length);
                    }
                }
            }
        }

        private void Teardown()
        {
            // Remove keys
            if (File.Exists(publicKeyFilePath1))
            {
                File.Delete(publicKeyFilePath1);
            }

            if (File.Exists(privateKeyFilePath1))
            {
                File.Delete(privateKeyFilePath1);
            }

            if (File.Exists(publicKeyFilePath2))
            {
                File.Delete(publicKeyFilePath2);
            }

            if (File.Exists(privateKeyFilePath2))
            {
                File.Delete(privateKeyFilePath2);
            }

            if (Directory.Exists(keyDirectory))
            {
                Directory.Delete(keyDirectory);
            }

            // Remove content
            if (File.Exists(contentFilePath))
            {
                File.Delete(contentFilePath);
            }

            if (File.Exists(encryptedContentFilePath))
            {
                File.Delete(encryptedContentFilePath);
            }

            if (File.Exists(signedContentFilePath))
            {
                File.Delete(signedContentFilePath);
            }

            if (File.Exists(decryptedContentFilePath1))
            {
                File.Delete(decryptedContentFilePath1);
            }

            if (File.Exists(decryptedContentFilePath2))
            {
                File.Delete(decryptedContentFilePath2);
            }

            if (Directory.Exists(contentDirectory))
            {
                Directory.Delete(contentDirectory);
            }
        }

        public enum KeyType
        {
            Generated,
            Known,
            KnownGpg
        }

        public static IEnumerable<object[]> KeyTypeValues()
        {
            foreach (var keyType in Enum.GetValues(typeof(KeyType)))
            {
                yield return new object[] { keyType };
            }
        }

        public static IEnumerable<object[]> HashAlgorithmTagValues()
        {
            foreach (var hashAlgorithmTag in Enum.GetValues(typeof(HashAlgorithmTag)))
            {
                yield return new object[] { hashAlgorithmTag };
            }
        }

        public enum FileType
        {
            GeneratedLarge,
            Known
        }

        // Content
        const string contentDirectory = "./Content/";
        const string content = "The quick brown fox jumps over the lazy dog";
        const string contentFilePath = contentDirectory + "content.txt";
        const string encryptedContentFilePath = contentDirectory + "encryptedContent.pgp";
        const string signedContentFilePath = contentDirectory + "signedContent.pgp";
        const string decryptedContentFilePath1 = contentDirectory + "decryptedContent1.txt";
        const string decryptedContentFilePath2 = contentDirectory + "decryptedContent2.txt";
        const string encryptedContent1 = @"-----BEGIN PGP MESSAGE-----
Version: OpenPGP.js v3.0.9
Comment: https://openpgpjs.org

wcBMA+cxhM+dKt4UAQgA6MiSXq2KSOlAsGFl2DrCp/j7CIeFBSxc/elikmS0
9jvYV8yhTZ6F3N1Cj1tDQZ18d7Ih5npRkCXlCMKijTRJ6T4gChQ/rIAtA1hr
tSjz8UzHFetFxiXCacWUNK+Q1WRG7CKfClvF9tOBrG6WmKwkY+KzbDQ0vRzQ
1JRnHAfJ++fq5y3mJIlUoCNhgYMl5vDvr6rkGW7bFjFfB6amLdIHZn9Tc3GV
jRG6v5MxqAppEsqIhEgr17/6qSslU/IFTokNNsd0OTGTzTejmY49SPM3O6e9
Ou2hqUPPRovNuhqOtys6HpMU+mesprrdx6a7OeWnlDvCkg3N37LLpssyHqum
kNJjAaHUdUGuuQ8ZCtuu7NC/LdfCGu+WT0iQAR9kdLTwNOq1TgsYu68TEX1u
Dq3YVTdbdAF/uURDx4aexQDVTq8IDk32FwVSaES6PG5qCgR0RCkwkJGxruhT
sZg/AsVo3z+/sr7a
=4Wwg
-----END PGP MESSAGE-----";
        const string encryptedContent2 = @"-----BEGIN PGP MESSAGE-----
Version: OpenPGP.js v3.0.9
Comment: https://openpgpjs.org

wcBMA9QtMjxDkm//AQf6Aqxd0fr81dBjxP892DEtC9Nwq2AXFgBAnAlhTGIr
8zPrtr12V5V6aTOZ0IChldtsaEGwxVrodFhqWO4WKlFrpVon86RglOednHU/
/sJNbdsnW3t8dUbD8k8V+5pkba+oX6iklvzv8hpqAEMc7Gwp7fMcDPF00BkY
mhIBvZXpCbLRtQt/K4qo3kpRqZDJSWKGtGPtXDGtx7duCxR41ArleQjfGyxN
Be5bWPu0/gZWMkew62PFTDIqeBfRR7+V1PMRhwL0WJdgOqhRoDkNQhUPJ7aa
ALSy//blnbktrSZrR7vWo3lm2ZGFl0uzcpBM3pcFFMssieOPi+E7IovfZTW0
O9JjAUTVXka99zj8wPlezPqUsekTIhgVw5vso4gJTz3DsJR0jtTIWczgp5+U
1hay6pEQUCGasIB5OWQImpKmTNEHmv+jvXskuk4kuPy7gqOiWcN34XTmGGbz
MFHwXEtblMhDz7ni
=navA
-----END PGP MESSAGE-----";

        // Keys
        const string keyDirectory = "./Keys/";
        const string publicKeyFilePath1 = keyDirectory + "publicKey1.asc";
        const string publicKeyFilePath2 = keyDirectory + "publicKey2.asc";
        const string privateKeyFilePath1 = keyDirectory + "privateKey1.asc";
        const string privateKeyFilePath2 = keyDirectory + "privateKey2.asc";
        const string userName1 = "email1@email.com";
        const string userName2 = "email2@email.com";
        const string password1 = "password1";
        const string password2 = "password2";

        // Known keys, generated using https://pgpkeygen.com/
        const string publicKey1 = @"-----BEGIN PGP PUBLIC KEY BLOCK-----
Version: Keybase OpenPGP v1.0.0
Comment: https://keybase.io/crypto

xo0EXRzQ9gEEALWy0pmWiNwti765q5l/cgohqa5fKBZWy2VggB8YlLNSGaiR4Esd
Ya0+SSkwe0C3O9xjzUlQA/0SGYelxjgYhxqTyvLiVKKTx6HE1FW6PPrYMK4+GQaH
SfhO5ILLqXx0/o7XF77qSmxdrcQrIwNhdeOwDBDOrwLWDuU+Gx/F9AU9ABEBAAHN
I2VtYWlsMUBlbWFpbC5jb20gPGVtYWlsMUBlbWFpbC5jb20+wq0EEwEKABcFAl0c
0PYCGy8DCwkHAxUKCAIeAQIXgAAKCRAy61veQBr1zRx8A/43SUeO5lGjksMbZuqp
fiJFdjd3aT94jz7oukfUL/t+ToVtxRRSTr6aoYVclK21TP797zme86zsmM3fUKzO
nVCs4V4E9c7lz69hd2+PBhDX29a7fywFWOQ5dAavuHUAw8akLZdY7sWh720Gbh8Q
3GRdrUry78nmkAWuw8JBh71uX86NBF0c0PYBBADW3E+IuxoDxc1CSJBL8iLc4A9L
3FpWeifBbq5PCpjYcqodb1FvD5eaqYgqf5/hPQLdRP/XRHtKKkph+XdF5Wrx0AMC
sEgr6JZ3SicobLev028DADYugJcZ9E1T/nkkkggamQBX5ryxB6X8se0m27QTd06n
KIhN67qCX/Gi+3UkmwARAQABwsCDBBgBCgAPBQJdHND2BQkPCZwAAhsuAKgJEDLr
W95AGvXNnSAEGQEKAAYFAl0c0PYACgkQHCBL6iCIoI+EhQP+OgbEfsQwixiyVQaG
1D+RSAGAnARX2Y+VatAtRsWuEXNYeNjFsPDMRbgtoCfrAlQoL0wXQXu+TXOu9xkL
u3hq4Nd8+fvvE1znc1zT7Ie1Tb20luA7Qzk3lQV4w2nxpXL3hl7JN1KxmPwanrQv
bT99eh9lhceoQHls/g1+sjOtQ4Kr1wQAnUMopnAavdlnfpJYXTqHH6QI4uBYscNH
ZHa5OdLgFBzBx+IGvYpDZzTjxuAmbVvQZIkJi4iI0xua/ER/AJIdYgSUTbKT7nif
f8neNHVvJGTF1iYoORMFrQEjnYPwRaEnzMpLkCryBsGFjYfj1X2wrzNL5dEzU97M
R2qeFsfC3szOjQRdHND2AQQAp1m2xMs34pmeVzGqbmRcoASe+MHazJyv+L+XhEF0
OxThH4NKLJLXotib9KXZlgqfiETgmRvoLeQvBu2f/5Nf5TgGITcS8/0jyvolwv+9
IRPxXRBXbk3H89z5UqVFa2FkEnS21wQUMRYqUEzO1n04ImhAWOUDF3b8eOT1q2+A
HnMAEQEAAcLAgwQYAQoADwUCXRzQ9gUJDwmcAAIbLgCoCRAy61veQBr1zZ0gBBkB
CgAGBQJdHND2AAoJEEdOvSYcuM90w1YD/3XCcndLA4OIF7cJlo1DbPkN3cwtldvT
vyvf9n7G5epB99/wNjDrWzzFXWU+3oOOwnnQXk9oZoWOPmMp02OlZW7s3WLWj5ZQ
0RoEzM3cQRdpTU1oX02zNKoMGcHY5Tfiacfvr/EZx3ElsyZ81zIR0HtyXMwRrgTg
A4KsnnILrp6JpVkD/20JllnAfq7xIqGpQCFCs1CxYYDEfEuqxcQf+wpdICG6FqRn
P4IOoqsVnY2EEHwdr9VjKyf6L+Pd2PLou8pWCu6rF/M3zIjAwzzPsJ5/AlINTql0
b8xSWNM02DrVx932kcSOx4k8BaZ0IiSwzny4xZEoOIPKK8SZ+EZeZaeopZ7h
=O3ub
-----END PGP PUBLIC KEY BLOCK-----";
        const string publicKey2 = @"-----BEGIN PGP PUBLIC KEY BLOCK-----
Version: Keybase OpenPGP v1.0.0
Comment: https://keybase.io/crypto

xo0EXRzR8gEEAOJguWTfef08UottAIqsBxYh0Cea7QF4toOdSCOXwT70pY+uiwVj
gMgd1IqI8/uZg2orYsI2+6SYUjyNbYXOMIBgLt7LNz2Xu//RCqcVgdhpgusXnQoI
ru+BoT1H0IcgGAmwQ1MxvvTX5MmcDpzRBgNkpfQIsohmnYsX46GzYUpjABEBAAHN
I2VtYWlsMkBlbWFpbC5jb20gPGVtYWlsMkBlbWFpbC5jb20+wq0EEwEKABcFAl0c
0fICGy8DCwkHAxUKCAIeAQIXgAAKCRDHOc/0fnhNpuFhBAChzcCOwhGnNZTV2xFB
8CXbAt6mEfuxgcVdiKEKNZvvk75HJKmN0/5hW9ubfIGpu4oxsfFV7DEElKpCoj6K
513kM9J32wmfzx49mRJYXsMFeResF3XS1qN7JfY0o/vrI3HZAFwA2xddkK4NkXl+
r1TXO+VrJrW4FAc34a2OCGb5w86NBF0c0fIBBADSE2B/pYRFSSVmbuqQM+37BZhm
Hwk1aXlHVpX4IKV65SzVID9qrub2PrwClRdm1q+1wuaiEaWsT2obYRXLaXfsWb6F
3g9gumIoMd7k1T8rUsmVgddroyegtPsEFSNcSGtFKpBVwhMznTMqBkr4QMLxAyw0
fOSwag0Rc2ipBW+i/wARAQABwsCDBBgBCgAPBQJdHNHyBQkPCZwAAhsuAKgJEMc5
z/R+eE2mnSAEGQEKAAYFAl0c0fIACgkQUI7UIwZpecWdvwP9FekQEnaxm3i+Sevv
B8MQlIzuypOWBIqTWx8Xcw/ldkFZDfujFHBIvLULMXNxO8rrsRXii5w1gR0xVj5A
mxTp6v+q2z+fmRoVr0Ym/r/chNlkbR4Jle+QckPeSnhKMZEfLmB4D4K6tX4CUCSF
EoIx6oWWeIbTdeNCQnHvbGALpEkDIwQAx0ihTWXggVZXaCtyOFVJKwCK8EPKu3pR
vK64vzoNqlqxd7F8Qhzo971aR9vTOvS4CV78ovQFX02TZGHocRWZx1mGdrlVPZWp
OlzHR0vT0psBSvaFWqkaifOScEQ0ATKguJNvo+kHOKBW3p/F6zrzqcG94RCPkHf2
MrSSQubDtOfOjQRdHNHyAQQAu5YHRDMFBLa7afjPtkMooybqM1KSeC62jByXReRT
EfVIgRDdI+1p19z/hPBz//OSU0kN6ePrhYSlIvhT74Nk8CTpvAwpS1791SC7mwxU
wZK5jNMi5HrfOlGwlhasdSe+v3xiSbSkHEtwPscBbyBWSqqGZbZIkk1OfjtBcK3P
55kAEQEAAcLAgwQYAQoADwUCXRzR8gUJDwmcAAIbLgCoCRDHOc/0fnhNpp0gBBkB
CgAGBQJdHNHyAAoJEEbXCPn6ISugs+kD/340wN26UWPXEJUugy+yjpixYkU3T6vS
V0QzF3188TEUhrVd6TBVea7HBQOsg+aSZQTrEICfcmif6zmJ9r+6Q5BNuIc8wy7G
zkBJ7kR/XyfAHN5MNLfdBnHSZZqRwIbrm4rVNIOjXhLVUNaOF3v9wlor7JNVoXP/
+3yMMp8k32a28HUEAN988ZbipEZFyZjhZWPQbpuNA0LxRiqV4HMoCiJ+jBM3lGVp
O3IEvHTXyUErcgSBekr3BhIuHTHwt9RWTVNWBku8UsX9Ao1M8vRWimNwIlGdBrIT
iSJGzF6qhiiorxaJkMNx7xDgxQFZgHiihjIsolKego98NLI8e9j7+6zOHR0f
=lgvU
-----END PGP PUBLIC KEY BLOCK-----";
        const string publicGpgKey1 = @"-----BEGIN PGP PUBLIC KEY BLOCK-----

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
=g+zu
-----END PGP PUBLIC KEY BLOCK-----";
        const string publicGpgKey2 = @"-----BEGIN PGP PUBLIC KEY BLOCK-----

mQENBF04J+YBCADdpMrZ2j2rzwWAU+b8d6CUpE+W7IpY/0/ZZjD6yinyiHou7T56
PUbr1vA0E+GjNM+iLhG4BdxJhkU3F1HJ2j6kFHP2iupkVYFs0jGEtO0wHpFpyrun
eOrEwWXHMn4SjR4a1Sjo/WWJi8Q9klypaTinPgbq45Sn4XRTXrAHwV/edKSBbFZ/
MDq2bPnHpy22AVoA3h78pbEShbIcpMa1fr9iVjhEwYK2oRsx/LqXmHBlE/+tf1WB
hkmV1lxRhMv6ZtLTB3DpmSMkrIyP6yy4+b++dwd3PX21UJlYYE3Ivv+wDie/nkbc
+uDs+RLeNfwVSAzhKdch6gsz+xtww+zN6Lu5ABEBAAG0Hm1hdHRvc2F1cnVzIDxl
bWFpbDJAZW1haWwuY29tPokBVAQTAQgAPhYhBLorJjEIguwDBiVJmM8lFD3YPt2S
BQJdOCfmAhsDBQkDwmcABQsJCAcCBhUKCQgLAgQWAgMBAh4BAheAAAoJEM8lFD3Y
Pt2SObwIALkoUY29RZDN0DzC4BexldMBQuGJzAF/Peaf8A0FUvE+xNoXgcHzFIPB
zI3vAUd09cxYc6EXhMxQi4mEVzTluAtuMO1y6dXYFflPL7LRKgdiZSW3GMcpn3Yy
2osbUX2w0HJMUjdtuZHS5aYnFRFGVCDF9QbXb5JP9atYJud0yjY6nKh3pCkUdPLC
2EhebmmZsEZyHvuThRQ1g7ZPpX01tAFrxZ4E2B2bZa67uQDOvqdTgZnT5GeOPcqb
5hsHwzFIivd76I2fdQn3PK0KoDJ6X+0embKJZTno6mr9WTGHfu5pO/7FtoUufjdt
NDp4rSaYmDpdPwLLPYQNM9tG6g503qO5AQ0EXTgn5gEIALvG66UdJIYV8k6PWxPT
gt45qphgzPSfgbV1VwXLbNOK3hRf3AtrVjCbFjeBT82eVeG4Q4WRmPSaShOzGzve
Wcr0YTU3QOfr4k6Z8oldYkDPDbJ4PpK+u+NgVspzjrPoCQhUi490OunDDupnCKET
e8DzIrE2QQ2pCruhjFBZPRTbq1zlhcJ6Xhf1OVW7VcCaaFZ+N9pBbjhXAc4frs+/
JZhKBVWCMM04DWvHjeEPKoXRivZviHy6os5OUYts+BlWLHkIv2XM8FqjGdlM4s7v
s9sTNT6LupG+dcgo4CYuax5Rh7KrS5P3pHSKaffaEB++ho+Gi8p9jJ2DY8MTwTlv
tr0AEQEAAYkBPAQYAQgAJhYhBLorJjEIguwDBiVJmM8lFD3YPt2SBQJdOCfmAhsM
BQkDwmcAAAoJEM8lFD3YPt2SIOwH/1MH0waFa6zRapYr1uSNANAWnB1yHFtMQgt/
wxHteAThbDKkH68hpNY+MsMAlopLNb5dzSbwa8rzqIGf9+gTr3ZCOO8LoNrrurQf
4bgNi87QT+if1euRE4bMBG++ztyHj7tf2sDAufr30cGJ/1Kk44mgp/ZPelN8bgQ3
/pJEcFQdvS92Aqb6ZoDS0Vj/iSEowXSKzxWmNmF5O2RBIv+pEHAyTes8vrIsU3zf
AP5/T4AbSvis6g4hE9vS/whp4tARY7D7WjFXFWD3fIAiC0KHZPUudy2g8Hcwr5M9
pCNCbkyRFW+kY0x8hZKez3AsiXtDSiDG9WD1NbrUvEAxaf2MkSU=
=LNCL
-----END PGP PUBLIC KEY BLOCK-----";
        const string privateKey1 = @"-----BEGIN PGP PRIVATE KEY BLOCK-----
Version: Keybase OpenPGP v1.0.0
Comment: https://keybase.io/crypto

xcFGBF0c0PYBBAC1stKZlojcLYu+uauZf3IKIamuXygWVstlYIAfGJSzUhmokeBL
HWGtPkkpMHtAtzvcY81JUAP9EhmHpcY4GIcak8ry4lSik8ehxNRVujz62DCuPhkG
h0n4TuSCy6l8dP6O1xe+6kpsXa3EKyMDYXXjsAwQzq8C1g7lPhsfxfQFPQARAQAB
/gkDCAxMEPFE1TAdYHfWNqa10kyokP1r6n586PNtCKl0DGuGKl8v0irCxCIDOBKj
8JInmsV8AfZzPfUFF+/f8v/svVZDZ1CXtoLcagYygPEZ+MPQF3QpBzHUk6ug9A5Q
1LBB1q5Z7ETytFYg4Tp2bDcOtY10/VHSt6f8B0eSNseh259++XanQ5Qi8aJeL8j6
0yQfmmVKB2uLST9UWYV19pz6qs3pSlahDf6wD5e9OGNWtJoDSH3Ioj/Vnrm5664Y
0fXCukCCgN8rgWSMODtckmXX9y70IWDwTsfEX6XeOFIOeFvi5WCBj/FGUwxQwC0I
IPl/iNsFoDFD1hy3rAoSzPoW7pfVu/KozJ/qik7ytfWpSeZ80XzPAGfvt9eqfLO+
ixStJ0YxZTICd2fMAsVc78OwLHkyEbKcnOmY/8vOTP+K/qncIcfEcta3AEzIqRG8
j5ZM+EecAZXqxUJDROrdSYp0BuczwhuSV2Pl/Vf5NDPBLpR1mHY0AjLNI2VtYWls
MUBlbWFpbC5jb20gPGVtYWlsMUBlbWFpbC5jb20+wq0EEwEKABcFAl0c0PYCGy8D
CwkHAxUKCAIeAQIXgAAKCRAy61veQBr1zRx8A/43SUeO5lGjksMbZuqpfiJFdjd3
aT94jz7oukfUL/t+ToVtxRRSTr6aoYVclK21TP797zme86zsmM3fUKzOnVCs4V4E
9c7lz69hd2+PBhDX29a7fywFWOQ5dAavuHUAw8akLZdY7sWh720Gbh8Q3GRdrUry
78nmkAWuw8JBh71uX8fBRgRdHND2AQQA1txPiLsaA8XNQkiQS/Ii3OAPS9xaVnon
wW6uTwqY2HKqHW9Rbw+XmqmIKn+f4T0C3UT/10R7SipKYfl3ReVq8dADArBIK+iW
d0onKGy3r9NvAwA2LoCXGfRNU/55JJIIGpkAV+a8sQel/LHtJtu0E3dOpyiITeu6
gl/xovt1JJsAEQEAAf4JAwg+oGGJN3BjzWDfy1uxVuLcebJbePiKo+zRm3ztdIpu
BbxAIDkAoKJVnmD8Meh7xvjT9W+GJ5tn0R/UfQLTWSUdprV/7bOQb4YPXaVgAFVX
A6ZzHtKjHP9AN8ncazaTz60GxmQ0EDFaaGEfrfUdHYIytXko10UdMqqpid4/Iund
uvvprM70kcnkphfkd4RQRq1Y/wt8k0yHdnnxmfOh40gygPSAKxqx4nrJTGOAvZsM
T62gL050bzNphVDpJfBHDAD9XFfA97d8p2VO74VZnSd04OB/hu8Ba1gsulSr/wwo
3TFZ9gi+Cbg3OxU46pQWxComOQtlqADQ7N+EanMi6dEyrrTxO0knlfI0xQoX1TMO
keK2HXcMdMxkoEuvyhUdM52ggNhVhRtwAB05d9ztCsk02TMNFZLaCPlTiOkabVzw
FidJnEp+lvGfOHiffnvr8Q1qXF31wFTtCJfd/qm64kzOGkM/rK4RGVJQJaBq5Tps
FasCjCHrwsCDBBgBCgAPBQJdHND2BQkPCZwAAhsuAKgJEDLrW95AGvXNnSAEGQEK
AAYFAl0c0PYACgkQHCBL6iCIoI+EhQP+OgbEfsQwixiyVQaG1D+RSAGAnARX2Y+V
atAtRsWuEXNYeNjFsPDMRbgtoCfrAlQoL0wXQXu+TXOu9xkLu3hq4Nd8+fvvE1zn
c1zT7Ie1Tb20luA7Qzk3lQV4w2nxpXL3hl7JN1KxmPwanrQvbT99eh9lhceoQHls
/g1+sjOtQ4Kr1wQAnUMopnAavdlnfpJYXTqHH6QI4uBYscNHZHa5OdLgFBzBx+IG
vYpDZzTjxuAmbVvQZIkJi4iI0xua/ER/AJIdYgSUTbKT7niff8neNHVvJGTF1iYo
ORMFrQEjnYPwRaEnzMpLkCryBsGFjYfj1X2wrzNL5dEzU97MR2qeFsfC3szHwUYE
XRzQ9gEEAKdZtsTLN+KZnlcxqm5kXKAEnvjB2sycr/i/l4RBdDsU4R+DSiyS16LY
m/Sl2ZYKn4hE4Jkb6C3kLwbtn/+TX+U4BiE3EvP9I8r6JcL/vSET8V0QV25Nx/Pc
+VKlRWthZBJ0ttcEFDEWKlBMztZ9OCJoQFjlAxd2/Hjk9atvgB5zABEBAAH+CQMI
szCWLsNc0ZpghwQQszYzu3csbLUin7OzEYMjpAgWMuM4Iu2bgxDBvF9NIShozZjj
tBYJDdFIKzpcKn/1r1VzLgK6sxlq11MD9RBqialqhUPCYeBKRh5RCTYJG6iRLvQ2
FYeqe5JAYwak61Pq1FfvzcGuhB67IIVyR+CIY2ibGX/HL22G89DDYIAyvAwbaGTV
iMNJx3TCv91DOYRbn6+4h/ci6vBQryo/dN/m+7xXkHmmXH3xHw8sZcAdHGWk6bqB
z+D7SiZGKUJyF/rWzkMJBZBEhq0vkOE/VWZQ+asgv177M71V+OvEcNW3tzpXQiGb
hbyejM0aPd6NrUs1NwMVefqXO7kaMwUBHjCJObmdbqtffxB9BQsCaMPdsHuZMrTn
gd1blddefuombaiYZPa2n7rFe0VR+oNps+yZHYxmi3/SQkIszx5wEhQna0vg1zLf
apTjrF4sa3wjxShW5KOM4Tm0vL8Ln8gkfVeOfaZFNH+HnbOY7MLAgwQYAQoADwUC
XRzQ9gUJDwmcAAIbLgCoCRAy61veQBr1zZ0gBBkBCgAGBQJdHND2AAoJEEdOvSYc
uM90w1YD/3XCcndLA4OIF7cJlo1DbPkN3cwtldvTvyvf9n7G5epB99/wNjDrWzzF
XWU+3oOOwnnQXk9oZoWOPmMp02OlZW7s3WLWj5ZQ0RoEzM3cQRdpTU1oX02zNKoM
GcHY5Tfiacfvr/EZx3ElsyZ81zIR0HtyXMwRrgTgA4KsnnILrp6JpVkD/20JllnA
fq7xIqGpQCFCs1CxYYDEfEuqxcQf+wpdICG6FqRnP4IOoqsVnY2EEHwdr9VjKyf6
L+Pd2PLou8pWCu6rF/M3zIjAwzzPsJ5/AlINTql0b8xSWNM02DrVx932kcSOx4k8
BaZ0IiSwzny4xZEoOIPKK8SZ+EZeZaeopZ7h
=i6tW
-----END PGP PRIVATE KEY BLOCK-----";
        const string privateKey2 = @"-----BEGIN PGP PRIVATE KEY BLOCK-----
Version: Keybase OpenPGP v1.0.0
Comment: https://keybase.io/crypto

xcFGBF0c0fIBBADiYLlk33n9PFKLbQCKrAcWIdAnmu0BeLaDnUgjl8E+9KWProsF
Y4DIHdSKiPP7mYNqK2LCNvukmFI8jW2FzjCAYC7eyzc9l7v/0QqnFYHYaYLrF50K
CK7vgaE9R9CHIBgJsENTMb701+TJnA6c0QYDZKX0CLKIZp2LF+Ohs2FKYwARAQAB
/gkDCGCPgQ5tMrgKYIoTL6JCozh+dp4lf01ivRMEu5BcUy3Dj9lZZZhIyJuZ+ipM
Nj6ouw+rT8Gu21xA1CH6FJHeVhT584I4/H2LbFf8L8thZvr45EA8UqsvJ7mXXAHj
XWsS9onzQ9N2Ll5rgDsC8Az1aP0+pgxGqvv/KR7DFowGV+rosHlo85i7q6tkHMWD
1LyaweCED09DEncO3oCuTXgUVCjxzq+XWP9v/aO9KsDMcxD2BpIRlBv5rKCxHHIT
ubqHlD2SAAM/N1l0KgTun3O3IwNtXRXtH5HGnKQUevCG5ehM0DNbqUJ4osaBI0YA
OH3RXWRhkNdzC4mhwCB06E+m+pubN5cLNEWPg0vRj7PDQ7IM58U7UGOnElaomPmQ
a9dT8krf4y1VfvFEUGAGVFpeJdGqjdaTS7xr5PqlHO597k0Q2kopsQhIdaDYdQpr
rldYofrOK5aGEGwOpYjv0sxBf6RPcc2EGSO7YDLYpQb7Lt5OB5zaMRnNI2VtYWls
MkBlbWFpbC5jb20gPGVtYWlsMkBlbWFpbC5jb20+wq0EEwEKABcFAl0c0fICGy8D
CwkHAxUKCAIeAQIXgAAKCRDHOc/0fnhNpuFhBAChzcCOwhGnNZTV2xFB8CXbAt6m
EfuxgcVdiKEKNZvvk75HJKmN0/5hW9ubfIGpu4oxsfFV7DEElKpCoj6K513kM9J3
2wmfzx49mRJYXsMFeResF3XS1qN7JfY0o/vrI3HZAFwA2xddkK4NkXl+r1TXO+Vr
JrW4FAc34a2OCGb5w8fBRgRdHNHyAQQA0hNgf6WERUklZm7qkDPt+wWYZh8JNWl5
R1aV+CCleuUs1SA/aq7m9j68ApUXZtavtcLmohGlrE9qG2EVy2l37Fm+hd4PYLpi
KDHe5NU/K1LJlYHXa6MnoLT7BBUjXEhrRSqQVcITM50zKgZK+EDC8QMsNHzksGoN
EXNoqQVvov8AEQEAAf4JAwhM646/tFL92WCf8Zsle1X1wkMRcdXXA00tt4dz58r8
6I7jyi4I7NtUkwEnUDIAhELQMiCpYfGkEmnH79PoRgZ3TKiPdQGloKoEIR/RL0DB
YiTnzSFlej/zzMWf1gICiAzD0YW7n57LeOgR+nE4+saBN6KVpUOcKjjTIzt1Py1B
pB9HOBx5T2DHCgP0PqoBLVJ2Ni3tf8jn9ijSJxHMWh0HfHywD0tCvIu3rR6OcM3H
voDm4Gx4xo5Sryn9v6SFdSotfl2xYCckFJdexKLCdjJzghQQ/2WtFJigewS68T/8
ueeT3QNbO4JeYK0kKC083W097JMMdbS0/Ppg9594jCvG0eQxXSoysls3gv9dArih
EASW1ZXjG4uom/O+mdPADUCUWs8KJX2F7vmztmknCZeklmzLcgbhaNV+inBGrcG1
Z09rtnOpNUBspU2U/BGwqhmvwBQwYcbNHmdYe1qpl1cj4qvciq0LtOULk11bZyhn
x9k8tFCKwsCDBBgBCgAPBQJdHNHyBQkPCZwAAhsuAKgJEMc5z/R+eE2mnSAEGQEK
AAYFAl0c0fIACgkQUI7UIwZpecWdvwP9FekQEnaxm3i+SevvB8MQlIzuypOWBIqT
Wx8Xcw/ldkFZDfujFHBIvLULMXNxO8rrsRXii5w1gR0xVj5AmxTp6v+q2z+fmRoV
r0Ym/r/chNlkbR4Jle+QckPeSnhKMZEfLmB4D4K6tX4CUCSFEoIx6oWWeIbTdeNC
QnHvbGALpEkDIwQAx0ihTWXggVZXaCtyOFVJKwCK8EPKu3pRvK64vzoNqlqxd7F8
Qhzo971aR9vTOvS4CV78ovQFX02TZGHocRWZx1mGdrlVPZWpOlzHR0vT0psBSvaF
WqkaifOScEQ0ATKguJNvo+kHOKBW3p/F6zrzqcG94RCPkHf2MrSSQubDtOfHwUUE
XRzR8gEEALuWB0QzBQS2u2n4z7ZDKKMm6jNSkngutowcl0XkUxH1SIEQ3SPtadfc
/4Twc//zklNJDenj64WEpSL4U++DZPAk6bwMKUte/dUgu5sMVMGSuYzTIuR63zpR
sJYWrHUnvr98Ykm0pBxLcD7HAW8gVkqqhmW2SJJNTn47QXCtz+eZABEBAAH+CQMI
j93fnOLVcQtg+hmBEkRgcgw1zCoCuUt4jvAPq3gFl7evGOSFdz9oCy+8/s7A8xHc
Vs6FRZKgNjbQJX//f4QsPeyLa4Nf/UQYjsyRFy6+DeBnxQgM/dqYOw6alvA4VG71
tYgcO+ze02g9w1vmlCGb/cJvNLWvUIr6RWjbbNAKCLgYmf2GxwBFSQEmdBTXaLRO
pmIJS0K5749ZAI3aZ6EZrzCChtnaZQEJ619Dls0on7DOi+M22146zdq4PjkvZCzm
tua/NL7QTdg3KwooBOx2z6sWtHTGsK8P4zelu9eM+MrVxiojYimx+oFDGqg/lYKr
O6gYeRhmtelWdmNr2ZyYtTfCYE/nxClcUkgl05i0FKpnNvNiE9VxCjioHRHidTwA
W1US6KQHr+XrvGF+XUPCL10+l0bOboliuTppfr8fL4xy2FaurfzInpJaMDB9952i
1BsV+9/suSNG2rpWmXktwVdi8wfbEa558w18hoC6BLmp/ZnBwsCDBBgBCgAPBQJd
HNHyBQkPCZwAAhsuAKgJEMc5z/R+eE2mnSAEGQEKAAYFAl0c0fIACgkQRtcI+foh
K6Cz6QP/fjTA3bpRY9cQlS6DL7KOmLFiRTdPq9JXRDMXfXzxMRSGtV3pMFV5rscF
A6yD5pJlBOsQgJ9yaJ/rOYn2v7pDkE24hzzDLsbOQEnuRH9fJ8Ac3kw0t90GcdJl
mpHAhuubitU0g6NeEtVQ1o4Xe/3CWivsk1Whc//7fIwynyTfZrbwdQQA33zxluKk
RkXJmOFlY9Bum40DQvFGKpXgcygKIn6MEzeUZWk7cgS8dNfJQStyBIF6SvcGEi4d
MfC31FZNU1YGS7xSxf0CjUzy9FaKY3AiUZ0GshOJIkbMXqqGKKivFomQw3HvEODF
AVmAeKKGMiyiUp6Cj3w0sjx72Pv7rM4dHR8=
=dUJo
-----END PGP PRIVATE KEY BLOCK-----";
        const string privateGpgKey1 = @"-----BEGIN PGP PRIVATE KEY BLOCK-----

lQPGBF03IDMBCADGzQ5zfQgOFjZxB5ekIJJD/lgG3VeouChPiJlgY+QDlO/edqd0
T5Ww0yw3gZtunOoIykn5ha8toPLe91Mzz/Dj1aE+U2bC++9uw+Zkacs/HGsZj9SM
EcndKi3U/53LCzD1crAxfmPAtwcKYIBJN39vYfJFahxWq1Rtz7SDNx9gy2jQ3zaN
8c3ePiXaI7mpvcTJW4HlbDzqOXPjguJ0OEI0R1qhLeiRIspJ7z4PZwLmK1SWI34B
LL9+qpjM26EcaBAXRxybqo5awoYQpnCSnZXmn/PDYs4h2w91YUTKaz3iieU9zrAU
XacXvGYJVSD5WB+IQJEAdh8BmzzNTA/s/nZHABEBAAH+BwMCduUdUX+1S1y+2ReY
xd9zYr7M0YWTQd74bM2sjuDxCj7jiwZtb/JXBYZpt/rDI/tZqE041j9vxeJVWKBl
FEfYEGF8He8roinuIr+Perft7xZ6BbtNJlaEMJBY/jSkp5MlZGw/dCkZVsd1xm4d
TKtBOr0HoZjjXNT9NojWiWXkrc18crOkRZnML5wwmNlsy0eGiG8gILbRxXxZZ8k/
/ld/IMaCYZ6s5hN1kU59Ze9RltAygKNuflRJawB59m5tMmZ8CR/3kxbIBWaeo+TO
sl/FxUthWq6XOYICSCpmQZTRUx7ZLh01X4yS6toSvzQIdOiXnwdlvBfA9cFWbAIY
R4xUK5Z2RGOyKaejTCU9rxbPAzwUEmRbrilx4bJzh+JGU/pYqXhvF9cNyb95/ZXE
Vw2wj2auN8PxN+U9pAZbLrD/bM7uqE1hfPl4dS2yss1cCyCUflD4WaFXEifG+dL2
ra/tIa8miHIiF3/tu66xWwl9AqCTy4UJk/ElaAz/MrlJJjR0RNpVq3JP3IEM/xs8
w1kATfsPc5H9NxtAPFvX7J9vbI/h8XyBHDuvwOi6fFfOrN6fYXCA9JH+CDOPLdMH
FVUzm5JIk0eFhvPKAODIEpFYUXy6h80YaZkjmh7mtwoQBkaNSbnJ7s8g/L7gBzN6
6zCj3sptXMGcmxmKilzgabsgr4OUrdDlav8aZJUwm3E/Cip5Ow9kSrVBBvXiVYKg
iFKwnmsQjt+5LT9Qp9VX4zB7+u8mpaTXHmjTVJus29pHprtnj7QeedUKaNXp2x0/
NW/LhNvWnwOiTGRiOwwJ9lT8jcyCJfRf8R26zkI5XFAyVvd4SBkjpCov3AZfy6xw
GOkEThq6ZJnqsP7z/tgQkv5GeTLOhusAuCHbyFSFQBdSpsn0WGsFjJ3IP2Qt4t5u
JPVnAqHLDwMBtCBNYXR0aGV3IEdyYW50IDxlbWFpbDFAZW1haWwuY29tPokBVAQT
AQgAPhYhBIxGNXPMroVpiVSSZsvGoG9kWRUrBQJdNyAzAhsDBQkDwmcABQsJCAcC
BhUKCQgLAgQWAgMBAh4BAheAAAoJEMvGoG9kWRUrk04H/A0u17Dkq600L/qlqMSp
MfppVP+LFhTccUKtLffE1NUH9KpMjZalcsnYWKQHrn/wPCzJBfNKK8i8oZuTbH6z
lZf22wO4UfEymxoRZSYiqvEUifMg6hiCLfHyf7D7c6PuwRhxETyoDvR2ClVeAZ04
4ZgH94Jafgt9olJwv9JKj6gXLafdZhCV+2yCxg+yA+X2rkgviVeBmiBMVkSqHWiJ
L+ud83jCdOdf+kh8Q/Qk0Ss5As0HwIanG/D9fHuU5R6b4teTPekGJcXD4LLpiNUR
f7g+KBVsCkfpn7RFskchspsjQPKc0WzA6o8R7j4CjmJ+iy1YblIlF1QySmcfIUo7
3WqdA8YEXTcgMwEIALyUUYFHjK9OSC/twF5JeWWpfSSQUzzKHoqeoB05DwahTixi
K9BOlLcHsV077HAw+F5ffs3/Dc4/s+Brd7pVAg+qLqNQSmbUZz/60r7cV1HP6T8C
hj4C4oaDEvxH5CiC5yWmObON/z2m0aJ35Vo8RzFPIn9XEj9/WeAA73aieH35jI7M
PtxPO5x+uRsRtQymHxy46RIZtNUdX3+OY9D3P6TJkE2DcLL+jb6hGfHzFs5M2SKF
RIZ3lvMeHnso96ERq4AZhJ8bfusRyJwiGIGyI8YjDjyIq/N90KtwojjTl05MnYsf
ywvcrEDES+UqfzGH8fIU0X8SwYVMsOf4RNNmVPMAEQEAAf4HAwKIHRXMtwZxRr7n
0btEZ3g6X67a41JSNs2ykflPpcI0UvjTzxazyChiZwCNo9OLrsQx7D84Mz7nPMVj
NtXGuZaeQqYwQCHRD8JrvNSMal8r4stgqj7EYg2t9hxK5CrNCDxvgav+ZyWeOgRZ
YvtxCInLLk3y+MQwDMwaBssAVYxhtTzcW68mP5JdCg+8Z51baka3FY4bwBO+kYC4
roBs75/lsK1c3iUSlCFAZmV3OOxnubdigP3WOSU08jQkUbOT5cqaDf1UhQr4PuBe
2/Ck1ctHq1raxLbD1SrNjq0MV3wg7m79UOH7MQLz/nTxHy7LxMojJoag206drZak
QuHDwqZ655SPSEI+i7yVs7MjgdJfJiPpZhrJPMGpbUQjXnJ5nUahjv0CE+jgBtPT
Qb1etrmHDPC8NO6NMWVQdWyiuryYwMu1Xnk5VAkH7lYVjjmzz2Ifmdy0yYsjDJfc
sKDsyC+qocwE7nAobk1O16nL68AlUsVjRbmfPYFKN5ws7yRp3yH0gmBQGUdewbPP
vi+coXnFVY9/Xzu7ZGURzVG0zCvvXnb1JhRVXNTloPPiSt6xKeXN+1riisLED6SX
NyY04Iou1rwjgRKBpTvhsVeIrdyuLTyT1t5tSoLkf1bU0QDDM9MzSjHXJLIZI5z1
ojggaVoMd31v78+MiWjI98yzap2sriTvFZK7ZDFRhnCsMUzkduQ2p+j+GIb9Ocaj
8+q/j+9SpbygjVbQvCk3HA5QsrZ130VXVEhmHUuzxfPLq1sznalBcpd+f9IzYnaZ
pxV56EtfXcMgfcQ1MKiM50ftMS/nv3pjmeiZcSNJfD1+cBMD4YQGpydiZrvTDZlA
TevNqGYnsuOyZ7J1e16KXnHejg3lWzdfDFZ+jrW2zsbWQOFnzm9CCNuZd7945IGr
Iy/HI0BANCKm5HyJATwEGAEIACYWIQSMRjVzzK6FaYlUkmbLxqBvZFkVKwUCXTcg
MwIbDAUJA8JnAAAKCRDLxqBvZFkVK0wHB/4+7nhFbpp5CXxoAwkiouKqPaanNy4x
UR/mG9fT8HJxrrcIs1ObZu0Ydcfp1hpug/vU0ffRBm7piihca+2EOO79DwFRBTaU
JqJrNAIRE4f5qxTbKuLETqtgCW2ar43Xpn1MGauV5Vw5kFbXzM7iMLteOQiJ36CG
jvUtGbM+MTB/UDs8cYgska8COQoc7Nx9suIVHsrfQfyhgEUyBZsVJsI6gyzoLCJQ
cJyMLj7FCCv12UiDvX5ZCExS9fEglOCSum07pMRq6BBS3NlU3yJ1auE5ilw+FklM
lmWT1tazjG0b6vS+b5ATanDKio8awJD9XI2Z9rry8ZfZCrClR5dg33jTlQPFBF04
JrsBCADlKpuwPHEEUS7G8RVrCt6d6gARyu3RKZpi6GpZzvuakVnMDafu8kDGFgAH
NFBJlSDoGeFPdrN3GfFDhRt6vAlgWOm+J2wnFI6qm7Oyr+ZnHvkQZXpnnHyxW0jn
vXsDveK0yTUtdKTZ7DlkwMTRQRP8sYqZJ8b0cxrekyFy6XodzapU3OVlQELojF41
PZUth77LqosQz0aHPiw6RQEJXymKVKQH1SkKIsuzx/2CuccUHql0+X4e+/GWYft3
SLJ4aSiIVd/ta/rHv7QW+TYiKFJJPBSlp2Aw48TIqDZ4b7iE0B0w/dCTxnZOz69t
G2HxPqXDgO0OTGqXrJAALYXVxxBFABEBAAH+BwMCt1d6O+as1kO+HUWPiUKJ8Sx1
bFBPe446IScmmhI/VDsSQhuGgqzAjxLLZlnfDt/5FgNbA5HPt0Z1TrH24fuUrbzO
nE5wKoHHRl77hpCEoANgbhEnbIOCFp3cR1ZEu3BYyEMg5I2Ki4ojDvGWTQHYoiHP
Hq4oSaMSRYToW+8n8Je7SMOpuNOy5hfn5zfqOx+RYpiU2bXOBiwcEchtHdZOUQrY
KHqTjg+RuPc25SYGM+znXpx38Zn9VYTUFDx1qBZRJSKP1jvJZSfiK/AcZ2oXloL3
n8eGgRcaCmqpDM6mKPoIESbimcZAeo/5PQHaS2Y/WpEWE9i2u5Hh8teFq0dVeGGi
dUmu3h9Wgp7WmgDuROk1NVgPUx5S2vMTjaZmNFWMjzyB8KyIWuZl59xp6C6Ktx9r
Rpi1Z8RGiLQM52+qnVV7zLB0XFnQaSvR3u29xgcpRU2yHPrdapbjAoxMEPy5jEeR
wbYjClMzxhkuYdz4DBSfFiA7C4BTmSXSiZ3dSvi/sdhMVGV45fx+CgiR3cpTeI5F
nq7toovWZdfgyo/XGgAQDRP4evhgproQl06KtbqQbAFU7L5q/E/WP+3LpeHGpIwb
MGfWiDlN1v4H6CmCe7tz8WjeczmPFtRYRyyaS4K2BGL//29Mk4uljCbaZyKZ6W5T
deL67Ay0+nugPGIbyb0MsKzkDC0v7pCxe8CYyQq9tGoStYiPx5S2mdslDghK0LqF
phtQEgbh9lt4Va9+78Vv/fWIg4uK3g0s4xkiL+mERF+VXMWSxUAqMUZQXciNlpDe
tlYH6nDXGdDyEXkX24V11QU80XfluS5CDUiPJCjuYCp2SmgIvRlS66rf0I2oT1kU
LC4ZGupK/PE9A5kUGI9NUjhX6KL0TbRM/ARL86GU8c/SXvGu4oR0vfhcB0hjrOGO
bu+0Hm1hdHRvc2F1cnVzIDxlbWFpbDFAZW1haWwuY29tPokBVAQTAQgAPhYhBGN9
68UQ+bUQSyhMwudUUHi0vV/QBQJdOCa7AhsDBQkDwmcABQsJCAcCBhUKCQgLAgQW
AgMBAh4BAheAAAoJEOdUUHi0vV/QGcoIALQ8q92Th6BttxLaAUx8gvvpShz0KKT3
lYCN8t0SOXaIGZxiUeatZxI9W+WcXgS+88ooRxwQgCs7ZDABNYXF4fjvHVV1tWMr
+9h5FhJRxpyDuw9e0agUaP3zs0s4tgKeALt0VtscFjtHmSf+clhkJBxmEQNCsiFN
V/eCTr1/xoHGCPeqet4ArUSfChyIQQp2EKgXUI84TwBqzt3P9Zt7QOgaVp1FlRIA
TuHRSS6tFjxCzmNdU7YpwGMyl87oHHuRxFbGEZxO8Ej1JbX35h29NomlWyXa90qg
EdvKpmosL4Xo9Vdy7F+dp8BkyzkHgM1jVJeMu78E9EqOjYvPepGsknydA8YEXTgm
uwEIAL94MSfsw1EuRsnsr15T5hMhnn5JaSrmzOkey24Pu0/VDzgWvBNyuJHIYSIJ
O5B9WZ/7g9TfHuQ7c6Cv9mwk76v2dHO2wHg8c6jf2YFEme9PO54Zqk+u080y/OU4
iU4ns1JeSmEiWJrfaz1nS/XgTUzlVfZCS1uPJzAaia+KwMlA7yUlmkEt8h6S44eQ
yoNNilU2jbgoccO8TC+MUVRFlLxyXf9vPntbY81A1BHAKfLo/bzOqqIw1qrphF2H
XTotEN1CeZdxFqRo5ELONtmkimSO8B/1RB4S0DV8oCaEdLQ+rhV+mqtlgHavWdjR
ZNtc7kMAqbeLH/fFGdgy7fjKwXUAEQEAAf4HAwLpSWI1HEGjk75R8bJ7PkUOSNHS
ZSuIR6dtZ6tbfuT9om1Y7qE1Bdom4stcezNUkKHlxaTIWD7EhcsPaU2w5+uWcZr3
8ElYrZSf1NmoO6VdhzrIEamAkl9XJxIUIMeaTmVDaCI1jACblQv8LQvAL6t9ZFqQ
YMG/Lw3JlU5qknOgeeYDDZih7mSbscifmnD04KmYrqfr5LCBV3xTm+XEkVddHrsl
yvTCzSaXbn6+kLmnZxra6ZdpCU5LLG5f3MUj9Pqk0Vf3w8vxzZZMcHNI2E7Nt2Rl
Dtkd/1D++8/1/trN8MCjapom52UyrofDk/zW5E/LZqhfob7grl4ReNzZyo3RYBK1
3SSiqoPEHSnwymo3VjbtMlGkqdHSYTr3/xr4fAEm20ubwKDtvK98t1rDlNm7XlNC
vY+3fhp5BQee9fxYu9pwYkyPHpqSAjOHnAwfiFK8CWRoc6XgLEO29zNhXpajbEZu
hSDt003ngyPCJ+3ZnMawFs3lT1DlQWZhdxjISHeGSAzxD/eG0fIk3R3e+0xQQ0M+
IPnEpAzqTJc21BnqeThRJ1tZsqpHfl8xtS74qRg9xmOc9YC3WA98VGYreExvNm4T
6Gp71kOhHRsOD+RVoWDdilXcIl6cDsR5m7qZZj85J7WdJu8sNN8y8Y5wekyBO5x/
vLuUrWZjLGKZN2CxZBDxT0i1ZNitSLwVMze80gjkRgHnQXnZCNY4ZvWrOeFAqzwS
vGtD4Ppvts9mZJAV+6+xRU+k7+d1aRaSynzES1II7OINxPK7zcU88FTRyU98kZA0
4bnOEa8OHFpPjB28M5Yf3QMLk8G02TEXKN2XMMODvZesLZDGYjxaGzhVc2bG1Rhx
TZacrT2bvaqAJRu2CNGz3eDo14IyMkRaZFOo1EgGyadUB8p1W11B8Cbu72uWNdca
AVSJATwEGAEIACYWIQRjfevFEPm1EEsoTMLnVFB4tL1f0AUCXTgmuwIbDAUJA8Jn
AAAKCRDnVFB4tL1f0KUPCADb/2qNKux8ml8j8xFH2djPDgXBMc7TQUPAzEUqjiwW
e3PxnZqqsrxX9nfV8QPWOZrDRkeyuQmz8nRukvjIxcRU30Q1+vp3981l20jiqGa+
FBaHRCSkmo0VjIij3NWFYjpeenVqWwWvR7x7RZIM2LDp2xWSmUixe49L30rWFIR1
CFjx8G2S/oCi1nmsCLvPgSGnqF6Nt+W/uX4jduVPXnTh1wRaITiJV99mnBL70xK4
vglsq+OUZc0wxqBHVl1iTSElaJf42e643S4mQ6USHvARtovINV3DdgyXUnZKLnDy
hJ7zi7r7TgM1dbW9YZ+rAwrlpBNcYbJyLY8cNjuppswz
=J21a
-----END PGP PRIVATE KEY BLOCK-----";
        const string privateGpgKey2 = @"-----BEGIN PGP PRIVATE KEY BLOCK-----

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
=tfnh
-----END PGP PRIVATE KEY BLOCK-----";
    }
}
