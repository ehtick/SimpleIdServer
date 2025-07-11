﻿// Copyright (c) SimpleIdServer. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
var builder = WebApplication.CreateBuilder(args);

const string connectionString = "Host=localhost;Port=5432;Database=mydatabase;Username=admin;Password=tJWBx3ccNJ6dyp1wxoA99qqQ";
builder.AddSidIdentityServer()
    .AddDeveloperSigningCredential()
    .UseEfStore(e =>
    {
        e.UseSqlite(connectionString, o =>
        {
            o.MigrationsAssembly("SimpleIdServer.IdServer.SqliteMigrations");
            o.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery);
        });
    }, e =>
    {
        e.UseSqlite(connectionString, o =>
        {
            o.MigrationsAssembly("SidFormBuilder.SqliteMigrations");
            o.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery);
        });
    });

var app = builder.Build();
app.Services.SeedData();
app.UseSid();
app.Run();