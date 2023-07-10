﻿// Copyright (c) Microsoft. All rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.SemanticKernel.AI.Embeddings;
using Microsoft.SemanticKernel.Connectors.Memory.Postgres;
using Microsoft.SemanticKernel.Memory;
using Moq;
using Xunit;

namespace SemanticKernel.Connectors.UnitTests.Memory.Postgres;

/// <summary>
/// Unit tests for <see cref="PostgresMemoryStore"/> class.
/// </summary>
public class PostgresMemoryStoreTests
{
    private const string CollectionName = "fake-collection-name";

    private readonly Mock<IPostgresDbClient> _postgresDbClientMock;

    public PostgresMemoryStoreTests()
    {
        this._postgresDbClientMock = new Mock<IPostgresDbClient>();
        this._postgresDbClientMock
            .Setup(client => client.DoesTableExistsAsync(CollectionName, CancellationToken.None))
            .ReturnsAsync(true);
    }

    [Fact]
    public async Task ItCanCreateCollectionAsync()
    {
        // Arrange
        var store = new PostgresMemoryStore(this._postgresDbClientMock.Object);

        // Act
        await store.CreateCollectionAsync(CollectionName);

        // Assert
        this._postgresDbClientMock.Verify(client => client.CreateTableAsync(CollectionName, CancellationToken.None), Times.Once());
    }

    [Fact]
    public async Task ItCanDeleteCollectionAsync()
    {
        // Arrange
        var store = new PostgresMemoryStore(this._postgresDbClientMock.Object);

        // Act
        await store.DeleteCollectionAsync(CollectionName);

        // Assert
        this._postgresDbClientMock.Verify(client => client.DeleteTableAsync(CollectionName, CancellationToken.None), Times.Once());
    }

    [Fact]
    public async Task ItReturnsTrueWhenCollectionExistsAsync()
    {
        // Arrange
        var store = new PostgresMemoryStore(this._postgresDbClientMock.Object);

        // Act
        var doesCollectionExist = await store.DoesCollectionExistAsync(CollectionName);

        // Assert
        Assert.True(doesCollectionExist);
    }

    [Fact]
    public async Task ItReturnsFalseWhenCollectionDoesNotExistAsync()
    {
        // Arrange
        const string collectionName = "non-existent-collection";

        this._postgresDbClientMock
            .Setup(client => client.DoesTableExistsAsync(collectionName, CancellationToken.None))
            .ReturnsAsync(false);

        var store = new PostgresMemoryStore(this._postgresDbClientMock.Object);

        // Act
        var doesCollectionExist = await store.DoesCollectionExistAsync(collectionName);

        // Assert
        Assert.False(doesCollectionExist);
    }

    [Fact]
    public async Task ItCanUpsertAsync()
    {
        // Arrange
        var expectedMemoryRecord = this.GetRandomMemoryRecord();
        var postgresMemoryEntry = this.GetPostgresMemoryEntryFromMemoryRecord(expectedMemoryRecord)!;

        var store = new PostgresMemoryStore(this._postgresDbClientMock.Object);

        // Act
        var actualMemoryRecordKey = await store.UpsertAsync(CollectionName, expectedMemoryRecord);

        // Assert
        this._postgresDbClientMock.Verify(client => client.UpsertAsync(CollectionName, postgresMemoryEntry.Key, postgresMemoryEntry.MetadataString, It.Is<Pgvector.Vector>(x => x.ToArray().SequenceEqual(postgresMemoryEntry.Embedding!.ToArray())), postgresMemoryEntry.Timestamp, CancellationToken.None), Times.Once());
        Assert.Equal(expectedMemoryRecord.Key, actualMemoryRecordKey);
    }

