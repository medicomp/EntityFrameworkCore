// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using System.Transactions;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.InMemory.Internal;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xunit;

// ReSharper disable UnusedAutoPropertyAccessor.Local
// ReSharper disable UnusedMember.Local
// ReSharper disable InconsistentNaming
namespace Microsoft.EntityFrameworkCore.Query
{
    public class WarningsTest
    {
        [Fact]
        public void Should_throw_by_default_when_transaction()
        {
            var optionsBuilder
                = new DbContextOptionsBuilder()
                    .EnableServiceProviderCaching(false)
                    .UseInMemoryDatabase(Guid.NewGuid().ToString());

            using (var context = new DbContext(optionsBuilder.Options))
            {
                Assert.Equal(
                    CoreStrings.WarningAsErrorTemplate(
                        InMemoryEventId.TransactionIgnoredWarning,
                        InMemoryStrings.LogTransactionsNotSupported.GenerateMessage(),
                        "InMemoryEventId.TransactionIgnoredWarning"),
                    Assert.Throws<InvalidOperationException>(
                        () => context.Database.BeginTransaction()).Message);
            }
        }

        [Fact]
        public void Should_throw_by_default_when_transaction_enlisted()
        {
            var optionsBuilder
                = new DbContextOptionsBuilder()
                    .EnableServiceProviderCaching(false)
                    .UseInMemoryDatabase(Guid.NewGuid().ToString());

            using (var context = new DbContext(optionsBuilder.Options))
            {
                Assert.Equal(
                    CoreStrings.WarningAsErrorTemplate(
                        InMemoryEventId.TransactionIgnoredWarning,
                        InMemoryStrings.LogTransactionsNotSupported.GenerateMessage(),
                        "InMemoryEventId.TransactionIgnoredWarning"),
                    Assert.Throws<InvalidOperationException>(
                        () => context.Database.EnlistTransaction(new CommittableTransaction())).Message);
            }
        }

        [Fact]
        public void Should_not_throw_by_default_when_transaction_and_ignored()
        {
            var optionsBuilder
                = new DbContextOptionsBuilder()
                    .ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning))
                    .EnableServiceProviderCaching(false)
                    .UseInMemoryDatabase(Guid.NewGuid().ToString());

