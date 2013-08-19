// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Data;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Microsoft.TestCommon;
using Moq;
using WebMatrix.Data;

namespace WebMatrix.WebData.Test
{
    public class SimpleMembershipProviderTest
    {
        [Fact]
        public void ConfirmAccountReturnsFalseIfNoRecordExistsForToken()
        {
            // Arrange
            var database = new Mock<MockDatabase>(MockBehavior.Strict);
            database.Setup(d => d.Query("SELECT [UserId], [ConfirmationToken] FROM aspnet_Membership WHERE [ConfirmationToken] = @0", "foo"))
                .Returns(Enumerable.Empty<DynamicRecord>());
            var simpleMembershipProvider = new TestSimpleMembershipProvider(database.Object);

            // Act
            bool result = simpleMembershipProvider.ConfirmAccount("foo");

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void ConfirmAccountReturnsFalseIfConfirmationTokenDoesNotMatchInCase()
        {
            // Arrange
            var database = new Mock<MockDatabase>(MockBehavior.Strict);
            DynamicRecord record = GetRecord(new Guid("{7646C96B-6D04-42BC-8349-260DB5AB5214}"), "Foo");
            database.Setup(d => d.Query("SELECT [UserId], [ConfirmationToken] FROM aspnet_Membership WHERE [ConfirmationToken] = @0", "foo"))
                .Returns(new[] { record });
            var simpleMembershipProvider = new TestSimpleMembershipProvider(database.Object);

            // Act
            bool result = simpleMembershipProvider.ConfirmAccount("foo");

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void ConfirmAccountReturnsFalseIfNoConfirmationTokenFromMultipleListMatchesInCase()
        {
            // Arrange
            var database = new Mock<MockDatabase>(MockBehavior.Strict);
            DynamicRecord recordA = GetRecord(new Guid("{7646C96B-6D04-42BC-8349-260DB5AB5214}"), "Foo");
            DynamicRecord recordB = GetRecord(new Guid("{9D0F4BB9-9B4A-41FF-BA25-EC4DEFE16506}"), "fOo");
            database.Setup(d => d.Query("SELECT [UserId], [ConfirmationToken] FROM aspnet_Membership WHERE [ConfirmationToken] = @0", "foo"))
                .Returns(new[] { recordA, recordB });
            var simpleMembershipProvider = new TestSimpleMembershipProvider(database.Object);

            // Act
            bool result = simpleMembershipProvider.ConfirmAccount("foo");

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void ConfirmAccountUpdatesIsConfirmedFieldIfConfirmationTokenMatches()
        {
            // Arrange
            var database = new Mock<MockDatabase>(MockBehavior.Strict);
            DynamicRecord record = GetRecord(new Guid("{171E57BC-3D39-4045-807A-3B71C7F1532E}"), "foo");
            database.Setup(d => d.Query("SELECT [UserId], [ConfirmationToken] FROM aspnet_Membership WHERE [ConfirmationToken] = @0", "foo"))
                .Returns(new[] { record }).Verifiable();
            database.Setup(d => d.Execute("UPDATE aspnet_Membership SET [IsConfirmed] = 1 WHERE [UserId] = @0", new Guid("{171E57BC-3D39-4045-807A-3B71C7F1532E}"))).Returns(1).Verifiable();
            var simpleMembershipProvider = new TestSimpleMembershipProvider(database.Object);

            // Act
            bool result = simpleMembershipProvider.ConfirmAccount("foo");

            // Assert
            Assert.True(result);
            database.Verify();
        }

        [Fact]
        public void ConfirmAccountUpdatesIsConfirmedFieldIfAnyOneOfReturnRecordConfirmationTokenMatches()
        {
            // Arrange
            var database = new Mock<MockDatabase>(MockBehavior.Strict);
            DynamicRecord recordA = GetRecord(new Guid("{171E57BC-3D39-4045-807A-3B71C7F1532E}"), "Foo");
            DynamicRecord recordB = GetRecord(new Guid("{417AD677-61B5-41EE-9D3D-E5F4AE954816}"), "foo");
            DynamicRecord recordC = GetRecord(new Guid("{BA1999B9-A301-4FB8-8D2D-B0BC58799697}"), "fOo");
            database.Setup(d => d.Query("SELECT [UserId], [ConfirmationToken] FROM aspnet_Membership WHERE [ConfirmationToken] = @0", "foo"))
                .Returns(new[] { recordA, recordB, recordC }).Verifiable();
            database.Setup(d => d.Execute("UPDATE aspnet_Membership SET [IsConfirmed] = 1 WHERE [UserId] = @0", new Guid("{417AD677-61B5-41EE-9D3D-E5F4AE954816}"))).Returns(1).Verifiable();
            var simpleMembershipProvider = new TestSimpleMembershipProvider(database.Object);

            // Act
            bool result = simpleMembershipProvider.ConfirmAccount("foo");

            // Assert
            Assert.True(result);
            database.Verify();
        }

        [Fact]
        public void ConfirmAccountWithUserNameReturnsFalseIfNoRecordExistsForToken()
        {
            // Arrange
            var database = new Mock<MockDatabase>(MockBehavior.Strict);
            database.Setup(d => d.QuerySingle("SELECT m.[UserId], m.[ConfirmationToken] FROM aspnet_Membership m JOIN [Users] u ON m.[UserId] = u.[UserId] WHERE m.[ConfirmationToken] = @0 AND u.[UserName] = @1", "foo", "user12")).Returns(null);
            var simpleMembershipProvider = new TestSimpleMembershipProvider(database.Object) { UserIdColumn = "UserId", UserNameColumn = "UserName", UserTableName = "Users" };

            // Act
            bool result = simpleMembershipProvider.ConfirmAccount("user12", "foo");

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void ConfirmAccountWithUserNameReturnsFalseIfConfirmationTokenDoesNotMatchInCase()
        {
            // Arrange
            var database = new Mock<MockDatabase>(MockBehavior.Strict);
            DynamicRecord record = GetRecord(new Guid("{7646C96B-6D04-42BC-8349-260DB5AB5214}"), "Foo");
            database.Setup(d => d.QuerySingle("SELECT m.[UserId], m.[ConfirmationToken] FROM aspnet_Membership m JOIN [Users_bkp2_1] u ON m.[UserId] = u.[wishlist_site_real_user_id] WHERE m.[ConfirmationToken] = @0 AND u.[wishlist_site_real_user_name] = @1", "foo", "user13")).Returns(record);
            var simpleMembershipProvider = new TestSimpleMembershipProvider(database.Object) { UserIdColumn = "wishlist_site_real_user_id", UserNameColumn = "wishlist_site_real_user_name", UserTableName = "Users_bkp2_1" };

            // Act
            bool result = simpleMembershipProvider.ConfirmAccount("user13", "foo");

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void ConfirmAccountWithUserNameUpdatesIsConfirmedFieldIfConfirmationTokenMatches()
        {
            // Arrange
            var database = new Mock<MockDatabase>(MockBehavior.Strict);
            DynamicRecord record = GetRecord(new Guid("{171E57BC-3D39-4045-807A-3B71C7F1532E}"), "foo");
            database.Setup(d => d.QuerySingle("SELECT m.[UserId], m.[ConfirmationToken] FROM aspnet_Membership m JOIN [Users] u ON m.[UserId] = u.[Id] WHERE m.[ConfirmationToken] = @0 AND u.[UserName] = @1", "foo", "user14"))
                .Returns(record).Verifiable();
            database.Setup(d => d.Execute("UPDATE aspnet_Membership SET [IsConfirmed] = 1 WHERE [UserId] = @0", new Guid("{171E57BC-3D39-4045-807A-3B71C7F1532E}"))).Returns(1).Verifiable();
            var simpleMembershipProvider = new TestSimpleMembershipProvider(database.Object) { UserTableName = "Users", UserIdColumn = "Id", UserNameColumn = "UserName" };

            // Act
            bool result = simpleMembershipProvider.ConfirmAccount("user14", "foo");

            // Assert
            Assert.True(result);
            database.Verify();
        }

        [Fact]
        public void GenerateTokenHtmlEncodesValues()
        {
            // Arrange
            var generator = new Mock<RandomNumberGenerator>(MockBehavior.Strict);
            var generatedBytes = Encoding.Default.GetBytes("|aÿx§#½oÿ↨îA8Eµ");
            generator.Setup(g => g.GetBytes(It.IsAny<byte[]>())).Callback((byte[] array) => Array.Copy(generatedBytes, array, generatedBytes.Length));

            // Act
            var result = SimpleMembershipProvider.GenerateToken(generator.Object);

            // Assert
            Assert.Equal("fGH/eKcjvW//P+5BOEW1", Convert.ToBase64String(generatedBytes));
            Assert.Equal("fGH_eKcjvW__P-5BOEW1AA2", result);
        }

        private static DynamicRecord GetRecord(Guid userId, string confirmationToken)
        {
            var data = new Mock<IDataRecord>(MockBehavior.Strict);
            data.Setup(c => c[0]).Returns(userId);
            data.Setup(c => c[1]).Returns(confirmationToken);
            return new DynamicRecord(new[] { "UserId", "ConfirmationToken" }, data.Object);
        }

        private class TestSimpleMembershipProvider : SimpleMembershipProvider
        {
            private readonly IDatabase _database;

            public TestSimpleMembershipProvider(IDatabase database)
            {
                _database = database;
            }

            internal override IDatabase ConnectToDatabase()
            {
                return _database;
            }

            internal override void VerifyInitialized()
            {
                // Do nothing.
            }
        }
    }
}