    [Fact]
    public async Task ItCanUpsertBatchAsyncAsync()
    {
        // Arrange
        var memoryRecord1 = this.GetRandomMemoryRecord();
        var memoryRecord2 = this.GetRandomMemoryRecord();
        var memoryRecord3 = this.GetRandomMemoryRecord();

        var batchUpsertMemoryRecords = new[] { memoryRecord1, memoryRecord2, memoryRecord3 };
        var expectedMemoryRecordKeys = batchUpsertMemoryRecords.Select(l => l.Key).ToList();

        var store = new PostgresMemoryStore(this._postgresDbClientMock.Object);

        // Act
        var actualMemoryRecordKeys = await store.UpsertBatchAsync(CollectionName, batchUpsertMemoryRecords).ToListAsync();

        // Assert
        foreach (var memoryRecord in batchUpsertMemoryRecords)
        {
            var postgresMemoryEntry = this.GetPostgresMemoryEntryFromMemoryRecord(memoryRecord)!;
            this._postgresDbClientMock.Verify(client => client.UpsertAsync(CollectionName, postgresMemoryEntry.Key, postgresMemoryEntry.MetadataString, It.Is<Pgvector.Vector>(x => x.ToArray().SequenceEqual(postgresMemoryEntry.Embedding!.ToArray())), postgresMemoryEntry.Timestamp, CancellationToken.None), Times.Once());
        }

        for (int i = 0; i < expectedMemoryRecordKeys.Count; i++)
        {
            Assert.Equal(expectedMemoryRecordKeys[i], actualMemoryRecordKeys[i]);
        }
    }

    [Fact]
    public async Task ItCanGetMemoryRecordFromCollectionAsync()
    {
        // Arrange
        var expectedMemoryRecord = this.GetRandomMemoryRecord();
        var postgresMemoryEntry = this.GetPostgresMemoryEntryFromMemoryRecord(expectedMemoryRecord);

        this._postgresDbClientMock
            .Setup(client => client.ReadAsync(CollectionName, expectedMemoryRecord.Key, true, CancellationToken.None))
            .ReturnsAsync(postgresMemoryEntry);

        var store = new PostgresMemoryStore(this._postgresDbClientMock.Object);

        // Act
        var actualMemoryRecord = await store.GetAsync(CollectionName, expectedMemoryRecord.Key, withEmbedding: true);

        // Assert
        Assert.NotNull(actualMemoryRecord);
        this.AssertMemoryRecordEqual(expectedMemoryRecord, actualMemoryRecord);
    }

    [Fact]
    public async Task ItReturnsNullWhenMemoryRecordDoesNotExistAsync()
    {
        // Arrange
        const string memoryRecordKey = "fake-record-key";

        this._postgresDbClientMock
            .Setup(client => client.ReadAsync(CollectionName, memoryRecordKey, true, CancellationToken.None))
            .ReturnsAsync((PostgresMemoryEntry?)null);

        var store = new PostgresMemoryStore(this._postgresDbClientMock.Object);

        // Act
        var actualMemoryRecord = await store.GetAsync(CollectionName, memoryRecordKey, withEmbedding: true);

        // Assert
        Assert.Null(actualMemoryRecord);
    }

    [Fact]
    public async Task ItCanGetMemoryRecordBatchFromCollectionAsync()
    {
        // Arrange
        var memoryRecord1 = this.GetRandomMemoryRecord();
        var memoryRecord2 = this.GetRandomMemoryRecord();
        var memoryRecord3 = this.GetRandomMemoryRecord();

        var expectedMemoryRecords = new[] { memoryRecord1, memoryRecord2, memoryRecord3 };
        var memoryRecordKeys = expectedMemoryRecords.Select(l => l.Key).ToList();

        foreach (var memoryRecord in expectedMemoryRecords)
        {
            this._postgresDbClientMock
                .Setup(client => client.ReadAsync(CollectionName, memoryRecord.Key, true, CancellationToken.None))
                .ReturnsAsync(this.GetPostgresMemoryEntryFromMemoryRecord(memoryRecord));
        }

        memoryRecordKeys.Insert(0, "non-existent-record-key-1");
        memoryRecordKeys.Insert(2, "non-existent-record-key-2");
        memoryRecordKeys.Add("non-existent-record-key-3");

        this._postgresDbClientMock
                .Setup(client => client.ReadBatchAsync(CollectionName, memoryRecordKeys, true, CancellationToken.None))
                .Returns(expectedMemoryRecords.Select(memoryRecord => this.GetPostgresMemoryEntryFromMemoryRecord(memoryRecord)).ToAsyncEnumerable());

        var store = new PostgresMemoryStore(this._postgresDbClientMock.Object);

        // Act
        var actualMemoryRecords = await store.GetBatchAsync(CollectionName, memoryRecordKeys, withEmbeddings: true).ToListAsync();

        // Assert
        this._postgresDbClientMock.Verify(client => client.ReadBatchAsync(CollectionName, memoryRecordKeys, true, CancellationToken.None), Times.Once());
        Assert.Equal(expectedMemoryRecords.Length, actualMemoryRecords.Count);

        for (var i = 0; i < expectedMemoryRecords.Length; i++)
        {
            this.AssertMemoryRecordEqual(expectedMemoryRecords[i], actualMemoryRecords[i]);
        }
    }