            using (var context = new DbContext(optionsBuilder.Options))
            {
                context.Database.BeginTransaction();
            }
        }

        [Fact]
        public void Throws_when_warning_as_error_all()
        {
            var serviceProvider = new ServiceCollection()
                .AddEntityFrameworkInMemoryDatabase()
                .BuildServiceProvider();

            using (var context = new WarningAsErrorContext(serviceProvider))
            {
                Assert.Equal(
                    CoreStrings.WarningAsErrorTemplate(
                        CoreEventId.FirstWithoutOrderByAndFilterWarning.ToString(),
                        CoreStrings.LogFirstWithoutOrderByAndFilter.GenerateMessage(
                            "(from WarningAsErrorEntity <generated>_1 in DbSet<WarningAsErrorEntity> select [<generated>_1]).Firs..."),
                        "CoreEventId.FirstWithoutOrderByAndFilterWarning"),
                    Assert.Throws<InvalidOperationException>(
                        () => context.WarningAsErrorEntities.FirstOrDefault()).Message);
            }
        }

        [Fact]
        public void Throws_when_warning_as_error_specific()
        {
            var serviceProvider = new ServiceCollection()
                .AddEntityFrameworkInMemoryDatabase()
                .BuildServiceProvider();

            using (var context = new WarningAsErrorContext(serviceProvider, toThrow: CoreEventId.FirstWithoutOrderByAndFilterWarning))
            {
                Assert.Equal(
                    CoreStrings.WarningAsErrorTemplate(
                        CoreEventId.FirstWithoutOrderByAndFilterWarning.ToString(),
                        CoreStrings.LogFirstWithoutOrderByAndFilter.GenerateMessage(
                            "(from WarningAsErrorEntity <generated>_1 in DbSet<WarningAsErrorEntity> select [<generated>_1]).Firs..."),
                        "CoreEventId.FirstWithoutOrderByAndFilterWarning"),
                    Assert.Throws<InvalidOperationException>(
                        () => context.WarningAsErrorEntities.FirstOrDefault()).Message);
            }
        }

        [Fact]
        public void Logs_by_default_for_ignored_includes()
        {
            var loggerFactory = new ListLoggerFactory();

            var serviceProvider = new ServiceCollection()
                .AddEntityFrameworkInMemoryDatabase()
                .AddSingleton<ILoggerFactory>(loggerFactory)
                .BuildServiceProvider();

            using (var context = new WarningAsErrorContext(serviceProvider, defaultThrow: false))
            {
                var _ = context.WarningAsErrorEntities.Include(e => e.Nav).OrderBy(e => e.Id).Select(e => e.Id).ToList();

                Assert.Contains(CoreStrings.LogIgnoredInclude.GenerateMessage("[e].Nav"), loggerFactory.Log.Select(l => l.Message));
            }
        }

        [Fact]
        public void Ignored_includes_can_be_configured_to_throw()
        {
            var serviceProvider = new ServiceCollection()
                .AddEntityFrameworkInMemoryDatabase()
                .BuildServiceProvider();

            using (var context = new WarningAsErrorContext(serviceProvider, toThrow: CoreEventId.IncludeIgnoredWarning))
            {
                Assert.Equal(
                    CoreStrings.WarningAsErrorTemplate(
                        CoreEventId.IncludeIgnoredWarning.ToString(),
                        CoreStrings.LogIgnoredInclude.GenerateMessage("[e].Nav"),
                        "CoreEventId.IncludeIgnoredWarning"),
                    Assert.Throws<InvalidOperationException>(
                        ()
                            => context.WarningAsErrorEntities.Include(e => e.Nav).OrderBy(e => e.Id).Select(e => e.Id).ToList()).Message);
            }
        }

        [Fact]
        public void Throws_by_default_for_lazy_load_with_disposed_context()
        {
            var loggerFactory = new ListLoggerFactory();

            var serviceProvider = new ServiceCollection()
                .AddEntityFrameworkInMemoryDatabase()
                .AddSingleton<ILoggerFactory>(loggerFactory)
                .BuildServiceProvider();

            using (var context = new WarningAsErrorContext(serviceProvider, defaultThrow: false))
            {
                context.Add(
                    new WarningAsErrorEntity
                    {
                        Nav = new IncludedEntity()
                    });
                context.SaveChanges();
            }

            WarningAsErrorEntity entity;

            using (var context = new WarningAsErrorContext(serviceProvider, defaultThrow: false))
            {
                entity = context.WarningAsErrorEntities.OrderBy(e => e.Id).First();
            }

            Assert.Equal(
                CoreStrings.WarningAsErrorTemplate(
                    CoreEventId.LazyLoadOnDisposedContextWarning.ToString(),
                    CoreStrings.LogLazyLoadOnDisposedContext.GenerateMessage("Nav", "WarningAsErrorEntity"),
                    "CoreEventId.LazyLoadOnDisposedContextWarning"),
                Assert.Throws<InvalidOperationException>(
                    () => entity.Nav).Message);
        }

        [Fact]
        public void Lazy_load_with_disposed_context_can_be_configured_to_log()
        {
            var loggerFactory = new ListLoggerFactory();

            var serviceProvider = new ServiceCollection()
                .AddEntityFrameworkInMemoryDatabase()
                .AddSingleton<ILoggerFactory>(loggerFactory)
                .BuildServiceProvider();

            using (var context = new WarningAsErrorContext(
                serviceProvider,
                defaultThrow: false,
                CoreEventId.LazyLoadOnDisposedContextWarning))
            {
                context.Add(
                    new WarningAsErrorEntity
                    {
                        Nav = new IncludedEntity()
                    });
                context.SaveChanges();
            }

            WarningAsErrorEntity entity;

            using (var context = new WarningAsErrorContext(
                serviceProvider,
                defaultThrow: false,
                CoreEventId.LazyLoadOnDisposedContextWarning))
            {
                entity = context.WarningAsErrorEntities.OrderBy(e => e.Id).First();
            }

            Assert.Null(entity.Nav);

            Assert.Contains(
                CoreStrings.LogLazyLoadOnDisposedContext.GenerateMessage("Nav", "WarningAsErrorEntity"),
                loggerFactory.Log.Select(l => l.Message));
        }

        [Fact]
        public void Lazy_loading_is_logged_only_when_actually_loading()
        {
            var loggerFactory = new ListLoggerFactory();

            var serviceProvider = new ServiceCollection()
                .AddEntityFrameworkInMemoryDatabase()
                .AddSingleton<ILoggerFactory>(loggerFactory)
                .BuildServiceProvider();

            using (var context = new WarningAsErrorContext(serviceProvider, defaultThrow: false))
            {
                context.Add(
                    new WarningAsErrorEntity
                    {
                        Nav = new IncludedEntity()
                    });
                context.SaveChanges();
            }

            using (var context = new WarningAsErrorContext(serviceProvider, defaultThrow: false))
            {
                var entity = context.WarningAsErrorEntities.OrderBy(e => e.Id).First();

                loggerFactory.Clear();
                Assert.NotNull(entity.Nav);

                Assert.Contains(
                    CoreStrings.LogNavigationLazyLoading.GenerateMessage("Nav", "WarningAsErrorEntity"),
                    loggerFactory.Log.Select(l => l.Message));

                loggerFactory.Clear();
                Assert.NotNull(entity.Nav);
                Assert.DoesNotContain(
                    CoreStrings.LogNavigationLazyLoading.GenerateMessage("Nav", "WarningAsErrorEntity"),
                    loggerFactory.Log.Select(l => l.Message));
            }
        }

        [Fact]
        public void No_throw_when_event_id_not_registered()
        {
            var serviceProvider = new ServiceCollection()
                .AddEntityFrameworkInMemoryDatabase()
                .BuildServiceProvider();

            using (var context = new WarningAsErrorContext(serviceProvider, toThrow: CoreEventId.SensitiveDataLoggingEnabledWarning))
            {
                // ReSharper disable once ReturnValueOfPureMethodIsNotUsed
                context.WarningAsErrorEntities.FirstOrDefault();
            }
        }

        private class WarningAsErrorContext : DbContext
        {
            private readonly IServiceProvider _serviceProvider;
            private readonly bool _defaultThrow;
            private readonly EventId? _toLog;
            private readonly EventId? _toThrow;

            public WarningAsErrorContext(
                IServiceProvider serviceProvider,
                bool defaultThrow = true,
                EventId? toLog = null,
                EventId? toThrow = null)
            {
                _serviceProvider = serviceProvider;
                _defaultThrow = defaultThrow;
                _toLog = toLog;
                _toThrow = toThrow;
            }

            public DbSet<WarningAsErrorEntity> WarningAsErrorEntities { get; set; }

            protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
                => optionsBuilder
                    .UseInternalServiceProvider(_serviceProvider)
                    .UseInMemoryDatabase(nameof(WarningAsErrorContext)).ConfigureWarnings(
                        c =>
                        {
                            if (_toThrow != null)
                            {
                                c.Throw(_toThrow.Value);
                            }
                            else if (_toLog != null)
                            {
                                c.Log(_toLog.Value);
                            }
                            else if (_defaultThrow)
                            {
                                c.Default(WarningBehavior.Throw);
                            }
                        });
        }

        private class WarningAsErrorEntity
        {
            private readonly Action<object, string> _loader;
            private IncludedEntity _nav;

            public WarningAsErrorEntity()
            {
            }

            private WarningAsErrorEntity(Action<object, string> lazyLoader)
            {
                _loader = lazyLoader;
            }

            public IncludedEntity Nav
            {
                get => _loader.Load(this, ref _nav);
                set => _nav = value;
            }

            public string Id { get; set; }
        }

        private class IncludedEntity
        {
            public int Id { get; set; }
        }
    }
}
