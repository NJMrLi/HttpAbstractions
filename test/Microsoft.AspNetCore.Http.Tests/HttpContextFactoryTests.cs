// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.IO;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.ObjectPool;
using Microsoft.Extensions.Options;
using Xunit;

namespace Microsoft.AspNetCore.Http
{
    public class HttpContextFactoryTests
    {
        [Fact]
        public void CreateHttpContextSetsHttpContextAccessor()
        {
            // Arrange
            var accessor = new HttpContextAccessor();
            var contextFactory = new HttpContextFactory(new DefaultObjectPoolProvider(), Options.Create(new FormOptions()), accessor);

            // Act
            var context = contextFactory.Create(new FeatureCollection());

            // Assert
            Assert.True(ReferenceEquals(context, accessor.HttpContext));
        }

        [Fact]
        public void AllowsCreatingContextWithoutSettingAccessor()
        {
            // Arrange
            var contextFactory = new HttpContextFactory(new DefaultObjectPoolProvider(), Options.Create(new FormOptions()));

            // Act & Assert
            var context = contextFactory.Create(new FeatureCollection());
            contextFactory.Dispose(context);
        }

#if NET46
        private static void DomainFunc()
        {
            var accessor = new HttpContextAccessor();
            Assert.Equal(null, accessor.HttpContext);
            accessor.HttpContext = new DefaultHttpContext();
        }

        [Fact]
        public void ChangingAppDomainsDoesNotBreak()
        {
            // Arrange
            var accessor = new HttpContextAccessor();
            var contextFactory = new HttpContextFactory(new DefaultObjectPoolProvider(), Options.Create(new FormOptions()), accessor);
            var baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
            var setupInfo = new AppDomainSetup
            {
                ApplicationBase = baseDirectory,
                ConfigurationFile = Path.Combine(baseDirectory, Path.GetFileNameWithoutExtension(GetType().Assembly.Location) + ".dll.config"),
            };
            var domain = AppDomain.CreateDomain("newDomain", null, setupInfo);

            // Act
            var context = contextFactory.Create(new FeatureCollection());
            domain.DoCallBack(DomainFunc);
            AppDomain.Unload(domain);

            // Assert
            Assert.True(ReferenceEquals(context, accessor.HttpContext));
        }
#elif NETCOREAPP2_0
#else
#error Target framework needs to be updated
#endif
    }
}