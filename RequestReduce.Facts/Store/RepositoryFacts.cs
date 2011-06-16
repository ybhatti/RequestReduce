﻿using System;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using RequestReduce.Configuration;
using RequestReduce.Store;
using Xunit;

namespace RequestReduce.Facts.Store
{
    public class RepositoryFacts
    {

        class FakeFileRepository : FileRepository
        {
            public FakeFileRepository(IRRConfiguration config)
                : base(config)
            {
                Database.SetInitializer<RequestReduceContext>(new DropCreateDatabaseAlways<RequestReduceContext>());
                Context.Database.Initialize(true);
            }
        }

        class TestableRepository : Testable<FakeFileRepository>
        {
            public TestableRepository()
            {
                Mock<IRRConfiguration>().Setup(x => x.ConnectionStringName).Returns("RRConnection");
            }
        }

        public class Save
        {
            [Fact]
            public void WillSaveToDatabase()
            {
                var testable = new TestableRepository();
                var id = Guid.NewGuid();
                var file = new RequestReduceFile()
                               {
                                   Content = new byte[] {1},
                                   FileName = "fileName",
                                   Key = Guid.NewGuid(),
                                   LastAccessed = DateTime.Now,
                                   LastUpdated = DateTime.Now,
                                   OriginalName = "originalName",
                                   RequestReduceFileId = id
                               };

                testable.ClassUnderTest.Save(file);

                var savedFile = testable.ClassUnderTest[id];
                Assert.Equal(file.Content.Length, savedFile.Content.Length);
                Assert.Equal(file.Content[0], savedFile.Content[0]);
                Assert.Equal(file.FileName, savedFile.FileName);
                Assert.Equal(file.Key, savedFile.Key);
                Assert.Equal(file.OriginalName, savedFile.OriginalName);
                Assert.Equal(file.RequestReduceFileId, savedFile.RequestReduceFileId);
                Assert.Equal(file.LastAccessed, savedFile.LastAccessed);
                Assert.Equal(file.LastUpdated, savedFile.LastUpdated);
            }

            [Fact]
            public void WillUpdateContentAndLastUpdatedTime()
            {
                var testable = new TestableRepository();
                var id = Guid.NewGuid();
                var file = new RequestReduceFile()
                {
                    Content = new byte[] { 1 },
                    FileName = "fileName",
                    Key = Guid.NewGuid(),
                    LastAccessed = DateTime.Now,
                    LastUpdated = new DateTime(2010, 1, 1),
                    OriginalName = "originalName",
                    RequestReduceFileId = id
                };
                testable.ClassUnderTest.Save(file);
                var file2 = new RequestReduceFile()
                {
                    Content = new byte[] { 2 },
                    FileName = "fileName",
                    Key = Guid.NewGuid(),
                    LastAccessed = DateTime.Now,
                    LastUpdated = new DateTime(2011, 1, 1),
                    OriginalName = "originalName",
                    RequestReduceFileId = id
                };

                testable.ClassUnderTest.Save(file2);

                var savedFile = testable.ClassUnderTest[id];
                Assert.Equal(2, savedFile.Content[0]);
                Assert.Equal(new DateTime(2011, 1, 1), savedFile.LastUpdated);
            }

        }

        [Fact]
        public void WillReturnDistinctListOfKeys()
        {
            var testable = new TestableRepository();
            var id = Guid.NewGuid();
            var id2 = Guid.NewGuid();
            var file = new RequestReduceFile()
            {
                Content = new byte[] { 1 },
                FileName = RequestReduce.Utilities.UriBuilder.CssFileName,
                Key = id,
                LastAccessed = DateTime.Now,
                LastUpdated = DateTime.Now,
                OriginalName = "originalName",
                RequestReduceFileId = Guid.NewGuid()
            };
            var file2 = new RequestReduceFile()
            {
                Content = new byte[] { 1 },
                FileName = RequestReduce.Utilities.UriBuilder.CssFileName,
                Key = id,
                LastAccessed = DateTime.Now,
                LastUpdated = DateTime.Now,
                RequestReduceFileId = Guid.NewGuid()
            };
            var file3 = new RequestReduceFile()
            {
                Content = new byte[] { 1 },
                FileName = RequestReduce.Utilities.UriBuilder.CssFileName,
                Key = id2,
                LastAccessed = DateTime.Now,
                LastUpdated = DateTime.Now,
                OriginalName = "originalName2",
                RequestReduceFileId = Guid.NewGuid()
            };
            testable.ClassUnderTest.Save(file);
            testable.ClassUnderTest.Save(file2);
            testable.ClassUnderTest.Save(file3);

            var result = testable.ClassUnderTest.GetKeys();

            Assert.Equal(2, result.Count());
            Assert.True(result.Contains(id));
            Assert.True(result.Contains(id2));
        }
    }
}