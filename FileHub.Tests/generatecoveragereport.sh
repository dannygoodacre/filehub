#!/bin/bash

rm -rf ./coverage_data/*

dotnet test .. \
    --collect:"XPlat Code Coverage" \
    --results-directory ./coverage_data

reportgenerator \
    -reports:./coverage_data/*/coverage.cobertura.xml \
    -targetdir:coverage_report \
    -reporttypes:Html \
    -assemblyfilters:-FileHub.Tests.Common \
    -classfilters:"-*Query;-*Command;-*ApplicationContextFactory" \
    -filefilters:"\
-**/*.g.cs;\
-**/Program.cs;\
-**/FileHub.Data/Migrations/*.cs;\
-**/FileHub.Web/Extensions/WebApplicationExtensions.cs"

rm -rf ./coverage_data/
