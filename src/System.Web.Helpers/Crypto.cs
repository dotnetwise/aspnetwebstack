// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Web.Helpers.Resources;

namespace System.Web.Helpers
{
    public class PasswordCrypto
    {
        public static PasswordCrypto Instance = new PasswordCrypto();
        protected const int SaltSize = 128 / 8; // 128 bits
        public virtual string HashPassword(string password)
        {
            return Crypto.HashPassword(password);
        }
        public virtual bool VerifyHashedPassword(string userName, string hashedPassword, string password)
        {
            return Crypto.VerifyHashedPassword(hashedPassword, password);
        }
        public virtual string GenerateSalt(int bytesLength = SaltSize)
        {
            return Crypto.GenerateSalt(bytesLength);
        }
        public virtual string Hash(string input, string algorithm = "sha256")
        {
            return Crypto.Hash(input, algorithm);
        }

        public virtual string Hash(byte[] input, string algorithm = "sha256")
        {
            return Crypto.Hash(input, algorithm);
        }

        public virtual string SHA1(string input)
        {
            return Crypto.SHA1(input);
        }

        public virtual string SHA256(string input)
        {
            return Crypto.SHA256(input);
        }
    }
}