    [Fact]
    public async Task ItCanReturnCollectionsAsync()
    {
        // Arrange
        var expectedCollections = new List<string> { "fake-collection-1", "fake-collection-2", "fake-collection-3" };

        this._postgresDbClientMock
            .Setup(client => client.GetTablesAsync(CancellationToken.None))
            .Returns(expectedCollections.ToAsyncEnumerable());

        var store = new PostgresMemoryStore(this._postgresDbClientMock.Object);

        // Act
        var actualCollections = await store.GetCollectionsAsync().ToListAsync();

        // Assert
        Assert.Equal(expectedCollections.Count, actualCollections.Count);

        for (var i = 0; i < expectedCollections.Count; i++)
        {
            Assert.Equal(expectedCollections[i], actualCollections[i]);
        }
    }

    [Fact]
    public async Task ItCanRemoveAsync()
    {
        // Arrange
        const string memoryRecordKey = "fake-record-key";
        var store = new PostgresMemoryStore(this._postgresDbClientMock.Object);

        // Act
        await store.RemoveAsync(CollectionName, memoryRecordKey);

        // Assert
        this._postgresDbClientMock.Verify(client => client.DeleteAsync(CollectionName, memoryRecordKey, CancellationToken.None), Times.Once());
    }

    [Fact]
    public async Task ItCanRemoveBatchAsync()
    {
        // Arrange
        string[] memoryRecordKeys = new string[] { "fake-record-key1", "fake-record-key2", "fake-record-key3" };
        var store = new PostgresMemoryStore(this._postgresDbClientMock.Object);

        // Act
        await store.RemoveBatchAsync(CollectionName, memoryRecordKeys);

        // Assert
        this._postgresDbClientMock.Verify(client => client.DeleteBatchAsync(CollectionName, memoryRecordKeys, CancellationToken.None), Times.Once());
    }

    #region private ================================================================================

    private void AssertMemoryRecordEqual(MemoryRecord expectedRecord, MemoryRecord actualRecord)
    {
        Assert.Equal(expectedRecord.Key, actualRecord.Key);
        Assert.Equal(expectedRecord.Embedding.Vector, actualRecord.Embedding.Vector);
        Assert.Equal(expectedRecord.Metadata.Id, actualRecord.Metadata.Id);
        Assert.Equal(expectedRecord.Metadata.Text, actualRecord.Metadata.Text);
        Assert.Equal(expectedRecord.Metadata.Description, actualRecord.Metadata.Description);
        Assert.Equal(expectedRecord.Metadata.AdditionalMetadata, actualRecord.Metadata.AdditionalMetadata);
        Assert.Equal(expectedRecord.Metadata.IsReference, actualRecord.Metadata.IsReference);
        Assert.Equal(expectedRecord.Metadata.ExternalSourceName, actualRecord.Metadata.ExternalSourceName);
    }

    private MemoryRecord GetRandomMemoryRecord(Embedding<float>? embedding = null)
    {
        var id = Guid.NewGuid().ToString();
        var memoryEmbedding = embedding ?? new Embedding<float>(new[] { 1f, 3f, 5f });

        return MemoryRecord.LocalRecord(
            id: id,
            text: "text-" + Guid.NewGuid().ToString(),
            description: "description-" + Guid.NewGuid().ToString(),
            embedding: memoryEmbedding,
            additionalMetadata: "metadata-" + Guid.NewGuid().ToString(),
            key: id,
            timestamp: DateTimeOffset.Now);
    }

    private PostgresMemoryEntry GetPostgresMemoryEntryFromMemoryRecord(MemoryRecord memoryRecord)
    {
        return new PostgresMemoryEntry()
        {
            Key = memoryRecord.Key,
            Embedding = new Pgvector.Vector(memoryRecord.Embedding.Vector.ToArray()),
            MetadataString = memoryRecord.GetSerializedMetadata(),
            Timestamp = memoryRecord.Timestamp?.UtcDateTime
        };
    }

    #endregion
}
